using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Lọc IQueryable trực tiếp trên Database SQL Server không phân biệt hoa thường và không phân biệt dấu.
        /// Sử dụng Collation "SQL_Latin1_General_CP1_CI_AI" (Case-Insensitive, Accent-Insensitive) trên SQL Server.
        /// Chỉ dùng cho các truy vấn EF Core được thực thi ở tầng Infrastructure.
        /// </summary>
        public static IQueryable<T> WhereContainsUnsigned<T>(
            this IQueryable<T> source,
            Expression<Func<T, string>> propertySelector,
            string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return source;

            var parameter = propertySelector.Parameters[0];
            var propertyAccess = propertySelector.Body;

            // Tham chiếu tới EF.Functions
            var efFunctionsProp = typeof(EF).GetProperty(nameof(EF.Functions));
            var efFunctionsAccess = Expression.MakeMemberAccess(null, efFunctionsProp!);

            // Tìm phương thức EF.Functions.Collate(string property, string collation)
            var collateMethodInfo = typeof(RelationalDbFunctionsExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == nameof(RelationalDbFunctionsExtensions.Collate) && m.IsGenericMethod);

            if (collateMethodInfo == null)
                throw new InvalidOperationException("Không tìm thấy phương thức EF.Functions.Collate.");

            var collateMethod = collateMethodInfo.MakeGenericMethod(typeof(string));

            var collationConstant = Expression.Constant("SQL_Latin1_General_CP1_CI_AI");
            var collateCall = Expression.Call(null, collateMethod, efFunctionsAccess, propertyAccess, collationConstant);

            // Tìm phương thức string.Contains(string value)
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            if (containsMethod == null)
                throw new InvalidOperationException("Không tìm thấy phương thức string.Contains.");

            var searchTermConstant = Expression.Constant(searchTerm);
            var containsCall = Expression.Call(collateCall, containsMethod, searchTermConstant);

            // Tạo Lambda: x => EF.Functions.Collate(x.Property, "SQL_Latin1_General_CP1_CI_AI").Contains(searchTerm)
            var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);

            return source.Where(lambda);
        }

        /// <summary>
        /// Phân trang một đối tượng IQueryable trên Database sử dụng EF Core.
        /// Giới hạn kích thước trang tối đa ở mức 50.
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalItems = await source.CountAsync(cancellationToken);

            // Cấu hình tối đa 50 sản phẩm/bản ghi cho 1 trang theo yêu cầu
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, pageNumber, pageSize, totalItems, totalPages);
        }
    }
}
