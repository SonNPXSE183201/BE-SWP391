-- =========================================================================
-- SEED DATA — MangaPublishing (UTF-8)
-- CHẠY : sqlcmd -S localhost -f 65001 -i backend/Database/seed.sql
-- YÊU CẦU: Đã chạy schema.sql trước đó.
--
-- Mật khẩu mặc định TẤT CẢ tài khoản bên dưới: 12345
-- PasswordHash BCrypt dùng chung cho mọi user seed.
-- =========================================================================
-- TÀI KHOẢN MẶC ĐỊNH (UserId cố định — không đổi để tránh lệch FK)
-- ┌────────┬───────────────────┬──────────────────┬─────────┬──────────────────────────────┐
-- │ UserId │ UserName          │ RoleId / Vai trò │ Status  │ Email                        │
-- ├────────┼───────────────────┼──────────────────┼─────────┼──────────────────────────────┤
-- │   1    │ admin             │ 1 System Admin   │ Active  │ admin@mangapublishing.com    │
-- │   2    │ editor1           │ 2 Tantou Editor  │ Active  │ editor.tran@mangapublishing.com │
-- │   3    │ board1            │ 3 Editorial Board│ Active  │ board.le@mangapublishing.com │
-- │   4    │ mangaka1          │ 4 Mangaka        │ Active  │ mangaka.nam@gmail.com        │
-- │   5    │ assistant1        │ 5 Assistant      │ Active  │ assistant.son@gmail.com    │
-- │   6    │ assistant_pending │ 5 Assistant      │ Pending │ assistant.pending@gmail.com  │
-- │   7    │ board2            │ 3 Editorial Board│ Active  │ board2@mangapublishing.com   │
-- │   8    │ board3            │ 3 Editorial Board│ Active  │ board3@mangapublishing.com   │
-- │   9    │ board4            │ 3 Editorial Board│ Active  │ board4@mangapublishing.com   │
-- │  10    │ board5            │ 3 Editorial Board│ Active  │ board5@mangapublishing.com   │
-- │  11    │ board6            │ 3 Editorial Board│ Active  │ board6@mangapublishing.com   │
-- └────────┴───────────────────┴──────────────────┴─────────┴──────────────────────────────┘
-- Ví quỹ chung NXB: bảng Wallet (Kind = PlatformTreasury, UserId NULL) — không có user riêng.
--
-- SERIES MẪU (workflow đầy đủ)
--   1 — In Production        : đang sản xuất, đã ký HĐ
--   2 — Fund_Pending         : HĐ đã lập (test Phụ lục HĐ)
--   3 — Fund_Pending         : chờ lập HĐ (test Tạo HĐ)
--   4 — Pending_Approval     : Editor đang duyệt
--   5 — Pending_Board_Vote   : đang biểu quyết Hội đồng
--
-- BẢNG SEED (ngoại trừ __EFMigrationsHistory — do EF quản lý):
--   Role, User, Wallet, Transaction, AssistantProfile, PortfolioSample,
--   Series, Contract, ContractAddendum, Chapter, Page, Region, Tasks,
--   TaskVersion, BoardVote, RankingRecord, Notification,
--   Annotation, DisputeLog, Report, RefreshToken
-- =========================================================================

USE MangaPublishing;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 1: RESET (xóa theo thứ tự phụ thuộc FK, tránh lỗi khi chạy lại)
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 1: Resetting all seed tables...';
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

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
DELETE FROM dbo.BoardVotingConfig;
DELETE FROM dbo.RankingRecord;
DELETE FROM dbo.Series;
DELETE FROM dbo.Notification;
DELETE FROM dbo.PortfolioSample;
DELETE FROM dbo.AssistantProfile;
DELETE FROM dbo.[Transaction];
DELETE FROM dbo.Wallet;
DELETE FROM dbo.RefreshToken;
DELETE FROM dbo.[User];
DELETE FROM dbo.Role;

EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

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
DBCC CHECKIDENT ('dbo.BoardVotingConfig', RESEED, 0);
DBCC CHECKIDENT ('dbo.RankingRecord', RESEED, 0);
DBCC CHECKIDENT ('dbo.Series', RESEED, 0);
DBCC CHECKIDENT ('dbo.Notification', RESEED, 0);
DBCC CHECKIDENT ('dbo.PortfolioSample', RESEED, 0);
DBCC CHECKIDENT ('dbo.AssistantProfile', RESEED, 0);
DBCC CHECKIDENT ('dbo.[Transaction]', RESEED, 0);
DBCC CHECKIDENT ('dbo.Wallet', RESEED, 0);
DBCC CHECKIDENT ('dbo.RefreshToken', RESEED, 0);
DBCC CHECKIDENT ('dbo.[User]', RESEED, 0);
DBCC CHECKIDENT ('dbo.Role', RESEED, 0);
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 2: ROLES
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 2: Seeding roles...';
SET IDENTITY_INSERT dbo.Role ON;
INSERT INTO dbo.Role (RoleId, RoleName) VALUES
(1, N'System Admin'),
(2, N'Tantou Editor'),
(3, N'Editorial Board'),
(4, N'Mangaka'),
(5, N'Assistant');
SET IDENTITY_INSERT dbo.Role OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 2b: BOARD VOTING CONFIG
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 2b: Seeding Board Voting Config...';
SET IDENTITY_INSERT dbo.BoardVotingConfig ON;
INSERT INTO dbo.BoardVotingConfig (ConfigId, AutoResolveHours, ApprovalThresholdPercent, RejectionThresholdPercent, TiePolicy, ClearVotesOnResubmit, RequireOddBoardSize, BoardRoleId, ChairUserId, CreateAt)
VALUES (1, 48, 66, 66, N'Escalate', 1, 1, 3, NULL, GETUTCDATE());
SET IDENTITY_INSERT dbo.BoardVotingConfig OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 3: USERS — 12 tài khoản mặc định
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 3: Seeding default users (password: 12345)...';
SET IDENTITY_INSERT dbo.[User] ON;
INSERT INTO dbo.[User]
    (UserId, RoleId, UserName, PasswordHash, Email, FullName, Status, CreateAt, PenName, PortfolioUrl, Skills, IsOnLeave, AssignedEditorId)
