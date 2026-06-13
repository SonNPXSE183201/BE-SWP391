-- =========================================================================
-- DATABASE CREATION & SCHEMA INITIALIZATION SCRIPT
-- PROJECT: Manga Creation Workflow & Publishing Management System (MCWPMS)
-- DATABASE NAME: MangaPublishing
-- RDBMS: Microsoft SQL Server (MS SQL)
-- DATE: 2026-06-06
-- =========================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE master;
GO

-- Reset/Drop Database if it already exists to start fresh and avoid conflicts/errors
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MangaPublishing')
BEGIN
    ALTER DATABASE MangaPublishing SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MangaPublishing;
    PRINT 'Database "MangaPublishing" dropped successfully.';
END
GO

-- Create Database
CREATE DATABASE MangaPublishing;
PRINT 'Database "MangaPublishing" created successfully.';
GO

USE MangaPublishing;
GO

-- Drop existing tables in reverse dependency order to avoid constraints conflicts
IF OBJECT_ID('dbo.Report', 'U') IS NOT NULL DROP TABLE dbo.Report;
IF OBJECT_ID('dbo.Annotation', 'U') IS NOT NULL DROP TABLE dbo.Annotation;
IF OBJECT_ID('dbo.DisputeLog', 'U') IS NOT NULL DROP TABLE dbo.DisputeLog;
IF OBJECT_ID('dbo.TaskVersion', 'U') IS NOT NULL DROP TABLE dbo.TaskVersion;
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Region', 'U') IS NOT NULL DROP TABLE dbo.Region;
IF OBJECT_ID('dbo.Page', 'U') IS NOT NULL DROP TABLE dbo.Page;
IF OBJECT_ID('dbo.Chapter', 'U') IS NOT NULL DROP TABLE dbo.Chapter;
IF OBJECT_ID('dbo.ContractAddendum', 'U') IS NOT NULL DROP TABLE dbo.ContractAddendum;
IF OBJECT_ID('dbo.Contract', 'U') IS NOT NULL DROP TABLE dbo.Contract;
IF OBJECT_ID('dbo.BoardVote', 'U') IS NOT NULL DROP TABLE dbo.BoardVote;
IF OBJECT_ID('dbo.RankingRecord', 'U') IS NOT NULL DROP TABLE dbo.RankingRecord;
IF OBJECT_ID('dbo.Series', 'U') IS NOT NULL DROP TABLE dbo.Series;
IF OBJECT_ID('dbo.Notification', 'U') IS NOT NULL DROP TABLE dbo.Notification;
IF OBJECT_ID('dbo.AssistantProfile', 'U') IS NOT NULL DROP TABLE dbo.AssistantProfile;
IF OBJECT_ID('dbo.Transaction', 'U') IS NOT NULL DROP TABLE dbo.[Transaction];
IF OBJECT_ID('dbo.Wallet', 'U') IS NOT NULL DROP TABLE dbo.Wallet;
IF OBJECT_ID('dbo.RefreshToken', 'U') IS NOT NULL DROP TABLE dbo.RefreshToken;
IF OBJECT_ID('dbo.User', 'U') IS NOT NULL DROP TABLE dbo.[User];
IF OBJECT_ID('dbo.Role', 'U') IS NOT NULL DROP TABLE dbo.Role;
GO

-- =========================================================================
-- 1. TABLE: Role
-- =========================================================================
CREATE TABLE dbo.Role (
    RoleId INT IDENTITY(1,1) NOT NULL,
    RoleName NVARCHAR(50) NOT NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Role_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Role PRIMARY KEY CLUSTERED (RoleId),
    CONSTRAINT UQ_Role_RoleName UNIQUE (RoleName)
);
GO

-- =========================================================================
-- 2. TABLE: User
-- =========================================================================
CREATE TABLE dbo.[User] (
    UserId INT IDENTITY(1,1) NOT NULL,
    RoleId INT NOT NULL,
    UserName VARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_User_Status DEFAULT N'Pending',
    PenName NVARCHAR(100) NULL,
    PortfolioUrl NVARCHAR(500) NULL,
    Skills NVARCHAR(500) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_User_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT UQ_User_UserName UNIQUE (UserName),
    CONSTRAINT UQ_User_Email UNIQUE (Email),
    CONSTRAINT CK_User_Status CHECK (Status IN (N'Pending', N'Active', N'Rejected', N'Locked'))
);
GO

