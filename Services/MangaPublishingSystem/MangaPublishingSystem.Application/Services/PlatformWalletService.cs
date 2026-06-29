using System;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.Common;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Constants;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Services
{
    public class PlatformWalletService : IPlatformWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vnPayService;

        public PlatformWalletService(
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            IVnPayService vnPayService)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _vnPayService = vnPayService;
        }

        public async Task<PlatformWalletDto> GetTreasuryAsync()
        {
            var wallet = await GetOrCreateTreasuryWalletAsync();
            return MapDto(wallet);
        }

        public async Task<string> InitiateTopUpTreasuryAsync(int adminUserId, TopUpPlatformWalletDto dto, string ipAddr)
        {
            if (dto.Amount < 10000)
            {
                throw new BadRequestException("Số tiền nạp vào ví quỹ tối thiểu 10.000 VND.");
            }

            var wallet = await GetOrCreateTreasuryWalletAsync();
            var referenceCode = $"PLT{DateTime.UtcNow:yyyyMMddHHmmss}{adminUserId}";

            await _transactionRepository.AddAsync(new Transaction
            {
                WalletId = wallet.Id,
                Type = SystemWalletConstants.PlatformTopUpType,
                Amount = dto.Amount,
                WithdrawableAmount = dto.Amount,
                Status = "Pending",
                ReferenceCode = referenceCode,
                FromUserId = adminUserId,
                AdminNote = dto.Note
            });

            await _unitOfWork.SaveChangesAsync();

            var orderInfo = $"Nap quy NXB — Admin #{adminUserId}";
            return _vnPayService.BuildPaymentUrl(referenceCode, dto.Amount, ipAddr, orderInfo);
        }

        public async System.Threading.Tasks.Task DisburseProductionFundAsync(
            int seriesId,
            int mangakaId,
            decimal amount,
            Wallet mangakaWallet)
        {
            if (amount <= 0)
            {
                throw new BadRequestException("Số tiền cấp vốn không hợp lệ.");
            }

            var treasury = await GetOrCreateTreasuryWalletAsync();
            if (treasury.WithdrawableBalance < amount)
            {
                throw new BadRequestException(
                    $"Ví quỹ NXB không đủ số dư (còn {treasury.WithdrawableBalance:N0} VND). Admin cần nạp thêm quỹ hệ thống.");
            }

            var referenceCode = $"FUND-S{seriesId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            treasury.WithdrawableBalance -= amount;
            mangakaWallet.SetupFundBalance += amount;
            _walletRepository.Update(treasury);
            _walletRepository.Update(mangakaWallet);

            await _transactionRepository.AddAsync(new Transaction
            {
                WalletId = treasury.Id,
                Type = SystemWalletConstants.ProductionFundingType,
                ReferenceId = seriesId,
                Amount = -amount,
                WithdrawableAmount = -amount,
                Status = "Success",
                ReferenceCode = referenceCode,
                FromUserId = null,
                ToUserId = mangakaId,
                AdminNote = $"Chi cấp vốn sản xuất series #{seriesId}"
            });

            await _transactionRepository.AddAsync(new Transaction
            {
                WalletId = mangakaWallet.Id,
                Type = SystemWalletConstants.ProductionFundingType,
                ReferenceId = seriesId,
                Amount = amount,
                SetupFundAmount = amount,
                Status = "Success",
                ReferenceCode = referenceCode,
                FromUserId = null,
                ToUserId = mangakaId
            });
        }

        private async Task<Wallet> GetOrCreateTreasuryWalletAsync()
        {
            var wallet = await _walletRepository.GetPlatformTreasuryWalletAsync();
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new Wallet
            {
                UserId = null,
                Kind = WalletKinds.PlatformTreasury,
                SetupFundBalance = 0,
                WithdrawableBalance = 0,
                LockedFund = 0,
                LockedWithdrawable = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            return wallet;
        }

        private static PlatformWalletDto MapDto(Wallet wallet) => new()
        {
            Balance = wallet.WithdrawableBalance,
            TreasuryWalletId = wallet.Id
        };
    }
}