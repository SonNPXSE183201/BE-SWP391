-- Chay trong SSMS, DB: MangaPublishing
-- Muc dich: insert data test cho demo Admin APIs (series, assistant pending, giao dich)
-- KHONG commit file nay neu team DB khong cho — chi chay local de test

USE MangaPublishing;
GO

-- 1) Series Board_Approved (chua co hop dong) — test GET series + POST contract
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = N'Demo Series Admin Test')
BEGIN
    INSERT INTO dbo.Series (
        MangakaId, EditorId, Title, Genre, Synopsis,
        EstimatedProductionBudget, ApprovedProductionBudget,
        PublicationSchedule, Status, CreateAt
    )
    VALUES (
        4, 2, N'Demo Series Admin Test', N'Action, Fantasy', N'Truyen demo test hop dong admin',
        5000000, 4500000, N'Weekly', N'Board_Approved', GETUTCDATE()
    );
    PRINT 'Da them Series Board_Approved.';
END
ELSE
    PRINT 'Series demo da ton tai.';
GO

-- 2) Assistant Pending — test GET users + PUT approve
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE UserName = N'assistant_demo01')
BEGIN
    INSERT INTO dbo.[User] (
        RoleId, UserName, PasswordHash, Email, FullName, Status,
        PortfolioUrl, Skills, CreateAt
    )
    VALUES (
        5, N'assistant_demo01',
        N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6',
        N'assistant.demo01@gmail.com', N'Nguyen Van Assistant Demo', N'Pending',
        N'https://portfolio.example.com', N'To mau, Di net', GETUTCDATE()
    );

    DECLARE @assistantId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Wallet (UserId, SetupFundBalance, WithdrawableBalance, LockedFund, LockedWithdrawable)
    VALUES (@assistantId, 0, 0, 0, 0);

    INSERT INTO dbo.AssistantProfile (AssistantId, SpecialtyTags, TotalCompletedTasks, OnTimeRate, DisputeRate, CurrentActiveTasks, AverageRating)
    VALUES (@assistantId, N'To mau, Di net', 0, 0, 0, 0, 0);

    PRINT 'Da them Assistant Pending id=' + CAST(@assistantId AS VARCHAR(10));
END
ELSE
    PRINT 'assistant_demo01 da ton tai.';
GO

-- 3) Giao dich Deposit — test GET reconciliation (tuy chon)
IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE ReferenceCode = N'VNP-DEMO-001')
BEGIN
    DECLARE @walletId INT = (SELECT TOP 1 WalletId FROM dbo.Wallet WHERE UserId = 4);
    IF @walletId IS NOT NULL
    BEGIN
        INSERT INTO dbo.[Transaction] (WalletId, Type, Amount, Status, ReferenceCode, CreateAt)
        VALUES (@walletId, N'Deposit', 100000, N'Success', N'VNP-DEMO-001', GETUTCDATE());
        PRINT 'Da them giao dich demo reconciliation.';
    END
END
GO

-- Kiem tra
SELECT SeriesId, Title, Status FROM dbo.Series WHERE Status IN (N'Board_Approved', N'Approved');
SELECT UserId, UserName, Status FROM dbo.[User] WHERE RoleId = 5 AND Status = N'Pending';
SELECT TOP 5 TransactionId, Type, Amount, Status, ReferenceCode FROM dbo.[Transaction];