-- =========================================================================
-- 2b. TABLE: RefreshToken
-- =========================================================================
CREATE TABLE dbo.RefreshToken (
    RefreshTokenId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL CONSTRAINT DF_RefreshToken_IsRevoked DEFAULT 0,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_RefreshToken_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_RefreshToken PRIMARY KEY CLUSTERED (RefreshTokenId),
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_RefreshToken_Token UNIQUE (Token)
);
GO

-- =========================================================================
-- 3. TABLE: Wallet
-- =========================================================================
CREATE TABLE dbo.Wallet (
    WalletId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    SetupFundBalance DECIMAL(18,2) NOT NULL CONSTRAINT DF_Wallet_SetupFund DEFAULT 0.00,
    WithdrawableBalance DECIMAL(18,2) NOT NULL CONSTRAINT DF_Wallet_Withdrawable DEFAULT 0.00,
    LockedFund DECIMAL(18,2) NOT NULL CONSTRAINT DF_Wallet_LockedFund DEFAULT 0.00,
    LockedWithdrawable DECIMAL(18,2) NOT NULL CONSTRAINT DF_Wallet_LockedWithdrawable DEFAULT 0.00,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Wallet_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Wallet PRIMARY KEY CLUSTERED (WalletId),
    CONSTRAINT FK_Wallet_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_Wallet_UserId UNIQUE (UserId)
);
GO

