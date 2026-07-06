using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.Rankings;

namespace MangaPublishingSystem.Application.Services
{
    public class RankingRecordService : GenericService<RankingRecord>, IRankingRecordService
    {
        public RankingRecordService(IRankingRecordRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }

        public async System.Threading.Tasks.Task CreateRankingsAsync(CreateRankingsDto dto)
        {
            var orderedRecords = dto.Records.OrderByDescending(r => r.VoteCount).ToList();
            for (int i = 0; i < orderedRecords.Count; i++)
            {
                var record = orderedRecords[i];
                var rankingRecord = new RankingRecord
                {
                    SeriesId = record.SeriesId,
                    VoteCount = record.VoteCount,
                    RankPosition = i + 1,
                    RecordedDate = dto.RecordedDate
                };
                await _repository.AddAsync(rankingRecord);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<RankingRecord>> GetRankingsByPeriodAsync(DateTime period)
        {
            return await _repository.FindAsync(r => r.RecordedDate.Date == period.Date, r => r.Series);
        }
    }
}