PRINT 'Creating sp_DropNuFridgePackageTable stored procedure'
IF OBJECT_ID('sp_DropNuFridgePackageTable', 'P') IS NOT NULL
    DROP PROCEDURE sp_DropNuFridgePackageTable
GO
CREATE PROC sp_DropNuFridgePackageTable
  @feedId	int
AS
BEGIN

DECLARE @tableName NVARCHAR(MAX)
DECLARE @query NVARCHAR(MAX)

set @tableName = 'Package_' + CONVERT(NVARCHAR(MAX),@feedId)

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'NuFridge' 
                 AND  TABLE_NAME = @tableName))
BEGIN

set @query = 'DROP TABLE [NuFridge].[' + @tableName + ']'

execute sp_executesql @query
END
END
GO


PRINT 'Creating sp_CreateNuFridgePackageTable stored procedure'
IF OBJECT_ID('sp_CreateNuFridgePackageTable', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreateNuFridgePackageTable
GO
CREATE PROC sp_CreateNuFridgePackageTable
  @feedId	int,
  @tableName NVARCHAR(MAX) output
AS
BEGIN

DECLARE @query NVARCHAR(MAX)

set @tableName = 'Package_' + CONVERT(NVARCHAR(MAX),@feedId)

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'NuFridge' 
                 AND  TABLE_NAME = @tableName))
BEGIN

set @query = 'CREATE TABLE [NuFridge].[' + @tableName + '](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PackageId] [nvarchar](4000) NOT NULL,
	[Title] [nvarchar](4000) NULL,
	[Version] [nvarchar](4000) NOT NULL,
	[VersionMajor] [int] NOT NULL,
	[VersionMinor] [int] NOT NULL,
	[VersionBuild] [int] NOT NULL,
	[VersionRevision] [int] NOT NULL,
	[VersionSpecial] [nvarchar](4000) NULL,
	[IsAbsoluteLatestVersion] [bit] NOT NULL,
	[IsLatestVersion] [bit] NOT NULL,
	[IsPrerelease] [bit] NOT NULL,
	[Listed] [bit] NOT NULL,
	[Copyright] [nvarchar](4000) NULL,
	[LastUpdated] [datetime2](7) NULL,
	[LicenseUrl] [nvarchar](max) NULL,
	[ProjectUrl] [nvarchar](max) NULL,
	[RequireLicenseAcceptance] [bit] NOT NULL,
	[Tags] [nvarchar](max) NULL,
	[Owners] [nvarchar](max) NULL,
	[IconUrl] [nvarchar](max) NULL,
	[Authors] [nvarchar](4000) NULL,
	[Description] [nvarchar](max) NULL,
	[ReleaseNotes] [nvarchar](max) NULL,
	[Summary] [nvarchar](max) NULL,
	[Hash] [nvarchar](4000) NOT NULL,
	[DownloadCount] [int] NOT NULL DEFAULT ((0)),
	[Created] [datetime2](7) NULL,
	[Published] [datetime2](7) NOT NULL,
	[Dependencies] [nvarchar](max) NULL,
	[SupportedFrameworks] [nvarchar](max) NULL,
	[DevelopmentDependency] [bit] NOT NULL,
	[VersionDownloadCount] [int] NOT NULL,
	[Language] [nvarchar](4000) NULL,
	[ReportAbuseUrl] [nvarchar](max) NULL,
 CONSTRAINT [PK_Package_Id_' + CONVERT(NVARCHAR(MAX),@feedId) + '] PRIMARY KEY ([Id] asc)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]'

execute sp_executesql @query
END
END
GO

DECLARE @Id INT
DECLARE @ReturnValue NVARCHAR(MAX)
DECLARE @query NVARCHAR(MAX)

PRINT 'Getting a list of existing feeds'

SELECT * INTO   #PackagesTemp FROM   [NuFridge].[Feed]

DECLARE @tableName NVARCHAR(MAX)


WHILE EXISTS(SELECT * FROM #PackagesTemp)
Begin

    SELECT TOP 1 @Id = Id FROM #PackagesTemp

	PRINT 'Creating new packages table for feed id ' + CONVERT(NVARCHAR(MAX),@Id)

	EXECUTE sp_CreateNuFridgePackageTable @Id, @tableName OUTPUT;

	PRINT 'Created new packages table for feed id ' + CONVERT(NVARCHAR(MAX),@Id) + ' called [NuFridge].[' + @tableName + ']' 

	if (@tableName IS NULL)
	BEGIN
	RAISERROR('Stored proc sp_CreateNuFridgePackageTable did not return a table name',16,1)
	RETURN
	END

	set @query = 'INSERT INTO [NuFridge].[' + @tableName + '] ([PackageId]
           ,[Title]
           ,[Version]
           ,[VersionMajor]
           ,[VersionMinor]
           ,[VersionBuild]
           ,[VersionRevision]
           ,[VersionSpecial]
           ,[IsAbsoluteLatestVersion]
           ,[IsLatestVersion]
           ,[IsPrerelease]
           ,[Listed]
           ,[Copyright]
           ,[LastUpdated]
           ,[LicenseUrl]
           ,[ProjectUrl]
           ,[RequireLicenseAcceptance]
           ,[Tags]
           ,[Owners]
           ,[IconUrl]
           ,[Authors]
           ,[Description]
           ,[ReleaseNotes]
           ,[Summary]
           ,[Hash]
           ,[DownloadCount]
           ,[Created]
           ,[Published]
           ,[Dependencies]
           ,[SupportedFrameworks]
           ,[DevelopmentDependency]
           ,[VersionDownloadCount]
           ,[Language]
           ,[ReportAbuseUrl])
	SELECT [PackageId]
           ,[Title]
           ,[Version]
           ,[VersionMajor]
           ,[VersionMinor]
           ,[VersionBuild]
           ,[VersionRevision]
           ,[VersionSpecial]
           ,[IsAbsoluteLatestVersion]
           ,[IsLatestVersion]
           ,[IsPrerelease]
           ,[Listed]
           ,[Copyright]
           ,[LastUpdated]
           ,[LicenseUrl]
           ,[ProjectUrl]
           ,[RequireLicenseAcceptance]
           ,[Tags]
           ,[Owners]
           ,[IconUrl]
           ,[Authors]
           ,[Description]
           ,[ReleaseNotes]
           ,[Summary]
           ,[Hash]
           ,[DownloadCount]
           ,[Created]
           ,[Published]
           ,[Dependencies]
           ,[SupportedFrameworks]
           ,[DevelopmentDependency]
           ,[VersionDownloadCount]
           ,[Language]
           ,[ReportAbuseUrl]
	FROM [NuFridge].[Package]
	WHERE FeedId = ' + CONVERT(NVARCHAR(MAX),@Id)

	PRINT 'Importing existing packages from [NuFridge].[Package] into [NuFridge].[' + @tableName + ']' 

	execute sp_executesql @query

	PRINT 'Imported existing packages from [NuFridge].[Package] into [NuFridge].[' + @tableName + ']' 

    DELETE #PackagesTemp WHERE Id = @Id

	DROP TABLE #PackagesTemp

END
GO
PRINT 'Dropping the [NuFridge].[Package] table'
DROP TABLE [NuFridge].[Package]
GO