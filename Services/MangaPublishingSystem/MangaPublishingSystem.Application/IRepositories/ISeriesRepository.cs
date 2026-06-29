using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ISeriesRepository : IGenericRepository<Series>
    {
        Task<bool> HasContractAsync(int seriesId);

        Task<IReadOnlyList<Series>> FindWithDetailsAsync(Expression<Func<Series, bool>> predicate);
    }
}