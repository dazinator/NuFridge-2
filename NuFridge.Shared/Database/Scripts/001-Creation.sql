IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'NuFridge' ) 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA NuFridge'
END

CREATE TABLE [NuFridge].[ApiKey] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ApiKey_Id] PRIMARY KEY CLUSTERED,
[UserId] int NOT NULL,
[ApiKeyHashed] NVARCHAR(100) NOT NULL,
[Created] DATETIMEOFFSET NOT NULL
)
ALTER TABLE [NuFridge].[ApiKey] ADD CONSTRAINT [UQ_ApiKeyUnique] UNIQUE([ApiKeyHashed])


CREATE TABLE [NuFridge].[Feed] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Feed_Id] PRIMARY KEY CLUSTERED,
[Name] NVARCHAR(100) NOT NULL,
[ApiKeyHashed] nvarchar(max) NULL,
[ApiKeySalt] nvarchar(max) NULL,
[GroupId] int NOT NULL
)
ALTER TABLE [NuFridge].[Feed] ADD CONSTRAINT [UQ_FeedNameUnique] UNIQUE([Name])

CREATE TABLE [NuFridge].[FeedConfiguration] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FeedConfiguration_Id] PRIMARY KEY CLUSTERED,
[Directory] NVARCHAR(1000) NOT NULL,
[FeedId] int NOT NULL,
[RetentionPolicyEnabled] bit NOT NULL DEFAULT(0),
[AllowPackageOverwrite] BIT NOT NULL DEFAULT 0,
[MaxReleasePackages] int NOT NULL DEFAULT(10),
[RetentionPolicyDeletePackages] bit NOT NULL  DEFAULT(1),
[MaxPrereleasePackages] int NOT NULL DEFAULT(10)
)

CREATE TABLE [NuFridge].[Framework]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(4000) NOT NULL
)

CREATE TABLE [NuFridge].[Package] (
    [PrimaryId]                INT             IDENTITY (1, 1) NOT NULL,
    [FeedId]                   INT             NOT NULL,
    [Id]                NVARCHAR (4000) NOT NULL,
    [IdHash]            AS              (CONVERT([varbinary](16),hashbytes('MD5',[Id]))) PERSISTED,
    [Title]                    NVARCHAR (4000) NULL,
    [Version]                  NVARCHAR (4000) NOT NULL,
    [VersionMajor]             INT             NOT NULL,
    [VersionMinor]             INT             NOT NULL,
    [VersionBuild]             INT             NOT NULL,
    [VersionRevision]          INT             NOT NULL,
    [VersionSpecial]           NVARCHAR (255) NULL,
    [IsPrerelease]             BIT             NOT NULL,
    [Listed]                   BIT             NOT NULL,
    [Copyright]                NVARCHAR (4000) NULL,
    [LastUpdated]              DATETIME2 (7)   NULL,
    [LicenseUrl]               NVARCHAR (MAX)  NULL,
    [ProjectUrl]               NVARCHAR (MAX)  NULL,
    [RequireLicenseAcceptance] BIT             NOT NULL,
    [Tags]                     NVARCHAR (MAX)  NULL,
    [Owners]                   NVARCHAR (MAX)  NULL,
    [IconUrl]                  NVARCHAR (MAX)  NULL,
    [Authors]                  NVARCHAR (4000) NULL,
    [Description]              NVARCHAR (MAX)  NULL,
    [ReleaseNotes]             NVARCHAR (MAX)  NULL,
    [Summary]                  NVARCHAR (MAX)  NULL,
    [Hash]                     NVARCHAR (4000) NOT NULL,
    [Created]                  DATETIME2 (7)   NULL,
	[MinClientVersion] NVARCHAR(255) NULL,
    [Published]                DATETIME2 (7)   NOT NULL,
    [Dependencies]             NVARCHAR (MAX)  NULL,
    [SupportedFrameworks]      NVARCHAR (MAX)  NULL,
    [DevelopmentDependency]    BIT             NOT NULL,
    [Language]                 NVARCHAR (4000) NULL,
    [ReportAbuseUrl]           NVARCHAR (MAX)  NULL,
	[PackageSize]			   BIGINT		   NOT NULL
);
GO

CREATE CLUSTERED INDEX [IX_NuFridgePackage_FeedId_Id]
    ON [NuFridge].[Package]([FeedId] ASC, [IdHash] ASC);
GO


