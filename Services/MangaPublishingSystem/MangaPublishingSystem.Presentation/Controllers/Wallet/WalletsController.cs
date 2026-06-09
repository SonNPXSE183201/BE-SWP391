using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Wallet;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Wallet
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<WalletDetailsDto>>> GetMyWalletDetails()
        {
            int userId = CurrentUserId;
            var wallet = await _walletService.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return NotFound(ApiResponse<WalletDetailsDto>.Failure(404, "Không tìm thấy thông tin ví của người dùng."));
            }

            var transactions = await _walletService.GetTransactionHistoryAsync(userId);

            var walletDto = MapToWalletDto(wallet);
            var transactionDtos = transactions.Select(MapToTransactionDto).ToList();

            var result = new WalletDetailsDto
            {
                Wallet = walletDto,
                Transactions = transactionDtos
            };

            return Ok(ApiResponse<WalletDetailsDto>.Success(result, "Lấy thông tin ví thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("deposit")]
        public async Task<ActionResult<ApiResponse<string>>> Deposit([FromBody] DepositRequestDto depositDto)
        {
            int userId = CurrentUserId;
            var redirectUrl = await _walletService.DepositAsync(userId, depositDto.Amount);
            return Ok(ApiResponse<string>.Success(redirectUrl, "Khởi tạo giao dịch nạp tiền thành công. Vui lòng thanh toán qua liên kết."));
        }

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpPost("withdraw")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> Withdraw([FromBody] WithdrawRequestDto withdrawDto)
        {
            int userId = CurrentUserId;
            var transaction = await _walletService.WithdrawAsync(
                userId, 
                withdrawDto.Amount, 
                withdrawDto.BankName, 
                withdrawDto.BankAccountNumber, 
                withdrawDto.BankAccountName);

            var result = MapToTransactionDto(transaction);
            return Ok(ApiResponse<TransactionDto>.Success(result, "Yêu cầu rút tiền đã được thực hiện thành công."));
        }

        [HttpGet("deposit/callback")]
        public async Task<ActionResult<ApiResponse<bool>>> DepositCallback([FromQuery] string referenceCode, [FromQuery] string status)
        {
            var isSuccess = await _walletService.ConfirmDepositAsync(referenceCode, status);
            var message = isSuccess ? "Nạp tiền thành công." : "Giao dịch nạp tiền thất bại hoặc bị hủy.";
            return Ok(ApiResponse<bool>.Success(isSuccess, message));
        }

        [HttpGet("deposit/checkout-mock")]
        public IActionResult GetCheckoutMockPage([FromQuery] string referenceCode, [FromQuery] decimal amount)
        {
            // Trả về một trang HTML giả lập VNPay Sandbox đẹp mắt và chuyên nghiệp
            var html = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Cổng Thanh Toán Giả Lập VNPay Sandbox</title>
    <link href=""https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;600;800&display=swap"" rel=""stylesheet"">
    <style>
        body {{
            font-family: 'Outfit', sans-serif;
            background: linear-gradient(135deg, #0f172a 0%, #1e1b4b 100%);
            color: #f8fafc;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            padding: 20px;
        }}
        .card {{
            background: rgba(30, 41, 59, 0.7);
            backdrop-filter: blur(16px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 24px;
            padding: 40px;
            width: 100%;
            max-width: 480px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.3);
            text-align: center;
            box-sizing: border-box;
        }}
        .logo {{
            font-size: 32px;
            font-weight: 800;
            background: linear-gradient(to right, #38bdf8, #818cf8);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 24px;
        }}
        .amount-box {{
            background: rgba(15, 23, 42, 0.5);
            border-radius: 16px;
            padding: 20px;
            margin: 24px 0;
            border: 1px solid rgba(255, 255, 255, 0.05);
        }}
        .label {{
            font-size: 14px;
            color: #94a3b8;
            text-transform: uppercase;
            letter-spacing: 1.5px;
            margin-bottom: 8px;
        }}
        .amount {{
            font-size: 36px;
            font-weight: 800;
            color: #38bdf8;
        }}
        .reference {{
            font-family: monospace;
            font-size: 16px;
            color: #e2e8f0;
            background: rgba(255,255,255,0.05);
            padding: 4px 8px;
            border-radius: 6px;
        }}
        .btn {{
            display: block;
            width: 100%;
            padding: 16px;
            border: none;
            border-radius: 14px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-bottom: 12px;
            text-decoration: none;
        }}
        .btn-success {{
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            box-shadow: 0 4px 14px rgba(16, 185, 129, 0.3);
        }}
        .btn-success:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(16, 185, 129, 0.4);
        }}
        .btn-fail {{
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            box-shadow: 0 4px 14px rgba(239, 68, 68, 0.3);
        }}
        .btn-fail:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(239, 68, 68, 0.4);
        }}
        .footer {{
            margin-top: 24px;
            font-size: 13px;
            color: #64748b;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""logo"">VNPAY SANDBOX</div>
        <p style=""color: #cbd5e1;"">Bạn đang thực hiện nạp tiền vào ví hệ thống MCWPMS</p>
        
        <div class=""amount-box"">
            <div class=""label"">Số tiền thanh toán</div>
            <div class=""amount"">{amount:N0} VND</div>
        </div>

        <div style=""margin-bottom: 32px; text-align: left; font-size: 15px; color: #cbd5e1;"">
            <div style=""margin-bottom: 8px;""><strong>Mã giao dịch:</strong> <span class=""reference"">{referenceCode}</span></div>
            <div><strong>Nội dung:</strong> Nạp tiền ví hệ thống</div>
        </div>

        <a href=""http://localhost:5000/api/v1/wallets/deposit/callback?referenceCode={referenceCode}&status=Success"" class=""btn btn-success"">Xác nhận THANH TOÁN THÀNH CÔNG</a>
        <a href=""http://localhost:5000/api/v1/wallets/deposit/callback?referenceCode={referenceCode}&status=Failed"" class=""btn btn-fail"">Huỷ giao dịch / THANH TOÁN THẤT BẠI</a>
        
        <div class=""footer"">
            Đây là trang cổng thanh toán giả lập dành riêng cho mục đích thử nghiệm hệ thống.
        </div>
    </div>
</body>
</html>";
            return Content(html, "text/html");
        }

        private static WalletDto MapToWalletDto(MangaPublishingSystem.Domain.Entities.Wallet wallet)
        {
            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                SetupFundBalance = wallet.SetupFundBalance,
                WithdrawableBalance = wallet.WithdrawableBalance,
                LockedFund = wallet.LockedFund,
                LockedWithdrawable = wallet.LockedWithdrawable,
                CreateAt = wallet.CreateAt,
                UpdateAt = wallet.UpdateAt
            };
        }

        private static TransactionDto MapToTransactionDto(MangaPublishingSystem.Domain.Entities.Transaction t)
        {
            return new TransactionDto
            {
                Id = t.Id,
                WalletId = t.WalletId,
                Type = t.Type,
                ReferenceId = t.ReferenceId,
                SetupFundAmount = t.SetupFundAmount,
                WithdrawableAmount = t.WithdrawableAmount,
                Amount = t.Amount,
                Status = t.Status,
                ReferenceCode = t.ReferenceCode,
                FromUserId = t.FromUserId,
                ToUserId = t.ToUserId,
                FromUserName = t.FromUser?.UserName,
                FromUserFullName = t.FromUser?.FullName,
                ToUserName = t.ToUser?.UserName,
                ToUserFullName = t.ToUser?.FullName,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }
    }
}
