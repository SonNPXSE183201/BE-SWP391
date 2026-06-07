using System;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs;


namespace MangaPublishingSystem.Application.Services
{
    public class TransactionService : GenericService<Transaction>, ITransactionService
    {
        public TransactionService(ITransactionRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _transactionRepository = repository;

        }

        private readonly ITransactionRepository _transactionRepository;


        public async Task<Guid> CreateDepositAsync(DepositRequestDto request)
        {
            var transaction = new VNPayTransaction
            {
                WalletId = request.WalletId,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Pending,
                ReferenceCode = Guid.NewGuid().ToString()
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            return transaction.TransactionId;
        }

        public async System.Threading.Tasks.Task HandleCallbackAsync(VnpayCallbackDto callback)
        {
            var transaction = await _transactionRepository.GetByReferenceAsync(callback.Vnp_TxnRef);
            if (transaction == null) return;

            transaction.Status = callback.Vnp_ResponseCode == "00" ? TransactionStatus.Success : TransactionStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}