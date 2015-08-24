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

SELECT COUNT(DISTINCT Id) FROM CTE as ctee
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
	@version nvarchar(4000)
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
WHERE ctee.[Version] = @version
GO

ALTER PROCEDURE [NuFridge].[GetAllPackages]
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

ALTER TABLE [NuFridge].[Package]
DROP COLUMN DownloadCount
GO

ALTER TABLE [NuFridge].[Package]
DROP COLUMN VersionDownloadCount
GO


DROP TABLE [NuFridge].[User];


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


CREATE TABLE [NuFridge].[FeedGroup] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (100) NOT NULL, 
    CONSTRAINT [PK_FeedGroup] PRIMARY KEY ([Id])
);
GO

INSERT INTO [NuFridge].[FeedGroup] ([Name])
VALUES ('Default')
GO


ALTER TABLE [NuFridge].[Feed]
ADD [GroupId] int
GO

UPDATE [NuFridge].[Feed]
SET GroupId = 1

ALTER TABLE [NuFridge].[Feed]
ADD FOREIGN KEY ([GroupId])
REFERENCES [NuFridge].[FeedGroup](Id)
GO

TRUNCATE TABLE [NuFridge].[User]
GO