CREATE TABLE [NuFridge].[PackageDownload] (
    [Id]              BIGINT          IDENTITY (1, 1) NOT NULL,
    [FeedId]          INT             NOT NULL,
    [PackageId]       NVARCHAR (4000) NOT NULL,
    [PackageIdHash]   AS              (CONVERT([varbinary](16),hashbytes('MD5',[PackageId]))) PERSISTED,
    [VersionMajor]    INT             NOT NULL,
    [VersionMinor]    INT             NOT NULL,
    [VersionBuild]    INT             NOT NULL,
    [VersionRevision] INT             NOT NULL,
    [VersionSpecial]  NVARCHAR (255)  NULL,
    [DownloadedAt]    DATETIME2 (7)   NOT NULL,
    [UserAgent]       NVARCHAR (MAX)  NOT NULL,
    [IPAddress]       NVARCHAR (45)   NOT NULL
);
GO

CREATE CLUSTERED INDEX [IX_NuFridgePackageDownload_FeedId_PackageIdHash_Version_DownloadedAt]
    ON [NuFridge].[PackageDownload]([FeedId] ASC, [PackageIdHash] ASC, [VersionMajor] DESC, [VersionMinor] DESC, [VersionBuild] DESC, [VersionRevision] DESC, [DownloadedAt] ASC);
GO


CREATE PROCEDURE [NuFridge].[GetAllPackages]
	@feedId int
AS

with cte as
(
   SELECT    pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk WITH(NOLOCK)
WHERE @feedId IS NULL OR pk.FeedId = @feedId
)

SELECT 
IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END,
IsLatestVersion = CASE WHEN (
	SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.Id = Id AND Listed = 1
	) = rn THEN 1 ELSE 0 END, 
DownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash),
VersionDownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash AND pd.VersionMajor = ctee.VersionMajor AND pd.VersionMinor = ctee.VersionMinor AND pd.VersionBuild = ctee.VersionBuild AND pd.VersionRevision = ctee.VersionRevision AND pd.VersionSpecial = ctee.VersionSpecial),
* FROM cte as ctee
GO

CREATE PROCEDURE [NuFridge].[GetUniquePackageCount]
	@feedId int
AS
with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk WITH(NOLOCK)
WHERE pk.FeedId = @feedId
)

SELECT COUNT_BIG(DISTINCT Id) FROM CTE as ctee
WHERE (rn = 1 OR (SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.Id = Id AND Listed = 1) = rn) AND Listed = 1
GO

CREATE PROCEDURE [NuFridge].[GetLatestPackages]
	@feedId int,
	@includePrerelease bit,
	@partialId NVARCHAR(4000)
AS

with cte as
(
   SELECT
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk WITH(NOLOCK)
WHERE pk.FeedId = @feedId AND (@partialId IS NULL OR Id LIKE '%' + @partialId + '%')
)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND orig.Id = Id AND Listed = 1
) = rn THEN 1 ELSE 0 END,
DownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = orig.FeedId AND pd.PackageIdHash = orig.IdHash),
VersionDownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = orig.FeedId AND pd.PackageIdHash = orig.IdHash AND pd.VersionMajor = orig.VersionMajor AND pd.VersionMinor = orig.VersionMinor AND pd.VersionBuild = orig.VersionBuild AND pd.VersionRevision = orig.VersionRevision AND pd.VersionSpecial = orig.VersionSpecial),
 * FROM CTE as orig 
WHERE (@includePrerelease = 1 AND orig.rn = 1 AND orig.Listed = 1) OR (@includePrerelease = 0 AND orig.rn = (SELECT TOP(1) rn FROM cte as ctee where ctee.IsPrerelease = 0 AND ctee.Id = orig.Id AND ctee.Listed = 1))
GO

CREATE PROCEDURE [NuFridge].[GetVersionsOfPackage]
	@feedId int,
	@includePrerelease bit,
	@packageId nvarchar(4000)
AS

with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk WITH(NOLOCK)
WHERE (@feedId IS NULL OR pk.FeedId = @feedId) AND pk.Id = @packageId AND (@includePrerelease = 1 OR pk.IsPrerelease = 0)
)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.Id = Id AND Listed = 1
) = rn THEN 1 ELSE 0 END,
DownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash),
VersionDownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash AND pd.VersionMajor = ctee.VersionMajor AND pd.VersionMinor = ctee.VersionMinor AND pd.VersionBuild = ctee.VersionBuild AND pd.VersionRevision = ctee.VersionRevision AND pd.VersionSpecial = ctee.VersionSpecial),
 * FROM cte as ctee
