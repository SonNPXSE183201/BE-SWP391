using Microsoft.AspNetCore.Mvc;
using MangaPublishingSystem.Application.DTOs;
using MangaPublishingSystem.Application.IServices;
using BuildingBlocks.Web.Responses;
using System;

namespace MangaPublishingSystem.Presentation.Controllers.Transaction;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    // POST api/v1/transactions/deposit
    [HttpPost("deposit")]
    public async Task<ApiResponse<Guid>> Deposit([FromBody] DepositRequestDto request)
    {
        var transactionId = await _transactionService.CreateDepositAsync(request);
        return ApiResponse<Guid>.Success(transactionId);
    }

    // VNPay callback (form‑urlencoded)
    [HttpPost("callback")]
    public async Task<ApiResponse<object>> VnpayCallback([FromForm] VnpayCallbackDto callback)
    {
        await _transactionService.HandleCallbackAsync(callback);
        return ApiResponse<object>.Success(null);
    }
}