VALUES
( 1, 1, 'admin',             N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'admin@mangapublishing.com',        N'Nguyễn Văn Admin',          N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
( 2, 2, 'editor1',           N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'editor.tran@mangapublishing.com',  N'Trần Thị Biên Tập',        N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
( 3, 3, 'board1',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board.le@mangapublishing.com',     N'Lê Văn Hội Đồng',          N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
( 4, 4, 'mangaka1',          N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'mangaka.nam@gmail.com',            N'Phan Hoàng Nam',           N'Active',  GETUTCDATE(), N'NamArt', NULL,                              NULL,                        0, 2),
( 5, 5, 'assistant1',        N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'assistant.son@gmail.com',          N'Nguyễn Sơn',               N'Active',  GETUTCDATE(), NULL,      N'https://portfolio.nguyenson.com', N'Vẽ nền, Đi nét, Tô màu',  0, NULL),
( 6, 5, 'assistant_pending', N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'assistant.pending@gmail.com',      N'Nguyễn Văn Chờ Duyệt',     N'Pending', GETUTCDATE(), NULL,      N'https://portfolio.com/pending',   N'Coloring, Lineart',        0, NULL),
( 7, 3, 'board2',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board2@mangapublishing.com',       N'Nguyễn Văn Hội Đồng 2',    N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
( 8, 3, 'board3',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board3@mangapublishing.com',       N'Trần Thị Hội Đồng 3',      N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
( 9, 3, 'board4',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board4@mangapublishing.com',       N'Lê Thị Hội Đồng 4',        N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
(10, 3, 'board5',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board5@mangapublishing.com',       N'Phạm Văn Hội Đồng 5',      N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL),
(11, 3, 'board6',            N'$2a$11$MYGlbol73VYbWKwQNlBWeue7YregoBRkXJg2Kji/OOsDL3xrnKeK6', 'board6@mangapublishing.com',       N'Vũ Văn Hội Đồng 6',        N'Active',  GETUTCDATE(), NULL,      NULL,                              NULL,                        0, NULL);
SET IDENTITY_INSERT dbo.[User] OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 4: WALLETS — mỗi user một ví + ví quỹ NXB (không gắn user)
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 4: Seeding wallets...';
SET IDENTITY_INSERT dbo.Wallet ON;
INSERT INTO dbo.Wallet (WalletId, UserId, Kind, SetupFundBalance, WithdrawableBalance, LockedFund, LockedWithdrawable) VALUES
( 1,  1,  N'User',              0.00,      0.00,      0.00, 0.00),
( 2,  2,  N'User',              0.00,      0.00,      0.00, 0.00),
( 3,  3,  N'User',              0.00,      0.00,      0.00, 0.00),
( 4,  4,  N'User',       38800000.00, 9000000.00, 1200000.00, 0.00),
( 5,  5,  N'User',              0.00, 1500000.00,      0.00, 0.00),
( 6,  6,  N'User',              0.00,      0.00,      0.00, 0.00),
( 7,  7,  N'User',              0.00,      0.00,      0.00, 0.00),
( 8,  8,  N'User',              0.00,      0.00,      0.00, 0.00),
( 9,  9,  N'User',              0.00,      0.00,      0.00, 0.00),
(10, 10,  N'User',              0.00,      0.00,      0.00, 0.00),
(11, 11,  N'User',              0.00,      0.00,      0.00, 0.00),
(12, NULL, N'PlatformTreasury', 0.00, 5470000000.00, 0.00, 0.00);
SET IDENTITY_INSERT dbo.Wallet OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 5: TRANSACTIONS
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 5: Seeding transactions...';
SET IDENTITY_INSERT dbo.[Transaction] ON;
INSERT INTO dbo.[Transaction]
    (TransactionId, WalletId, Type, ReferenceId, SetupFundAmount, WithdrawableAmount, Amount, Status, ReferenceCode, FromUserId, ToUserId, CreateAt)
VALUES
(1, 4, N'Escrow_Lock', 3, -600000.00, 0.00, 600000.00, N'Success', NULL, 4, NULL, GETUTCDATE()),
(2, 4, N'Escrow_Lock', 4, -250000.00, 0.00, 250000.00, N'Success', NULL, 4, NULL, GETUTCDATE()),
(3, 4, N'Escrow_Lock', 5, -150000.00, 0.00, 150000.00, N'Success', NULL, 4, NULL, GETUTCDATE()),
(4, 4, N'Escrow_Lock', 6, -200000.00, 0.00, 200000.00, N'Success', NULL, 4, NULL, GETUTCDATE()),
(5, 12, N'Platform_TopUp', NULL, 0, 500000000, 500000000, N'Success', N'PLT202601010000001', 1, NULL, DATEADD(day, -14, GETUTCDATE())),
(6, 4,  N'Deposit', NULL, 0, 2000000, 2000000, N'Success', N'DEP202601020000004', NULL, 4, DATEADD(day, -10, GETUTCDATE())),
(7, 4,  N'Deposit', NULL, 0, 3000000, 3000000, N'Success', N'DEP202601050000004', NULL, 4, DATEADD(day, -7, GETUTCDATE())),
(8, 4,  N'Withdrawal', NULL, 0, -1000000, 1000000, N'Success', N'WDR202601080000004', 4, NULL, DATEADD(day, -3, GETUTCDATE())),
(9, 12, N'Production_Funding', 2, 0, -30000000, -30000000, N'Success', N'FUND-S2-20260109000000', NULL, 4, DATEADD(day, -1, GETUTCDATE())),
(10, 4, N'Production_Funding', 2, 30000000, 0, 30000000, N'Success', N'FUND-S2-20260109000000', NULL, 4, DATEADD(day, -1, GETUTCDATE()));
SET IDENTITY_INSERT dbo.[Transaction] OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 6: ASSISTANT PROFILES & PORTFOLIO
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 6: Seeding assistant profiles & portfolio...';
SET IDENTITY_INSERT dbo.AssistantProfile ON;
INSERT INTO dbo.AssistantProfile
    (ProfileId, AssistantId, SpecialtyTags, TotalCompletedTasks, OnTimeRate, DisputeRate, CurrentActiveTasks, AverageRating)
VALUES
(1, 5, N'Background, Lineart, Coloring', 12, 95.80, 0.00, 1, 4.85),
(2, 6, N'Coloring, Lineart',              0,  0.00, 0.00, 0, 0.00);
SET IDENTITY_INSERT dbo.AssistantProfile OFF;
GO

SET IDENTITY_INSERT dbo.PortfolioSample ON;
INSERT INTO dbo.PortfolioSample (SampleId, AssistantId, Title, ImageUrl, Category) VALUES
(1, 5, N'Thiết kế nền thành phố cổ', N'http://localhost:9000/manga-publishing/portfolios/son-bg1.png',  N'Background'),
(2, 5, N'Đi nét nhân vật chính',    N'http://localhost:9000/manga-publishing/portfolios/son-line1.png', N'Lineart'),
(3, 6, N'Tô màu demo',              N'http://localhost:9000/manga-publishing/portfolios/pending-color1.png', N'Coloring');
SET IDENTITY_INSERT dbo.PortfolioSample OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 7: SERIES — đủ trạng thái workflow
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 7: Seeding series (all workflow states)...';
SET IDENTITY_INSERT dbo.Series ON;
INSERT INTO dbo.Series
    (SeriesId, MangakaId, EditorId, EditorNote, Title, Genre, Synopsis, CoverArtworkUrl,
     EstimatedProductionBudget, ApprovedProductionBudget, PublicationSchedule, Status, ResourceFolderUrl, CreateAt)
VALUES
-- [1] Đang sản xuất
(1, 4, 2, NULL,
 N'Hành Trình Kỳ Thú', N'Shounen, Phiêu lưu',
 N'Câu chuyện phiêu lưu kỳ thú của một cậu bé đi tìm kho báu cổ xưa.',
 N'http://localhost:9000/manga-publishing/covers/hanh-trinh-ky-thu.jpg',
 50000000.00, 45000000.00, N'Weekly', N'In Production',
 N'http://localhost:9000/manga-publishing/resources/series-1', GETUTCDATE()),

-- [2] Fund_Pending + đã có HĐ → test Phụ lục HĐ (Admin /admin/contracts)
(2, 4, 2, NULL,
 N'Học Viện Siêu Nhiên', N'Comedy, Fantasy',
 N'Cuộc sống học đường đầy khó khăn tại một ngôi trường đặc biệt.',
 N'http://localhost:9000/manga-publishing/covers/hoc-vien-sieu-nhien.jpg',
 30000000.00, 30000000.00, NULL, N'Fund_Pending',
 N'http://localhost:9000/manga-publishing/resources/series-2', GETUTCDATE()),

-- [3] Fund_Pending + chưa có HĐ → test Tạo HĐ
(3, 4, 2, NULL,
 N'Tokyo Dreamers', N'Shojo, Romance',
 N'Nhóm bạn trẻ theo đuổi ước mơ tại thành phố Tokyo.',
 N'http://localhost:9000/manga-publishing/covers/tokyo-dreamers.jpg',
 23000000.00, 23000000.00, N'Bi-weekly', N'Fund_Pending',
 N'http://localhost:9000/manga-publishing/resources/series-3', GETUTCDATE()),

-- [4] Editor đang duyệt
(4, 4, 2, N'Cần chỉnh sửa phần mở đầu cho sút hơn.',
 N'Bí Ẩn Midgard', N'Fantasy, Mystery',
 N'Thế giới Midgard ẩn giấu bí mật về nguồn gốc của các chiến binh.',
 N'http://localhost:9000/manga-publishing/covers/bi-an-midgard.jpg',
 28000000.00, 0.00, NULL, N'Pending_Approval',
 N'http://localhost:9000/manga-publishing/resources/series-4', GETUTCDATE()),

-- [5] Đang biểu quyết Hội đồng
(5, 4, 2, NULL,
 N'Cyber Ronin', N'Seinen, Sci-Fi',
 N'Tương lai đen tối nơi samurai cơ khí chiến đấu vì công lý.',
 N'http://localhost:9000/manga-publishing/covers/cyber-ronin.jpg',
 35000000.00, 0.00, N'Weekly', N'Pending_Board_Vote',
 N'http://localhost:9000/manga-publishing/resources/series-5', GETUTCDATE());
SET IDENTITY_INSERT dbo.Series OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 8: CONTRACTS & ADDENDUMS
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 8: Seeding contracts...';
SET IDENTITY_INSERT dbo.Contract ON;
INSERT INTO dbo.Contract (ContractId, UserId, SeriesId, BaseGenkouryoPrice, SignedDate, Status, CreateAt) VALUES
(1, 4, 1, 500000.00, GETUTCDATE(), N'Signed',  GETUTCDATE()),
(2, 4, 2, 400000.00, GETUTCDATE(), N'Active',  GETUTCDATE());
-- Series 3: chưa có HĐ (test Tạo HĐ)
SET IDENTITY_INSERT dbo.Contract OFF;
GO

SET IDENTITY_INSERT dbo.ContractAddendum ON;
INSERT INTO dbo.ContractAddendum (AddendumId, ContractId, NewGenkouryoPrice, EffectiveDate, SignedDate, CreateAt) VALUES
(1, 1, 550000.00, DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -30, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT dbo.ContractAddendum OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 9: CHAPTERS, PAGES, REGIONS, TASKS
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 9: Seeding chapters, pages, tasks...';
SET IDENTITY_INSERT dbo.Chapter ON;
INSERT INTO dbo.Chapter
    (ChapterId, SeriesId, ChapterNumber, Title, ValidPageCount, AppliedGenkouryoPrice, SubmissionDeadline, QcChecklistData, Status, CreateAt)
VALUES
(1, 1, 1, N'Khởi đầu cuộc hành trình', 3, 500000.00, DATEADD(day, 10, GETUTCDATE()), N'{"Technical":"Passed","Art":"Passed","Content":"Passed"}', N'Approved', GETUTCDATE()),
(2, 1, 2, N'Gặp gỡ đồng đội mới',       2, 500000.00, DATEADD(day, 20, GETUTCDATE()), NULL, N'Draft', GETUTCDATE());
SET IDENTITY_INSERT dbo.Chapter OFF;
GO

SET IDENTITY_INSERT dbo.Page ON;
INSERT INTO dbo.Page
    (PageId, ChapterId, PageNumber, RawImageUrl, CompositeImageUrl, BaseLayerUrl, Status, IsApproved, CreateAt)
VALUES
(1, 1, 1, N'http://localhost:9000/manga-publishing/series-1/chap-1/page-1-raw.jpg',       N'http://localhost:9000/manga-publishing/series-1/chap-1/page-1-composite.jpg', N'http://localhost:9000/manga-publishing/series-1/chap-1/page-1-base.jpg', N'Composited', 1, GETUTCDATE()),
(2, 1, 2, N'http://localhost:9000/manga-publishing/series-1/chap-1/page-2-raw.jpg',       N'http://localhost:9000/manga-publishing/series-1/chap-1/page-2-composite.jpg', N'http://localhost:9000/manga-publishing/series-1/chap-1/page-2-base.jpg', N'Composited', 1, GETUTCDATE()),
(3, 1, 3, N'http://localhost:9000/manga-publishing/series-1/chap-1/page-3-raw.jpg',       N'http://localhost:9000/manga-publishing/series-1/chap-1/page-3-composite.jpg', N'http://localhost:9000/manga-publishing/series-1/chap-1/page-3-base.jpg', N'Composited', 1, GETUTCDATE()),
(4, 2, 1, N'http://localhost:9000/manga-publishing/series-1/chap-2/page-1-raw.jpg',       NULL, N'http://localhost:9000/manga-publishing/series-1/chap-2/page-1-base.jpg', N'Pending', 0, GETUTCDATE()),
(5, 2, 2, N'http://localhost:9000/manga-publishing/series-1/chap-2/page-2-raw.jpg',       NULL, N'http://localhost:9000/manga-publishing/series-1/chap-2/page-2-base.jpg', N'Pending', 0, GETUTCDATE());
SET IDENTITY_INSERT dbo.Page OFF;
GO

SET IDENTITY_INSERT dbo.Region ON;
INSERT INTO dbo.Region (RegionId, PageId, CoordinatesJson, Name, CreateAt) VALUES
(1, 1, N'{"top":100,"left":150,"width":300,"height":200}', N'Khung cảnh nền núi',           GETUTCDATE()),
(2, 1, N'{"top":400,"left":100,"width":250,"height":300}', N'Nhân vật chính',               GETUTCDATE()),
(3, 4, N'{"top":50,"left":50,"width":400,"height":300}',   N'Phân cảnh nền phòng học',      GETUTCDATE()),
(4, 4, N'{"top":400,"left":200,"width":200,"height":250}', N'Hiệu ứng hành động',           GETUTCDATE());
SET IDENTITY_INSERT dbo.Region OFF;
GO

SET IDENTITY_INSERT dbo.Tasks ON;
INSERT INTO dbo.Tasks
    (TaskId, MangakaId, RegionId, AssistantId, Description, PaymentAmount, Deadline,
     ExtensionRequestDays, ExtensionReason, ExtensionStatus, ZIndex_Order, Status, Rating, FeedbackComment, CreateAt)
VALUES
(1, 4, 1, 5, N'Vẽ chi tiết hậu cảnh đồi núi đỏ với buổi hoàng hôn', 500000.00, DATEADD(day, -2, GETUTCDATE()), NULL, NULL, N'None',    1, N'Approved',   5, N'Hậu cảnh vẽ rất đẹp và chi tiết, đúng phong cách.', GETUTCDATE()),
(2, 4, 2, 5, N'Vẽ và tô màu trang phục nhân vật chính',               300000.00, DATEADD(day, -1, GETUTCDATE()), NULL, NULL, N'None',    2, N'Approved',   4, N'Màu sắc tươi sáng, đúng yêu cầu.',                   GETUTCDATE()),
(3, 4, 3, 5, N'Vẽ nền phòng học có bàn ghế và cửa sổ lớn',           600000.00, DATEADD(day,  3, GETUTCDATE()), NULL, NULL, N'None',    1, N'Submitted',  NULL, NULL, GETUTCDATE()),
(4, 4, 4, 5, N'Vẽ hiệu ứng các tia chớp bao quanh',                   250000.00, DATEADD(day,  5, GETUTCDATE()), NULL, NULL, N'None',    2, N'In_Progress', NULL, NULL, GETUTCDATE()),
(5, 4, 3, 5, N'Vẽ chi tiết các đồ vật nhỏ trên bàn học',             150000.00, DATEADD(day,  4, GETUTCDATE()), NULL, NULL, N'None',    1, N'Submitted',  NULL, NULL, GETUTCDATE()),
(6, 4, 3, 5, N'Vẽ bóng đổ cho khung cảnh phòng học',                  200000.00, DATEADD(day,  2, GETUTCDATE()), 3,    N'Cần thêm thời gian nghiên cứu phối cảnh phức tạp', N'Pending', 1, N'Disputed', NULL, NULL, GETUTCDATE());
SET IDENTITY_INSERT dbo.Tasks OFF;
GO

SET IDENTITY_INSERT dbo.TaskVersion ON;
INSERT INTO dbo.TaskVersion (VersionId, TaskId, VersionNumber, SubmittedFileUrl, Status, SubmittedAt, CreateAt) VALUES
(1, 1, 1, N'http://localhost:9000/manga-publishing/tasks/task-1-v1.png', N'Approved',  DATEADD(day, -3, GETUTCDATE()), GETUTCDATE()),
(2, 2, 1, N'http://localhost:9000/manga-publishing/tasks/task-2-v1.png', N'Approved',  DATEADD(day, -2, GETUTCDATE()), GETUTCDATE()),
(3, 3, 1, N'http://localhost:9000/manga-publishing/tasks/task-3-v1.png', N'Submitted', DATEADD(minute, -30, GETUTCDATE()), GETUTCDATE()),
(4, 5, 1, N'http://localhost:9000/manga-publishing/tasks/task-5-v1.png', N'Submitted', DATEADD(minute, -20, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT dbo.TaskVersion OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 10: BOARD VOTING CONFIG, VOTES & RANKING
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 10: Seeding board voting config, votes & ranking...';
SET IDENTITY_INSERT dbo.BoardVotingConfig ON;
INSERT INTO dbo.BoardVotingConfig
    (ConfigId, AutoResolveHours, ApprovalThresholdPercent, RejectionThresholdPercent, TiePolicy, ClearVotesOnResubmit, RequireOddBoardSize, BoardRoleId, ChairUserId, CreateAt)
VALUES
(1, 48, 66, 66, N'Escalate', 1, 1, 3, 3, GETUTCDATE());
SET IDENTITY_INSERT dbo.BoardVotingConfig OFF;
GO

SET IDENTITY_INSERT dbo.BoardVote ON;
INSERT INTO dbo.BoardVote
    (VoteId, SeriesId, BoardMemberId, VoteType, RecommendedBudget, Comment, VoteAt, CreateAt)
VALUES
-- Series 2: đã được Hội đồng phê duyệt đầy đủ → Fund_Pending
(1,  2,  3, N'Approve', 30000000.00, N'Cốt truyện hài hước thú vị, phù hợp độc giả trẻ.', GETUTCDATE(), GETUTCDATE()),
(2,  2,  7, N'Approve', 31000000.00, N'Đồng ý duyệt ngân sách.',                              GETUTCDATE(), GETUTCDATE()),
(3,  2,  8, N'Approve', 29500000.00, N'Phù hợp xu hướng thị trường.',                        GETUTCDATE(), GETUTCDATE()),
(4,  2,  9, N'Approve', 30000000.00, N'Đề xuất duyệt.',                                       GETUTCDATE(), GETUTCDATE()),
(5,  2, 10, N'Approve', 30500000.00, N'Chất lượng tốt.',                                      GETUTCDATE(), GETUTCDATE()),
(6,  2, 11, N'Approve', 30000000.00, N'Đồng thuận phê duyệt.',                                GETUTCDATE(), GETUTCDATE()),
-- Series 3: đã duyệt
(7,  3,  3, N'Approve', 23000000.00, N'Romance slice-of-life hấp dẫn.',                       GETUTCDATE(), GETUTCDATE()),
(8,  3,  7, N'Approve', 24000000.00, N'Đồng ý duyệt.',                                        GETUTCDATE(), GETUTCDATE()),
(9,  3,  8, N'Approve', 23000000.00, N'Phù hợp độc giả Shojo.',                               GETUTCDATE(), GETUTCDATE()),
-- Series 5: đang biểu quyết (mới 3/6 phiếu)
(10, 5,  3, N'Approve', 36000000.00, N'Sci-fi setting ấn tượng.',                               GETUTCDATE(), GETUTCDATE()),
(11, 5,  7, N'Approve', 35000000.00, N'Đồng ý thử nghiệm.',                                   GETUTCDATE(), GETUTCDATE()),
(12, 5,  8, N'Reject',  20000000.00, N'Ngân sách đề xuất quá cao.',                           GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT dbo.BoardVote OFF;
GO

SET IDENTITY_INSERT dbo.RankingRecord ON;
INSERT INTO dbo.RankingRecord (RankingId, SeriesId, VoteCount, RankPosition, RecordedDate, CreateAt) VALUES
(1, 1, 120, 5, DATEADD(day, -7, GETUTCDATE()), GETUTCDATE()),
(2, 1, 150, 3, GETUTCDATE(),                     GETUTCDATE()),
(3, 3,  80, 8, GETUTCDATE(),                     GETUTCDATE());
SET IDENTITY_INSERT dbo.RankingRecord OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 11: NOTIFICATIONS
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 11: Seeding notifications...';
SET IDENTITY_INSERT dbo.Notification ON;
INSERT INTO dbo.Notification (NotId, UserId, Content, Type, IsRead, CreateAt) VALUES
(1, 4, N'Bộ truyện Học Viện Siêu Nhiên đã được Hội đồng phê duyệt. Vui lòng xác nhận nhận vốn.', N'Series_Approved', 0, GETUTCDATE()),
(2, 4, N'Bộ truyện Tokyo Dreamers đã được Hội đồng phê duyệt. Chờ Admin lập hợp đồng.',         N'Series_Approved', 0, GETUTCDATE()),
(3, 5, N'Bạn được giao nhiệm vụ mới: Vẽ nền phòng học có bàn ghế và cửa sổ lớn.',              N'TaskAssigned',    0, GETUTCDATE()),
(4, 1, N'Có 1 series chờ lập hợp đồng: Tokyo Dreamers.',                                         N'Contract_Pending', 0, GETUTCDATE()),
(5, 2, N'Bộ truyện Bí Ẩn Midgard cần bạn duyệt (Pending_Approval).',                              N'SeriesReview',    0, GETUTCDATE()),
(6, 3, N'Bộ truyện Cyber Ronin đang chờ biểu quyết của Hội đồng.',                               N'SeriesVote',      0, GETUTCDATE());
SET IDENTITY_INSERT dbo.Notification OFF;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- PHASE 12: ANNOTATIONS, DISPUTES, REPORTS, REFRESH TOKENS
-- ─────────────────────────────────────────────────────────────────────────
PRINT 'Phase 12: Seeding annotations, disputes, reports, refresh tokens...';

SET IDENTITY_INSERT dbo.Annotation ON;
INSERT INTO dbo.Annotation
    (AnnotationId, CreatedByUserId, PageId, TaskVersionId, CoordinatesJson, Comment, Type, CreateAt)
VALUES
-- Ghi chú Canvas trên Page (QC / review)
(1, 4, 1, NULL, N'{"top":120,"left":200,"width":80,"height":60}',  N'Cần làm sắc nét đường nét nhân vật',           N'QC',      GETUTCDATE()),
(2, 2, 4, NULL, N'{"top":80,"left":120,"width":150,"height":100}', N'Bố cục phòng học hơi chật, nên dãn region',     N'Review',  GETUTCDATE()),
(3, 4, 2, NULL, N'{"top":300,"left":50,"width":200,"height":120}', N'Màu nền hoàng hôn cần ấm hơn',                  N'Note',    GETUTCDATE()),
-- Ghi chú trên bản nộp Task (revision)
(4, 4, NULL, 3, N'{"top":150,"left":180,"width":100,"height":80}',  N'Cửa sổ phía sau bị lệch phối cảnh',             N'Revision', GETUTCDATE()),
(5, 4, NULL, 4, N'{"top":90,"left":60,"width":120,"height":90}',   N'Chi tiết đồ vật trên bàn chưa đủ rõ',           N'Revision', GETUTCDATE());
SET IDENTITY_INSERT dbo.Annotation OFF;
GO

SET IDENTITY_INSERT dbo.DisputeLog ON;
INSERT INTO dbo.DisputeLog
    (DisputeId, EditorId, TaskId, EditorComment, ResolutionType, AssistantPercentage, MangakaPercentage, ResolvedAt, CreateAt)
VALUES
-- Task 1: tranh chấp đã xử lý xong (task hiện Approved)
(1, 2, 1,
 N'Editor quyết định phân xử tranh chấp: Trợ lý nhận 70%, Tác giả nhận 30%.',
 N'Arbitration', 70.00, 30.00, DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE()));
-- Task 6: đang Disputed — chưa có DisputeLog (Open, test màn hình tranh chấp)
SET IDENTITY_INSERT dbo.DisputeLog OFF;
GO

SET IDENTITY_INSERT dbo.Report ON;
INSERT INTO dbo.Report (ReportId, ReporterId, ReportedUserId, Reason, Status, CreateAt) VALUES
(1, 4, 5, N'Assistant nộp bài trễ deadline 2 lần liên tiếp mà không báo trước.',           N'Pending',  GETUTCDATE()),
(2, 2, 6, N'Tài khoản Assistant pending có portfolio không khớp kỹ năng khai báo.',     N'Pending',  GETUTCDATE()),
(3, 5, 4, N'Mangaka từ chối thanh toán task đã hoàn thành đúng yêu cầu.',              N'Reviewing', DATEADD(day, -2, GETUTCDATE())),
(4, 1, 5, N'Admin ghi nhận khiếu nại chất lượng — đã chuyển Editor xử lý.',             N'Resolved', DATEADD(day, -10, GETUTCDATE()));
SET IDENTITY_INSERT dbo.Report OFF;
GO

SET IDENTITY_INSERT dbo.RefreshToken ON;
INSERT INTO dbo.RefreshToken (RefreshTokenId, UserId, Token, ExpiresAt, IsRevoked, CreateAt) VALUES
(1, 1, 'seed-refresh-admin-revoked',    DATEADD(day, -1,  GETUTCDATE()), 1, DATEADD(day, -30, GETUTCDATE())),
(2, 4, 'seed-refresh-mangaka-active',  DATEADD(day,  7,  GETUTCDATE()), 0, GETUTCDATE()),
(3, 5, 'seed-refresh-assistant-active', DATEADD(day,  7,  GETUTCDATE()), 0, GETUTCDATE());
SET IDENTITY_INSERT dbo.RefreshToken OFF;
GO

PRINT '';
PRINT '=================================================================';
PRINT ' Seed completed successfully.';
PRINT ' Password for ALL default accounts: 12345';
PRINT ' Users: admin | editor1 | board1-6 | mangaka1 | assistant1 | assistant_pending';
PRINT ' Series: 5 records covering In Production / Fund_Pending / Pending_Approval / Pending_Board_Vote';
PRINT ' Contracts: Series 1 (Signed), Series 2 (Active) — Series 3 awaiting contract';
PRINT ' Also seeded: Annotation(5), DisputeLog(1), Report(4), RefreshToken(3)';
PRINT ' Note: __EFMigrationsHistory is managed by EF migrations, not seed.sql';
PRINT '=================================================================';
GO
