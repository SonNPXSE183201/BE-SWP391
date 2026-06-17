using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Wallet;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Wallet
{
    [ApiController]
    [Route("api/admin/reconciliation")]
    [Authorize] // Quyền Admin hoặc System Admin được xác định qua test case hoặc phân quyền chung
    public class ReconciliationController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public ReconciliationController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("import-csv")]
        public async Task<ActionResult<ApiResponse<ReconciliationReportDto>>> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<ReconciliationReportDto>.Failure(400, "File đối soát không hợp lệ hoặc trống."));
            }

            var rows = new List<ReconciliationRow>();
            try
            {
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    // Đọc dòng đầu làm header
                    var headerLine = await reader.ReadLineAsync();
                    
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(',');
                        if (parts.Length < 4) continue;

                        // Định dạng cột: TxnRef, Amount, ResponseCode, PayDate
                        var row = new ReconciliationRow
                        {
                            TxnRef = parts[0].Trim(),
                            Amount = decimal.Parse(parts[1].Trim()),
                            ResponseCode = parts[2].Trim(),
                            PayDate = parts[3].Trim()
                        };
                        rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationReportDto>.Failure(400, $"Lỗi khi đọc file CSV: {ex.Message}"));
            }

            var report = await _walletService.ReconcileTransactionsAsync(rows);
            return Ok(ApiResponse<ReconciliationReportDto>.Success(report, "Thực hiện đối soát giao dịch VNPay thành công."));
        }
    }
}
