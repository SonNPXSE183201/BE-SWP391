using System;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAdminReconciliationService
    {
        Task<ReconciliationResponseDto> GetReconciliationAsync(
            DateTime? from,
            DateTime? to,
            string? status,
            string? referenceCode);
    }
}
