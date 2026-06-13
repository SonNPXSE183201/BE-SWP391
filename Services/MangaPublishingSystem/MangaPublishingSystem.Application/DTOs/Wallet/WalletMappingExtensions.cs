using System.Collections.Generic;
using System.Linq;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public static class WalletMappingExtensions
    {
        public static WalletDto ToDto(this MangaPublishingSystem.Domain.Entities.Wallet w)
        {
            if (w == null) return null!;

            return new WalletDto
            {
                Id = w.Id,
                UserId = w.UserId,
                SetupFundBalance = w.SetupFundBalance,
                WithdrawableBalance = w.WithdrawableBalance,
                LockedFund = w.LockedFund,
                LockedWithdrawable = w.LockedWithdrawable,
                CreateAt = w.CreateAt,
                UpdateAt = w.UpdateAt
            };
        }

        public static TransactionDto ToDto(this MangaPublishingSystem.Domain.Entities.Transaction t)
        {
            if (t == null) return null!;

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
                AdminNote = t.AdminNote,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }

        public static IEnumerable<TransactionDto> ToDtoList(this IEnumerable<MangaPublishingSystem.Domain.Entities.Transaction> list)
        {
            if (list == null) return Enumerable.Empty<TransactionDto>();
            return list.Select(t => t.ToDto());
        }
    }
}
