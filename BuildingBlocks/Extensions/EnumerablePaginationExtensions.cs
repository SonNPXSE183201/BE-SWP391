using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.Web.Responses;

namespace BuildingBlocks.Extensions
{
    public static class EnumerablePaginationExtensions
    {
        /// <summary>
        /// Phân trang một bộ sưu tập In-Memory (IEnumerable). Cắt kích thước trang tối đa ở mức 50.
        /// </summary>
        public static PagedResult<T> ToPagedList<T>(
            this IEnumerable<T> source,
            int pageNumber,
            int pageSize)
        {
            var totalItems = source.Count();
            
            // Cấu hình tối đa 50 sản phẩm/bản ghi cho 1 trang theo yêu cầu
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>(items, pageNumber, pageSize, totalItems, totalPages);
        }
    }
}