GO

CREATE PROCEDURE [NuFridge].[GetPackage]
	@feedId int,
	@packageId nvarchar(4000),
	@versionMajor int,
	@versionMinor int,
	@versionBuild int,
	@versionRevision int,
	@versionSpecial nvarchar(255)
AS

with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk WITH(NOLOCK)
WHERE (@feedId IS NULL OR pk.FeedId = @feedId) AND pk.Id = @packageId
)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.Id = Id AND Listed = 1
) = rn THEN 1 ELSE 0 END, 
DownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash),
VersionDownloadCount = (SELECT COUNT(*) FROM [NuFridge].[PackageDownload] as pd WITH(NOLOCK) WHERE pd.FeedId = ctee.FeedId AND pd.PackageIdHash = ctee.IdHash AND pd.VersionMajor = ctee.VersionMajor AND pd.VersionMinor = ctee.VersionMinor AND pd.VersionBuild = ctee.VersionBuild AND pd.VersionRevision = ctee.VersionRevision AND pd.VersionSpecial = ctee.VersionSpecial),
*  FROM cte as ctee
WHERE ctee.[VersionMajor] = @versionMajor AND ctee.[VersionMinor] = @versionMinor AND ctee.[VersionBuild] = @versionBuild AND ctee.[VersionRevision] = @versionRevision AND (@versionSpecial IS NULL OR ctee.[VersionSpecial] = @versionSpecial)
GO

CREATE NONCLUSTERED INDEX [IX_NuFridgePackage_Version]
    ON [NuFridge].[Package]([IdHash] ASC, [Listed] DESC, [VersionMajor] DESC, [VersionMinor] DESC, [VersionBuild] DESC, [VersionRevision] DESC, [IsPrerelease] ASC, [VersionSpecial] DESC)
    INCLUDE([Id]);

GO

CREATE TABLE [NuFridge].[User] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [Username]       NVARCHAR (100)  NOT NULL,
    [DisplayName]    NVARCHAR (255)  NOT NULL,
    [EmailAddress]   NVARCHAR (4000) NOT NULL,
    [LastUpdated]    DATETIME2 (7)   NOT NULL,
    [IsActive]       BIT             NOT NULL,
    [IsService]      BIT             NOT NULL,
    [PasswordSalt]   NVARCHAR (MAX)  NOT NULL,
    [PasswordHashed] NVARCHAR (MAX)  NOT NULL,
	CONSTRAINT [PK_User_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_UserUsernameUnique] UNIQUE NONCLUSTERED ([Username] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_NuFridgePackageDownload_DownloadedAt]
    ON [NuFridge].[PackageDownload]([DownloadedAt])
	INCLUDE(Id, FeedId, PackageId, PackageIdHash, VersionMajor, VersionMinor, VersionBuild, VersionRevision, VersionSpecial, UserAgent, IPAddress)
GO

CREATE NONCLUSTERED INDEX [IX_NuFridgePackage_Published]
    ON [NuFridge].[Package]([Published] DESC)
	INCLUDE([Id], [Version]);
GO


CREATE TABLE [NuFridge].[UserRole] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserRole_Id] PRIMARY KEY CLUSTERED,
[Name] NVARCHAR(100) NOT NULL
)
ALTER TABLE [NuFridge].[UserRole] ADD CONSTRAINT [UQ_UserRoleNameUnique] UNIQUE([Name])



CREATE TABLE [NuFridge].[FeedGroup] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (100) NOT NULL, 
    CONSTRAINT [PK_FeedGroup] PRIMARY KEY ([Id])
);
GO

ALTER TABLE [NuFridge].[Feed]
ADD FOREIGN KEY ([GroupId])
REFERENCES [NuFridge].[FeedGroup](Id)
GO

INSERT INTO [NuFridge].[FeedGroup] ([Name])
VALUES ('Default')
GO

CREATE PROCEDURE [NuFridge].[GetPackageDownloadCount]
@feedId int
AS

SELECT DISTINCT COUNT_BIG(pd.Id) FROM [NuFridge].[PackageDownload] as pd  WITH(NOLOCK)
INNER JOIN [NuFridge].[Package] as pkg  WITH(NOLOCK) on pkg.IdHash = pd.PackageIdHash AND pkg.FeedId = pd.FeedId
INNER JOIN [NuFridge].[Feed] as fd  WITH(NOLOCK) on fd.Id = pd.FeedId
WHERE pd.FeedId = @feedId
GO