-- =========================================================================
-- 4. TABLE: Transaction
-- =========================================================================
CREATE TABLE dbo.[Transaction] (
    TransactionId INT IDENTITY(1,1) NOT NULL,
    WalletId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    ReferenceId INT NULL,
    SetupFundAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transaction_SetupFund DEFAULT 0.00,
    WithdrawableAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transaction_Withdrawable DEFAULT 0.00,
    Amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transaction_Amount DEFAULT 0.00,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Transaction_Status DEFAULT N'Pending',
    ReferenceCode VARCHAR(100) NULL,
    FromUserId INT NULL,
    ToUserId INT NULL,
    BankName NVARCHAR(100) NULL,
    BankAccountNumber VARCHAR(50) NULL,
    BankAccountName NVARCHAR(200) NULL,
    AdminNote NVARCHAR(500) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Transaction_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Transaction PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT FK_Transaction_Wallet FOREIGN KEY (WalletId) REFERENCES dbo.Wallet (WalletId) ON DELETE CASCADE,
    CONSTRAINT FK_Transaction_FromUser FOREIGN KEY (FromUserId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Transaction_ToUser FOREIGN KEY (ToUserId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- 5. TABLE: AssistantProfile
-- =========================================================================
CREATE TABLE dbo.AssistantProfile (
    ProfileId INT IDENTITY(1,1) NOT NULL,
    AssistantId INT NOT NULL,
    SpecialtyTags NVARCHAR(255) NULL,
    TotalCompletedTasks INT NOT NULL CONSTRAINT DF_AP_TotalTasks DEFAULT 0,
    OnTimeRate DECIMAL(5,2) NOT NULL CONSTRAINT DF_AP_OnTimeRate DEFAULT 0.00,
    DisputeRate DECIMAL(5,2) NOT NULL CONSTRAINT DF_AP_DisputeRate DEFAULT 0.00,
    CurrentActiveTasks INT NOT NULL CONSTRAINT DF_AP_ActiveTasks DEFAULT 0,
    AverageRating DECIMAL(3,2) NOT NULL CONSTRAINT DF_AP_AvgRating DEFAULT 0.00,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_AssistantProfile_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_AssistantProfile PRIMARY KEY CLUSTERED (ProfileId),
    CONSTRAINT FK_AssistantProfile_User FOREIGN KEY (AssistantId) REFERENCES dbo.[User] (UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_AssistantProfile_AssistantId UNIQUE (AssistantId)
);
GO

-- =========================================================================
-- 6. TABLE: Notification
-- =========================================================================
CREATE TABLE dbo.Notification (
    NotId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IsRead BIT NOT NULL CONSTRAINT DF_Notification_IsRead DEFAULT 0,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Notification_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Notification PRIMARY KEY CLUSTERED (NotId),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 7. TABLE: Series
-- =========================================================================
CREATE TABLE dbo.Series (
    SeriesId INT IDENTITY(1,1) NOT NULL,
    MangakaId INT NOT NULL,
    EditorId INT NULL,
    Title NVARCHAR(250) NOT NULL,
    Genre NVARCHAR(100) NULL,
    Synopsis NVARCHAR(MAX) NULL,
    CoverArtworkUrl NVARCHAR(500) NULL,
    EstimatedProductionBudget DECIMAL(18,2) NOT NULL CONSTRAINT DF_Series_EstBudget DEFAULT 0.00,
    ApprovedProductionBudget DECIMAL(18,2) NOT NULL CONSTRAINT DF_Series_AppBudget DEFAULT 0.00,
    PublicationSchedule NVARCHAR(50) NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Series_Status DEFAULT N'Draft',
    ResourceFolderUrl NVARCHAR(500) NULL,
    DraftManuscriptUrl NVARCHAR(500) NULL,
    EditorReport NVARCHAR(MAX) NULL,
    SuggestedBudget DECIMAL(18,2) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Series_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Series PRIMARY KEY CLUSTERED (SeriesId),
    CONSTRAINT FK_Series_Mangaka FOREIGN KEY (MangakaId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Series_Editor FOREIGN KEY (EditorId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- 8. TABLE: RankingRecord
-- =========================================================================
CREATE TABLE dbo.RankingRecord (
    RankingId INT IDENTITY(1,1) NOT NULL,
    SeriesId INT NOT NULL,
    VoteCount INT NOT NULL CONSTRAINT DF_RankingRecord_Votes DEFAULT 0,
    RankPosition INT NOT NULL,
    RecordedDate DATETIME2 NOT NULL CONSTRAINT DF_RankingRecord_Date DEFAULT GETDATE(),
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_RankingRecord_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_RankingRecord PRIMARY KEY CLUSTERED (RankingId),
    CONSTRAINT FK_RankingRecord_Series FOREIGN KEY (SeriesId) REFERENCES dbo.Series (SeriesId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 9. TABLE: BoardVote
-- =========================================================================
CREATE TABLE dbo.BoardVote (
    VoteId INT IDENTITY(1,1) NOT NULL,
    SeriesId INT NOT NULL,
    BoardMemberId INT NOT NULL,
    VoteType NVARCHAR(50) NOT NULL,
    RecommendedBudget DECIMAL(18,2) NOT NULL CONSTRAINT DF_BoardVote_Budget DEFAULT 0.00,
    Comment NVARCHAR(1000) NULL,
    VoteAt DATETIME2 NOT NULL CONSTRAINT DF_BoardVote_VoteAt DEFAULT GETDATE(),
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_BoardVote_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_BoardVote PRIMARY KEY CLUSTERED (VoteId),
    CONSTRAINT FK_BoardVote_Series FOREIGN KEY (SeriesId) REFERENCES dbo.Series (SeriesId) ON DELETE CASCADE,
    CONSTRAINT FK_BoardVote_User FOREIGN KEY (BoardMemberId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- 10. TABLE: Contract
-- =========================================================================
CREATE TABLE dbo.Contract (
    ContractId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    SeriesId INT NOT NULL,
    BaseGenkouryoPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Contract_Price DEFAULT 0.00,
    SignedDate DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Contract_Status DEFAULT N'Pending',
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Contract_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Contract PRIMARY KEY CLUSTERED (ContractId),
    CONSTRAINT FK_Contract_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Contract_Series FOREIGN KEY (SeriesId) REFERENCES dbo.Series (SeriesId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- 11. TABLE: ContractAddendum
-- =========================================================================
CREATE TABLE dbo.ContractAddendum (
    AddendumId INT IDENTITY(1,1) NOT NULL,
    ContractId INT NOT NULL,
    NewGenkouryoPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Addendum_Price DEFAULT 0.00,
    EffectiveDate DATETIME2 NOT NULL,
    SignedDate DATETIME2 NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_ContractAddendum_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_ContractAddendum PRIMARY KEY CLUSTERED (AddendumId),
    CONSTRAINT FK_ContractAddendum_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract (ContractId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 12. TABLE: Chapter
-- =========================================================================
CREATE TABLE dbo.Chapter (
    ChapterId INT IDENTITY(1,1) NOT NULL,
    SeriesId INT NOT NULL,
    ChapterNumber INT NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    ValidPageCount INT NOT NULL CONSTRAINT DF_Chapter_PageCount DEFAULT 0,
    AppliedGenkouryoPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Chapter_Genkouryo DEFAULT 0.00,
    SubmissionDeadline DATETIME2 NULL,
    QcChecklistData NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Chapter_Status DEFAULT N'Draft',
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Chapter_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Chapter PRIMARY KEY CLUSTERED (ChapterId),
    CONSTRAINT FK_Chapter_Series FOREIGN KEY (SeriesId) REFERENCES dbo.Series (SeriesId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 13. TABLE: Page
-- =========================================================================
CREATE TABLE dbo.Page (
    PageId INT IDENTITY(1,1) NOT NULL,
    ChapterId INT NOT NULL,
    PageNumber INT NOT NULL,
    RawImageUrl NVARCHAR(500) NOT NULL,
    CompositeImageUrl NVARCHAR(500) NULL,
    BaseLayerUrl NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Page_Status DEFAULT N'Pending',
    IsApproved BIT NOT NULL CONSTRAINT DF_Page_IsApproved DEFAULT 0,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Page_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Page PRIMARY KEY CLUSTERED (PageId),
    CONSTRAINT FK_Page_Chapter FOREIGN KEY (ChapterId) REFERENCES dbo.Chapter (ChapterId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 14. TABLE: Region
-- =========================================================================
CREATE TABLE dbo.Region (
    RegionId INT IDENTITY(1,1) NOT NULL,
    PageId INT NOT NULL,
    CoordinatesJson NVARCHAR(MAX) NOT NULL,
    Name NVARCHAR(100) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Region_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Region PRIMARY KEY CLUSTERED (RegionId),
    CONSTRAINT FK_Region_Page FOREIGN KEY (PageId) REFERENCES dbo.Page (PageId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 15. TABLE: Tasks
-- =========================================================================
CREATE TABLE dbo.Tasks (
    TaskId INT IDENTITY(1,1) NOT NULL,
    MangakaId INT NOT NULL,
    RegionId INT NOT NULL,
    AssistantId INT NULL,
    Description NVARCHAR(1000) NULL,
    PaymentAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Task_Payment DEFAULT 0.00,
    Deadline DATETIME2 NOT NULL,
    ExtensionRequestDays INT NULL,
    ExtensionReason NVARCHAR(500) NULL,
    ExtensionStatus NVARCHAR(50) NULL CONSTRAINT DF_Task_ExtStatus DEFAULT N'None',
    ZIndex_Order INT NOT NULL CONSTRAINT DF_Task_ZIndex DEFAULT 0,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Task_Status DEFAULT N'Draft',
    Rating INT NULL,
    FeedbackComment NVARCHAR(1000) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Tasks_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Tasks PRIMARY KEY CLUSTERED (TaskId),
    CONSTRAINT FK_Task_Mangaka FOREIGN KEY (MangakaId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Task_Region FOREIGN KEY (RegionId) REFERENCES dbo.Region (RegionId) ON DELETE CASCADE,
    CONSTRAINT FK_Task_Assistant FOREIGN KEY (AssistantId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- 16. TABLE: TaskVersion
-- =========================================================================
CREATE TABLE dbo.TaskVersion (
    VersionId INT IDENTITY(1,1) NOT NULL,
    TaskId INT NOT NULL,
    VersionNumber INT NOT NULL,
    SubmittedFileUrl NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_TaskVersion_Status DEFAULT N'Submitted',
    SubmittedAt DATETIME2 NOT NULL CONSTRAINT DF_TaskVersion_SubmittedAt DEFAULT GETDATE(),
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_TaskVersion_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_TaskVersion PRIMARY KEY CLUSTERED (VersionId),
    CONSTRAINT FK_TaskVersion_Task FOREIGN KEY (TaskId) REFERENCES dbo.Tasks (TaskId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 17. TABLE: DisputeLog
-- =========================================================================
CREATE TABLE dbo.DisputeLog (
    DisputeId INT IDENTITY(1,1) NOT NULL,
    EditorId INT NOT NULL,
    TaskId INT NOT NULL,
    EditorComment NVARCHAR(MAX) NULL,
    ResolutionType NVARCHAR(100) NOT NULL,
    AssistantPercentage DECIMAL(5,2) NULL,
    MangakaPercentage DECIMAL(5,2) NULL,
    ResolvedAt DATETIME2 NOT NULL CONSTRAINT DF_DisputeLog_ResolvedAt DEFAULT GETDATE(),
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_DisputeLog_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_DisputeId PRIMARY KEY CLUSTERED (DisputeId),
    CONSTRAINT FK_DisputeLog_Editor FOREIGN KEY (EditorId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_DisputeLog_Task FOREIGN KEY (TaskId) REFERENCES dbo.Tasks (TaskId) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 18. TABLE: Annotation
-- =========================================================================
CREATE TABLE dbo.Annotation (
    AnnotationId INT IDENTITY(1,1) NOT NULL,
    CreatedByUserId INT NOT NULL,
    PageId INT NULL,
    TaskVersionId INT NULL,
    CoordinatesJson NVARCHAR(MAX) NOT NULL,
    Comment NVARCHAR(1000) NOT NULL,
    Type NVARCHAR(50) NULL,
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Annotation_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Annotation PRIMARY KEY CLUSTERED (AnnotationId),
    CONSTRAINT FK_Annotation_User FOREIGN KEY (CreatedByUserId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Annotation_Page FOREIGN KEY (PageId) REFERENCES dbo.Page (PageId) ON DELETE CASCADE,
    CONSTRAINT FK_Annotation_TaskVersion FOREIGN KEY (TaskVersionId) REFERENCES dbo.TaskVersion (VersionId) ON DELETE NO ACTION,
    CONSTRAINT CK_Annotation_Target CHECK ((PageId IS NOT NULL AND TaskVersionId IS NULL) OR (PageId IS NULL AND TaskVersionId IS NOT NULL))
);
GO

-- =========================================================================
-- 19. TABLE: Report
-- =========================================================================
CREATE TABLE dbo.Report (
    ReportId INT IDENTITY(1,1) NOT NULL,
    ReporterId INT NOT NULL,
    ReportedUserId INT NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Report_Status DEFAULT N'Pending',
    CreateAt DATETIME2 NOT NULL CONSTRAINT DF_Report_CreateAt DEFAULT GETUTCDATE(),
    UpdateAt DATETIME2 NULL,
    CONSTRAINT PK_Report PRIMARY KEY CLUSTERED (ReportId),
    CONSTRAINT FK_Report_Reporter FOREIGN KEY (ReporterId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Report_ReportedUser FOREIGN KEY (ReportedUserId) REFERENCES dbo.[User] (UserId) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- INDEX DEFINITIONS FOR PERFORMANCE OPTIMIZATION
-- =========================================================================
CREATE INDEX IX_User_RoleId ON dbo.[User] (RoleId);
CREATE INDEX IX_Wallet_UserId ON dbo.Wallet (UserId);
CREATE INDEX IX_Transaction_WalletId ON dbo.[Transaction] (WalletId);
CREATE INDEX IX_Transaction_FromUserId ON dbo.[Transaction] (FromUserId);
CREATE INDEX IX_Transaction_ToUserId ON dbo.[Transaction] (ToUserId);
CREATE INDEX IX_AssistantProfile_AssistantId ON dbo.AssistantProfile (AssistantId);
CREATE INDEX IX_Notification_UserId ON dbo.Notification (UserId);
CREATE INDEX IX_Series_MangakaId ON dbo.Series (MangakaId);
CREATE INDEX IX_Series_EditorId ON dbo.Series (EditorId);
CREATE INDEX IX_Series_Status ON dbo.Series (Status);
CREATE INDEX IX_RankingRecord_SeriesId ON dbo.RankingRecord (SeriesId);
CREATE INDEX IX_BoardVote_SeriesId ON dbo.BoardVote (SeriesId);
CREATE INDEX IX_Contract_UserId ON dbo.Contract (UserId);
CREATE INDEX IX_Contract_SeriesId ON dbo.Contract (SeriesId);
CREATE INDEX IX_Chapter_SeriesId ON dbo.Chapter (SeriesId);
CREATE INDEX IX_Chapter_Status ON dbo.Chapter (Status);
CREATE INDEX IX_Page_ChapterId ON dbo.Page (ChapterId);
CREATE INDEX IX_Page_Status ON dbo.Page (Status);
CREATE INDEX IX_Region_PageId ON dbo.Region (PageId);
CREATE INDEX IX_Tasks_MangakaId ON dbo.Tasks (MangakaId);
CREATE INDEX IX_Tasks_AssistantId ON dbo.Tasks (AssistantId);
CREATE INDEX IX_Tasks_Status ON dbo.Tasks (Status);
CREATE INDEX IX_TaskVersion_TaskId ON dbo.TaskVersion (TaskId);
CREATE INDEX IX_DisputeLog_TaskId ON dbo.DisputeLog (TaskId);
CREATE INDEX IX_Annotation_CreatedByUserId ON dbo.Annotation (CreatedByUserId);
CREATE INDEX IX_Annotation_PageId ON dbo.Annotation (PageId) WHERE PageId IS NOT NULL;
CREATE INDEX IX_Annotation_TaskVersionId ON dbo.Annotation (TaskVersionId) WHERE TaskVersionId IS NOT NULL;
CREATE INDEX IX_Report_ReporterId ON dbo.Report (ReporterId);
CREATE INDEX IX_Report_ReportedUserId ON dbo.Report (ReportedUserId);
CREATE INDEX IX_RefreshToken_UserId ON dbo.RefreshToken (UserId);
CREATE INDEX IX_RefreshToken_Token ON dbo.RefreshToken (Token);
GO

PRINT 'Database MangaPublishing schema initialized successfully with unified audit columns (CreateAt/UpdateAt).';
GO
