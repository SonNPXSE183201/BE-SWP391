-- =========================================================================
-- SEED DATA SETUP SCRIPT WITH RESET
-- PROJECT: Manga Creation Workflow & Publishing Management System (MCWPMS)
-- DATABASE NAME: MangaPublishing
-- PASSWORD FOR ALL SEEDED ACCOUNTS: 12345 (BCrypt Hash)
-- DATE: 2026-06-06
-- =========================================================================

USE MangaPublishing;
GO

-- -------------------------------------------------------------------------
-- RESET DATA AND RE-ALIGN IDENTITIES
-- -------------------------------------------------------------------------
PRINT 'Resetting existing data in all tables...';

-- Disable check constraints and foreign keys to safely clean data
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

-- Clean all tables
DELETE FROM dbo.Report;
DELETE FROM dbo.Annotation;
DELETE FROM dbo.DisputeLog;
DELETE FROM dbo.TaskVersion;
DELETE FROM dbo.Tasks;
DELETE FROM dbo.Region;
DELETE FROM dbo.Page;
DELETE FROM dbo.Chapter;
DELETE FROM dbo.ContractAddendum;
DELETE FROM dbo.Contract;
DELETE FROM dbo.BoardVote;
DELETE FROM dbo.RankingRecord;
DELETE FROM dbo.Series;
DELETE FROM dbo.Notification;
DELETE FROM dbo.AssistantProfile;
DELETE FROM dbo.[Transaction];
DELETE FROM dbo.Wallet;
DELETE FROM dbo.[User];
DELETE FROM dbo.Role;

-- Re-enable check constraints and foreign keys
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- Reset identity column seeds
DBCC CHECKIDENT ('dbo.Report', RESEED, 0);
DBCC CHECKIDENT ('dbo.Annotation', RESEED, 0);
DBCC CHECKIDENT ('dbo.DisputeLog', RESEED, 0);
DBCC CHECKIDENT ('dbo.TaskVersion', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tasks', RESEED, 0);
DBCC CHECKIDENT ('dbo.Region', RESEED, 0);
DBCC CHECKIDENT ('dbo.Page', RESEED, 0);
DBCC CHECKIDENT ('dbo.Chapter', RESEED, 0);
DBCC CHECKIDENT ('dbo.ContractAddendum', RESEED, 0);
DBCC CHECKIDENT ('dbo.Contract', RESEED, 0);
DBCC CHECKIDENT ('dbo.BoardVote', RESEED, 0);
DBCC CHECKIDENT ('dbo.RankingRecord', RESEED, 0);
DBCC CHECKIDENT ('dbo.Series', RESEED, 0);
DBCC CHECKIDENT ('dbo.Notification', RESEED, 0);
DBCC CHECKIDENT ('dbo.AssistantProfile', RESEED, 0);
DBCC CHECKIDENT ('dbo.[Transaction]', RESEED, 0);
DBCC CHECKIDENT ('dbo.Wallet', RESEED, 0);
DBCC CHECKIDENT ('dbo.[User]', RESEED, 0);
DBCC CHECKIDENT ('dbo.Role', RESEED, 0);
GO

PRINT 'Data reset completed successfully. Starting seed data insertion...';

-- -------------------------------------------------------------------------
-- SEED DATA INSERTION
-- -------------------------------------------------------------------------

-- 1. Insert Roles
SET IDENTITY_INSERT dbo.Role ON;
INSERT INTO dbo.Role (RoleId, RoleName) VALUES
(1, N'System Admin'),
(2, N'Tantou Editor'),
(3, N'Editorial Board'),
(4, N'Mangaka'),
(5, N'Assistant');
SET IDENTITY_INSERT dbo.Role OFF;
GO

-- 2. Insert Vietnamese Users (BCrypt Password Hash of '12345')
SET IDENTITY_INSERT dbo.[User] ON;
INSERT INTO dbo.[User] (UserId, RoleId, UserName, PasswordHash, Email, FullName, Status, CreateAt, PenName, PortfolioUrl, Skills) VALUES
(1, 1, 'admin', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'admin@mangapublishing.com', N'Nguyễn Văn Admin', N'Active', GETUTCDATE(), NULL, NULL, NULL),
(2, 2, 'editor1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'editor.tran@mangapublishing.com', N'Trần Thị Biên Tập', N'Active', GETUTCDATE(), NULL, NULL, NULL),
(3, 3, 'board1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board.le@mangapublishing.com', N'Lê Văn Hội Đồng', N'Active', GETUTCDATE(), NULL, NULL, NULL),
(4, 4, 'mangaka1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'mangaka.nam@gmail.com', N'Phan Hoàng Nam', N'Active', GETUTCDATE(), N'NamArt', NULL, NULL),
(5, 5, 'assistant1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'assistant.son@gmail.com', N'Nguyễn Sơn', N'Active', GETUTCDATE(), NULL, N'https://portfolio.nguyenson.com', N'Vẽ nền, Đi nét, Tô màu');
SET IDENTITY_INSERT dbo.[User] OFF;
GO

-- 3. Insert Wallets for Users
SET IDENTITY_INSERT dbo.Wallet ON;
INSERT INTO dbo.Wallet (WalletId, UserId, SetupFundBalance, WithdrawableBalance, LockedFund, LockedWithdrawable) VALUES
(1, 1, 0.00, 0.00, 0.00, 0.00),
(2, 2, 0.00, 0.00, 0.00, 0.00),
(3, 3, 0.00, 0.00, 0.00, 0.00),
(4, 4, 10000000.00, 5000000.00, 0.00, 0.00), -- Mangaka setup fund and withdrawable balance
(5, 5, 0.00, 1500000.00, 0.00, 0.00);      -- Assistant withdrawable balance
SET IDENTITY_INSERT dbo.Wallet OFF;
GO

-- 4. Insert AssistantProfile for Assistant user
SET IDENTITY_INSERT dbo.AssistantProfile ON;
INSERT INTO dbo.AssistantProfile (ProfileId, AssistantId, SpecialtyTags, TotalCompletedTasks, OnTimeRate, DisputeRate, CurrentActiveTasks, AverageRating) VALUES
(1, 5, N'Background, Lineart, Coloring', 12, 95.80, 0.00, 1, 4.85);
SET IDENTITY_INSERT dbo.AssistantProfile OFF;
GO

PRINT 'Mock data seeded successfully with password ''12345'' for all users.';
GO
