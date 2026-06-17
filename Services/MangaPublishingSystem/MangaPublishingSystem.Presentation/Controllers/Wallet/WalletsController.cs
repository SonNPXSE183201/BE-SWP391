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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Wallet
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WalletsController(
            IWalletService walletService,
            IVnPayService vnPayService,
            IHttpContextAccessor httpContextAccessor)
        {
            _walletService = walletService;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
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

            // Lấy IP của người dùng
            var ipAddr = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddr == "::1") ipAddr = "127.0.0.1"; // IPv6 loopback → IPv4

            var redirectUrl = await _walletService.DepositAsync(userId, depositDto.Amount, ipAddr);
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
            return Ok(ApiResponse<TransactionDto>.Success(result, "Yêu cầu rút tiền thành công. Vui lòng chờ quản trị viên phê duyệt."));
        }

        [Authorize(Roles = "System Admin")]
        [HttpGet("withdraw/pending")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetPendingWithdrawals()
        {
            var transactions = await _walletService.GetPendingWithdrawalsAsync();
            var result = transactions.Select(MapToTransactionDto).ToList();
            return Ok(ApiResponse<List<TransactionDto>>.Success(result, "Lấy danh sách yêu cầu rút tiền thành công."));
        }

        [Authorize(Roles = "System Admin")]
        [HttpPost("withdraw/{id}/approve")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> ApproveWithdraw(int id, [FromBody] ApproveWithdrawRequestDto dto)
        {
            var transaction = await _walletService.ApproveWithdrawAsync(id, dto.IsApproved, dto.AdminNote);
            var result = MapToTransactionDto(transaction);
            
            var msg = dto.IsApproved ? "Phê duyệt rút tiền thành công." : "Từ chối rút tiền thành công. Tiền đã được hoàn lại.";
            return Ok(ApiResponse<TransactionDto>.Success(result, msg));
        }

        /// <summary>
        /// ReturnUrl: VNPay redirect trình duyệt người dùng về sau khi thanh toán.
        /// Xác minh chữ ký HMAC-SHA512, kiểm tra ResponseCode + TransactionStatus,
        /// cập nhật DB (idempotent) và trả về đầy đủ thông tin để FE hiển thị kết quả.
        /// Logic theo chuẩn code mẫu VNPay:
        ///   - Chữ ký hợp lệ: kiểm tra ResponseCode=00 VÀ TransactionStatus=00
        ///   - Hiển thị: TerminalID, TxnRef, TransactionNo, Amount, BankCode
        /// </summary>
        [HttpGet("deposit/return")]
        public async Task<ActionResult<ApiResponse<VnpayPaymentResultDto>>> DepositReturn([FromQuery] VnpayReturnDto dto)
        {
            var query = HttpContext.Request.Query;

            // Trích xuất các thông tin hiển thị cho người dùng (theo code mẫu VNPay)
            query.TryGetValue("vnp_TxnRef",             out var txnRef);
            query.TryGetValue("vnp_TransactionNo",       out var transactionNo);
            query.TryGetValue("vnp_Amount",              out var amountStr);
            query.TryGetValue("vnp_BankCode",            out var bankCode);
            query.TryGetValue("vnp_CardType",            out var cardType);
            query.TryGetValue("vnp_PayDate",             out var payDate);
            query.TryGetValue("vnp_ResponseCode",        out var responseCode);
            query.TryGetValue("vnp_TransactionStatus",   out var transactionStatus);
            query.TryGetValue("vnp_OrderInfo",           out var orderInfo);
            query.TryGetValue("vnp_TmnCode",             out var tmnCode);

            // Số tiền thực = vnp_Amount / 100 (VNPay gửi đã nhân 100)
            long.TryParse(amountStr, out var amountRaw);
            var amount = (decimal)(amountRaw / 100);

            // Bước 1: Xác minh chữ ký HMAC-SHA512
            var isSignatureValid = _vnPayService.ValidateCallback(
                query,
                out var referenceCode,
                out var isPaymentResponseSuccess);

            if (!isSignatureValid)
            {
                var invalidResult = new VnpayPaymentResultDto
                {
                    IsSuccess         = false,
                    ReferenceCode     = txnRef.ToString(),
                    Amount            = amount,
                    BankCode          = bankCode,
                    ResponseCode      = responseCode,
                    TransactionStatus = transactionStatus,
                    Message           = "Có lỗi xảy ra trong quá trình xử lý. Chữ ký không hợp lệ."
                };
                return BadRequest(ApiResponse<VnpayPaymentResultDto>.Failure(400, "Chữ ký xác thực không hợp lệ."));
            }

            // Bước 2: Kiểm tra kết quả thanh toán
            // Phải ĐỒNG THỜI: vnp_ResponseCode="00" VÀ vnp_TransactionStatus="00"
            var isTransactionSuccess = isPaymentResponseSuccess
                && transactionStatus.ToString() == "00";

            // Bước 3: Cập nhật DB (idempotent — nếu IPN đã xử lý trước thì bỏ qua)
            try
            {
                var dbStatus = isTransactionSuccess ? "Success" : "Failed";
                await _walletService.ConfirmDepositAsync(referenceCode, dbStatus);
            }
            catch
            {
                // Giao dịch đã được IPN confirm → bỏ qua, không ảnh hưởng response
            }

            // Bước 4: Trả về kết quả đầy đủ cho FE hiển thị (Mã GD, ngân hàng, số tiền...)
            var result = new VnpayPaymentResultDto
            {
                IsSuccess          = isTransactionSuccess,
                ReferenceCode      = txnRef.ToString(),       // Mã giao dịch thanh toán
                VnpayTransactionNo = transactionNo,           // Mã giao dịch tại VNPAY
                Amount             = amount,                  // Số tiền thanh toán (VND)
                BankCode           = bankCode,                // Ngân hàng thanh toán
                CardType           = cardType,
                PayDate            = payDate,
                ResponseCode       = responseCode,
                TransactionStatus  = transactionStatus,
                OrderInfo          = orderInfo,
                Message            = isTransactionSuccess
                    ? "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ."
                    : $"Có lỗi xảy ra trong quá trình xử lý. Mã lỗi: {responseCode}"
            };

            var apiMessage = isTransactionSuccess ? "Nạp tiền thành công." : "Giao dịch nạp tiền thất bại hoặc bị hủy.";
            return Content("<html><body><script>window.close();</script>Thanh toán hoàn tất, đang tự động đóng trang...</body></html>", "text/html");
        }

        /// <summary>
        /// IPN URL: VNPay gọi server-to-server để xác nhận kết quả thanh toán.
        /// Phải trả về đúng format JSON {"RspCode":"xx","Message":"..."} mà VNPay yêu cầu.
        /// Thứ tự kiểm tra theo chuẩn VNPay:
        ///   99 → Không có dữ liệu đầu vào
        ///   97 → Chữ ký không hợp lệ
        ///   01 → Không tìm thấy đơn hàng
        ///   04 → Số tiền không khớp
        ///   02 → Đơn hàng đã được xử lý (idempotency)
        ///   00 → Xác nhận thành công
        /// </summary>
        [HttpGet("deposit/ipn")]
        public async Task<IActionResult> DepositIpn([FromQuery] VnpayReturnDto dto)
        {
            var query = HttpContext.Request.Query;

            // Bước 1: Kiểm tra có dữ liệu đầu vào không
            if (query.Count == 0)
            {
                return Ok(new { RspCode = "99", Message = "Input data required" });
            }

            // Bước 2: Xác minh chữ ký HMAC-SHA512
            var isSignatureValid = _vnPayService.ValidateCallback(
                query,
                out var referenceCode,
                out var isPaymentSuccess);

            if (!isSignatureValid)
            {
                return Ok(new { RspCode = "97", Message = "Invalid signature" });
            }

            // Bước 3: Truy vấn đơn hàng trong DB theo vnp_TxnRef
            var order = await _walletService.GetDepositByReferenceCodeAsync(referenceCode);
            if (order == null)
            {
                return Ok(new { RspCode = "01", Message = "Order not found" });
            }

            // Bước 4: So khớp số tiền — VNPay gửi amount đã nhân 100, chia lại để so sánh
            if (query.TryGetValue("vnp_Amount", out var vnpAmountStr)
                && long.TryParse(vnpAmountStr, out var vnpAmountRaw))
            {
                var vnpAmount = (decimal)(vnpAmountRaw / 100);
                if (order.Amount != vnpAmount)
                {
                    return Ok(new { RspCode = "04", Message = "Invalid amount" });
                }
            }

            // Bước 5: Kiểm tra idempotency — đơn hàng đã được xử lý chưa?
            if (order.Status != "Pending")
            {
                return Ok(new { RspCode = "02", Message = "Order already confirmed" });
            }

            // Bước 6: Xác định kết quả — phải cả vnp_ResponseCode="00" VÀ vnp_TransactionStatus="00"
            query.TryGetValue("vnp_TransactionStatus", out var txnStatus);
            var isTransactionSuccess = isPaymentSuccess
                && txnStatus.ToString() == "00";

            // Bước 7: Cập nhật Database
            try
            {
                var status = isTransactionSuccess ? "Success" : "Failed";
                await _walletService.ConfirmDepositAsync(referenceCode, status);
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception)
            {
                return Ok(new { RspCode = "99", Message = "Unknown Error" });
            }
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
                BankName = t.BankName,
                BankAccountNumber = t.BankAccountNumber,
                BankAccountName = t.BankAccountName,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }
    }
}
