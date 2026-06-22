-- =========================================================================
-- SEED DATA SETUP SCRIPT WITH RESET
-- PROJECT: Manga Creation Workflow & Publishing Management System (MCWPMS)
-- DATABASE NAME: MangaPublishing
-- PASSWORD FOR ALL SEEDED ACCOUNTS: 12345 (BCrypt Hash)
-- DATE: 2026-06-06
-- =========================================================================

USE MangaPublishing;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- -------------------------------------------------------------------------
-- RESET DATA AND RE-ALIGN IDENTITIES
-- -------------------------------------------------------------------------
PRINT 'Resetting existing data in all tables...';

-- Disable check constraints and foreign keys to safely clean data
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

-- Clean all tables
DELETE FROM dbo.PortfolioSample;
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
DELETE FROM dbo.RefreshToken;
DELETE FROM dbo.[User];
DELETE FROM dbo.Role;

-- Re-enable check constraints and foreign keys
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- Reset identity column seeds
DBCC CHECKIDENT ('dbo.PortfolioSample', RESEED, 0);
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
DBCC CHECKIDENT ('dbo.RefreshToken', RESEED, 0);
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
INSERT INTO dbo.[User] (UserId, RoleId, UserName, PasswordHash, Email, FullName, Status, CreateAt, PenName, PortfolioUrl, Skills, AssignedEditorId) VALUES
(1, 1, 'admin', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'admin@mangapublishing.com', N'Nguyễn Văn Admin', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(2, 2, 'editor1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'editor.tran@mangapublishing.com', N'Trần Thị Biên Tập', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(3, 3, 'board1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board.le@mangapublishing.com', N'Lê Văn Hội Đồng', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(4, 4, 'mangaka1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'mangaka.nam@gmail.com', N'Phan Hoàng Nam', N'Active', GETUTCDATE(), N'NamArt', NULL, NULL, 2),
(5, 5, 'assistant1', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'assistant.son@gmail.com', N'Nguyễn Sơn', N'Active', GETUTCDATE(), NULL, N'https://portfolio.nguyenson.com', N'Vẽ nền, Đi nét, Tô màu', NULL),
(6, 5, 'assistant_pending', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'assistant.pending@gmail.com', N'Nguyễn Văn Chờ Duyệt', N'Pending', GETUTCDATE(), NULL, N'https://portfolio.com/pending', N'Coloring, Lineart', NULL),
(7, 3, 'board2', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board2@mangapublishing.com', N'Nguyễn Văn Hội Đồng 2', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(8, 3, 'board3', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board3@mangapublishing.com', N'Trần Thị Hội Đồng 3', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(9, 3, 'board4', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board4@mangapublishing.com', N'Lê Thị Hội Đồng 4', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(10, 3, 'board5', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board5@mangapublishing.com', N'Phạm Văn Hội Đồng 5', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL),
(11, 3, 'board6', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board6@mangapublishing.com', N'Vũ Văn Hội Đồng 6', N'Active', GETUTCDATE(), NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT dbo.[User] OFF;
GO

-- 3. Insert Wallets for Users
SET IDENTITY_INSERT dbo.Wallet ON;
INSERT INTO dbo.Wallet (WalletId, UserId, SetupFundBalance, WithdrawableBalance, LockedFund, LockedWithdrawable) VALUES
(1, 1, 0.00, 0.00, 0.00, 0.00),
(2, 2, 0.00, 0.00, 0.00, 0.00),
(3, 3, 0.00, 0.00, 0.00, 0.00),
(4, 4, 8800000.00, 5000000.00, 1200000.00, 0.00), -- Mangaka setup fund (10M total, 1.2M locked in escrow)
(5, 5, 0.00, 1500000.00, 0.00, 0.00),      -- Assistant withdrawable balance
(6, 6, 0.00, 0.00, 0.00, 0.00),            -- Pending Assistant wallet
(7, 7, 0.00, 0.00, 0.00, 0.00),
(8, 8, 0.00, 0.00, 0.00, 0.00),
(9, 9, 0.00, 0.00, 0.00, 0.00),
(10, 10, 0.00, 0.00, 0.00, 0.00),
(11, 11, 0.00, 0.00, 0.00, 0.00);
SET IDENTITY_INSERT dbo.Wallet OFF;
GO

-- 3b. Insert Transactions
SET IDENTITY_INSERT dbo.[Transaction] ON;
INSERT INTO dbo.[Transaction] (TransactionId, WalletId, Type, ReferenceId, SetupFundAmount, WithdrawableAmount, Amount, Status, FromUserId, CreateAt) VALUES
(1, 4, N'Escrow_Lock', 3, -600000.00, 0.00, 600000.00, N'Success', 4, GETUTCDATE()),
(2, 4, N'Escrow_Lock', 4, -250000.00, 0.00, 250000.00, N'Success', 4, GETUTCDATE()),
(3, 4, N'Escrow_Lock', 5, -150000.00, 0.00, 150000.00, N'Success', 4, GETUTCDATE()),
(4, 4, N'Escrow_Lock', 6, -200000.00, 0.00, 200000.00, N'Success', 4, GETUTCDATE());
SET IDENTITY_INSERT dbo.[Transaction] OFF;
GO

-- 4. Insert AssistantProfile for Assistant user
SET IDENTITY_INSERT dbo.AssistantProfile ON;
INSERT INTO dbo.AssistantProfile (ProfileId, AssistantId, SpecialtyTags, TotalCompletedTasks, OnTimeRate, DisputeRate, CurrentActiveTasks, AverageRating) VALUES
(1, 5, N'Background, Lineart, Coloring', 12, 95.80, 0.00, 1, 4.85),
(2, 6, N'Coloring, Lineart', 0, 0.00, 0.00, 0, 0.00);
SET IDENTITY_INSERT dbo.AssistantProfile OFF;
GO

-- 5. Insert Series
SET IDENTITY_INSERT dbo.Series ON;
INSERT INTO dbo.Series (SeriesId, MangakaId, EditorId, Title, Genre, Synopsis, CoverArtworkUrl, EstimatedProductionBudget, ApprovedProductionBudget, PublicationSchedule, Status, ResourceFolderUrl, CreateAt) VALUES
(1, 4, 2, N'Hành Trình Kỳ Thú', N'Shounen, Phiêu lưu', N'Câu chuyện phiêu lưu kỳ thú của một cậu bé đi tìm kho báu cổ xưa.', N'https://storage.mangapublishing.com/covers/hanh-trinh-ky-thu.jpg', 50000000.00, 45000000.00, N'Weekly', N'In Production', N'https://storage.mangapublishing.com/resources/series-1', GETUTCDATE()),
(2, 4, 2, N'Học Viện Siêu Nhiên', N'Comedy, Fantasy', N'Cuộc sống học đường dở khóc dở cười tại một ngôi trường đặc biệt.', N'https://storage.mangapublishing.com/covers/hoc-vien-sieu-nhien.jpg', 30000000.00, 0.00, NULL, N'Pending_Approval', N'https://storage.mangapublishing.com/resources/series-2', GETUTCDATE());
SET IDENTITY_INSERT dbo.Series OFF;
GO

-- 6. Insert Contracts
SET IDENTITY_INSERT dbo.Contract ON;
INSERT INTO dbo.Contract (ContractId, UserId, SeriesId, BaseGenkouryoPrice, SignedDate, Status, CreateAt) VALUES
(1, 4, 1, 500000.00, GETUTCDATE(), N'Signed', GETUTCDATE()),
(2, 4, 2, 400000.00, NULL, N'Pending', GETUTCDATE());
SET IDENTITY_INSERT dbo.Contract OFF;
GO

-- 7. Insert Chapters
SET IDENTITY_INSERT dbo.Chapter ON;
INSERT INTO dbo.Chapter (ChapterId, SeriesId, ChapterNumber, Title, ValidPageCount, AppliedGenkouryoPrice, SubmissionDeadline, QcChecklistData, Status, CreateAt) VALUES
(1, 1, 1, N'Khởi đầu cuộc hành trình', 3, 500000.00, DATEADD(day, 10, GETUTCDATE()), N'{"Technical": "Passed", "Art": "Passed", "Content": "Passed"}', N'Approved', GETUTCDATE()),
(2, 1, 2, N'Gặp gỡ đồng đội mới', 2, 500000.00, DATEADD(day, 20, GETUTCDATE()), NULL, N'Draft', GETUTCDATE());
SET IDENTITY_INSERT dbo.Chapter OFF;
GO

-- 8. Insert Pages
SET IDENTITY_INSERT dbo.Page ON;
INSERT INTO dbo.Page (PageId, ChapterId, PageNumber, RawImageUrl, CompositeImageUrl, BaseLayerUrl, Status, IsApproved, CreateAt) VALUES
(1, 1, 1, N'https://storage.mangapublishing.com/series-1/chap-1/page-1-raw.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-1-composite.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-1-base.jpg', N'Composited', 1, GETUTCDATE()),
(2, 1, 2, N'https://storage.mangapublishing.com/series-1/chap-1/page-2-raw.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-2-composite.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-2-base.jpg', N'Composited', 1, GETUTCDATE()),
(3, 1, 3, N'https://storage.mangapublishing.com/series-1/chap-1/page-3-raw.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-3-composite.jpg', N'https://storage.mangapublishing.com/series-1/chap-1/page-3-base.jpg', N'Composited', 1, GETUTCDATE()),
(4, 2, 1, N'https://storage.mangapublishing.com/series-1/chap-2/page-1-raw.jpg', NULL, N'https://storage.mangapublishing.com/series-1/chap-2/page-1-base.jpg', N'Pending', 0, GETUTCDATE()),
(5, 2, 2, N'https://storage.mangapublishing.com/series-1/chap-2/page-2-raw.jpg', NULL, N'https://storage.mangapublishing.com/series-1/chap-2/page-2-base.jpg', N'Pending', 0, GETUTCDATE());
SET IDENTITY_INSERT dbo.Page OFF;
GO

-- 9. Insert Regions
SET IDENTITY_INSERT dbo.Region ON;
INSERT INTO dbo.Region (RegionId, PageId, CoordinatesJson, Name, CreateAt) VALUES
(1, 1, N'{"top": 100, "left": 150, "width": 300, "height": 200}', N'Khung cảnh nền núi', GETUTCDATE()),
(2, 1, N'{"top": 400, "left": 100, "width": 250, "height": 300}', N'Nhân vật chính', GETUTCDATE()),
(3, 4, N'{"top": 50, "left": 50, "width": 400, "height": 300}', N'Phân cảnh nền phòng học', GETUTCDATE()),
(4, 4, N'{"top": 400, "left": 200, "width": 200, "height": 250}', N'Hiệu ứng hành động', GETUTCDATE());
SET IDENTITY_INSERT dbo.Region OFF;
GO

-- 10. Insert Tasks
SET IDENTITY_INSERT dbo.Tasks ON;
INSERT INTO dbo.Tasks (TaskId, MangakaId, RegionId, AssistantId, Description, PaymentAmount, Deadline, ExtensionRequestDays, ExtensionReason, ExtensionStatus, ZIndex_Order, Status, Rating, FeedbackComment, CreateAt) VALUES
(1, 4, 1, 5, N'Vẽ chi tiết hậu cảnh đồi núi đá vôi buổi hoàng hôn', 500000.00, DATEADD(day, -2, GETUTCDATE()), NULL, NULL, N'None', 1, N'Approved', 5, N'Hậu cảnh vẽ rất đẹp và chi tiết, đúng phong cách.', GETUTCDATE()),
(2, 4, 2, 5, N'Vẽ và tô màu trang phục nhân vật chính', 300000.00, DATEADD(day, -1, GETUTCDATE()), NULL, NULL, N'None', 2, N'Approved', 4, N'Màu sắc tươi sáng, đúng yêu cầu.', GETUTCDATE()),
(3, 4, 3, 5, N'Vẽ nền phòng học có bàn ghế và cửa sổ lớn', 600000.00, DATEADD(day, 3, GETUTCDATE()), NULL, NULL, N'None', 1, N'Submitted', NULL, NULL, GETUTCDATE()),
(4, 4, 4, 5, N'Vẽ hiệu ứng các tia chớp bao quanh', 250000.00, DATEADD(day, 5, GETUTCDATE()), NULL, NULL, N'None', 2, N'In_Progress', NULL, NULL, GETUTCDATE()),
(5, 4, 3, 5, N'Vẽ chi tiết các đồ vật nhỏ trên bàn học', 150000.00, DATEADD(day, 4, GETUTCDATE()), NULL, NULL, N'None', 1, N'Submitted', NULL, NULL, GETUTCDATE()),
(6, 4, 3, 5, N'Vẽ bóng đổ cho khung cảnh phòng học', 200000.00, DATEADD(day, 2, GETUTCDATE()), 3, N'Cần thêm thời gian nghiên cứu phối cảnh phức tạp', N'Pending', 1, N'In_Progress', NULL, NULL, GETUTCDATE());
SET IDENTITY_INSERT dbo.Tasks OFF;
GO

-- 11. Insert Task Versions
SET IDENTITY_INSERT dbo.TaskVersion ON;
INSERT INTO dbo.TaskVersion (VersionId, TaskId, VersionNumber, SubmittedFileUrl, Status, SubmittedAt, CreateAt) VALUES
(1, 1, 1, N'https://storage.mangapublishing.com/tasks/task-1-v1.png', N'Approved', DATEADD(day, -3, GETUTCDATE()), GETUTCDATE()),
(2, 2, 1, N'https://storage.mangapublishing.com/tasks/task-2-v1.png', N'Approved', DATEADD(day, -2, GETUTCDATE()), GETUTCDATE()),
(3, 3, 1, N'https://storage.mangapublishing.com/tasks/task-3-v1.png', N'Submitted', DATEADD(minute, -30, GETUTCDATE()), GETUTCDATE()),
(4, 5, 1, N'https://storage.mangapublishing.com/tasks/task-5-v1.png', N'Submitted', DATEADD(minute, -20, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT dbo.TaskVersion OFF;
GO

-- 12. Insert Board Votes for Series 2 (Học Viện Siêu Nhiên)
SET IDENTITY_INSERT dbo.BoardVote ON;
INSERT INTO dbo.BoardVote (VoteId, SeriesId, BoardMemberId, VoteType, RecommendedBudget, Comment, VoteAt, CreateAt) VALUES
(1, 2, 3, N'Approve', 32000000.00, N'Cốt truyện hài hước thú vị, phù hợp độc giả trẻ tuổi. Đề xuất duyệt.', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT dbo.BoardVote OFF;
GO

-- 13. Insert Ranking Records for Series 1
SET IDENTITY_INSERT dbo.RankingRecord ON;
INSERT INTO dbo.RankingRecord (RankingId, SeriesId, VoteCount, RankPosition, RecordedDate, CreateAt) VALUES
(1, 1, 120, 5, DATEADD(day, -7, GETUTCDATE()), GETUTCDATE()),
(2, 1, 150, 3, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT dbo.RankingRecord OFF;
GO

-- 14. Insert Notifications
SET IDENTITY_INSERT dbo.Notification ON;
INSERT INTO dbo.Notification (NotId, UserId, Content, Type, IsRead, CreateAt) VALUES
(1, 4, N'Bộ truyện Học Viện Siêu Nhiên đã nhận được 1 phiếu biểu quyết từ Hội đồng biên tập.', N'SeriesVote', 0, GETUTCDATE()),
(2, 5, N'Bạn được giao nhiệm vụ mới: Vẽ nền phòng học có bàn ghế và cửa sổ lớn.', N'TaskAssigned', 0, GETUTCDATE());
SET IDENTITY_INSERT dbo.Notification OFF;
GO

-- 15. Insert Portfolio Samples
SET IDENTITY_INSERT dbo.PortfolioSample ON;
INSERT INTO dbo.PortfolioSample (SampleId, AssistantId, Title, ImageUrl, Category) VALUES
(1, 5, N'Thiết kế nền thành phố cổ', N'https://storage.mangapublishing.com/portfolios/son-bg1.png', N'Background'),
(2, 5, N'Đi nét nhân vật chính', N'https://storage.mangapublishing.com/portfolios/son-line1.png', N'Lineart');
SET IDENTITY_INSERT dbo.PortfolioSample OFF;
GO

PRINT 'Mock data seeded successfully with password ''12345'' for all users.';
GO
