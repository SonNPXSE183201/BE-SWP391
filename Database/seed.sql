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

-- 5. Insert Sample Series for mangaka1 (UserId = 4)
SET IDENTITY_INSERT dbo.Series ON;
INSERT INTO dbo.Series (SeriesId, MangakaId, EditorId, Title, Genre, Synopsis, Status, EstimatedProductionBudget, ApprovedProductionBudget, CreateAt)
VALUES
(1, 4, 2, N'Vùng Đất Lửa', N'Action', N'Bộ truyện về các chiến binh trong thế giới lửa.', N'In_Production', 5000000.00, 5000000.00, GETUTCDATE());
SET IDENTITY_INSERT dbo.Series OFF;
GO

-- 6. Insert Sample Chapter
SET IDENTITY_INSERT dbo.Chapter ON;
INSERT INTO dbo.Chapter (ChapterId, SeriesId, ChapterNumber, Title, Status, CreateAt)
VALUES
(1, 1, 1, N'Chương 1 - Khởi nguồn ngọn lửa', N'In_Progress', GETUTCDATE());
SET IDENTITY_INSERT dbo.Chapter OFF;
GO

-- 7. Insert Sample Pages
SET IDENTITY_INSERT dbo.Page ON;
INSERT INTO dbo.Page (PageId, ChapterId, PageNumber, RawImageUrl, BaseLayerUrl, Status, IsApproved, CreateAt)
VALUES
(1, 1, 1, N'https://picsum.photos/seed/page1/800/1200', N'https://picsum.photos/seed/base1/800/1200', N'Draft', 0, GETUTCDATE()),
(2, 1, 2, N'https://picsum.photos/seed/page2/800/1200', N'https://picsum.photos/seed/base2/800/1200', N'Draft', 0, GETUTCDATE());
SET IDENTITY_INSERT dbo.Page OFF;
GO

-- 8. Insert Sample Regions (drawing zones on pages)
SET IDENTITY_INSERT dbo.Region ON;
INSERT INTO dbo.Region (RegionId, PageId, CoordinatesJson, Name, CreateAt)
VALUES
(1, 1, N'{"x":0,"y":0,"width":800,"height":400}', N'Vùng nền phía trên - Trang 1', GETUTCDATE()),
(2, 1, N'{"x":0,"y":400,"width":800,"height":400}', N'Vùng hiệu ứng lửa - Trang 1', GETUTCDATE()),
(3, 1, N'{"x":200,"y":100,"width":400,"height":800}', N'Vùng đi nét nhân vật - Trang 1', GETUTCDATE()),
(4, 2, N'{"x":0,"y":0,"width":800,"height":600}', N'Vùng tô màu nền - Trang 2', GETUTCDATE()),
(5, 2, N'{"x":100,"y":200,"width":600,"height":800}', N'Vùng bóng đổ - Trang 2', GETUTCDATE());
SET IDENTITY_INSERT dbo.Region OFF;
GO

-- 9. Insert Sample Pending Tasks (Mangaka giao việc cho các Region chưa có người nhận)
-- Mangaka (UserId=4) tạo task với tiền ký quỹ từ ví (ví setup fund có 10,000,000 VND)
SET IDENTITY_INSERT dbo.Tasks ON;
INSERT INTO dbo.Tasks (TaskId, MangakaId, RegionId, AssistantId, Description, PaymentAmount, Deadline, ZIndex_Order, Status, ExtensionStatus, CreateAt)
VALUES
(1, 4, 1, NULL, N'Vẽ nền phía trên trang 1: Phong cảnh núi lửa đang phun trào, tone màu đỏ cam, kỹ thuật tô màu gradient', 500000.00, DATEADD(DAY, 7, GETUTCDATE()), 1, N'Pending', N'None', GETUTCDATE()),
(2, 4, 2, NULL, N'Vẽ hiệu ứng lửa trang 1: Các ngọn lửa bùng cháy xung quanh nhân vật, PNG trong suốt', 750000.00, DATEADD(DAY, 5, GETUTCDATE()), 2, N'Pending', N'None', GETUTCDATE()),
(3, 4, 3, NULL, N'Đi nét nhân vật chính trang 1: Line art sắc nét, độ dày nét 2px, không tô màu', 600000.00, DATEADD(DAY, 10, GETUTCDATE()), 3, N'Pending', N'None', GETUTCDATE()),
(4, 4, 4, 5,    N'Tô màu nền trang 2: Rừng rậm về đêm, tone màu xanh lá tối, kỹ năng coloring', 450000.00, DATEADD(DAY, 6, GETUTCDATE()), 1, N'In_Progress', N'None', GETUTCDATE()),
(5, 4, 5, 5,    N'Vẽ bóng đổ trang 2: Bóng đổ thực tế cho nhân vật theo nguồn sáng từ phía tay phải', 350000.00, DATEADD(DAY, 4, GETUTCDATE()), 2, N'Submitted', N'None', GETUTCDATE());
SET IDENTITY_INSERT dbo.Tasks OFF;
GO

PRINT 'Mock data seeded successfully with password ''12345'' for all users.';
PRINT 'Series/Chapter/Page/Region/Tasks sample data also created.';
GO
