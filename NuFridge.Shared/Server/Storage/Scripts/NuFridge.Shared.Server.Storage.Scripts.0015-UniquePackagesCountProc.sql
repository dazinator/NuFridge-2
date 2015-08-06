CREATE PROCEDURE [NuFridge].[GetUniquePackageCount]
	@feedId int
AS
with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, PackageIdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE pk.FeedId = @feedId
)

SELECT COUNT(DISTINCT PackageId) FROM CTE as ctee
WHERE (rn = 1 OR (SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.PackageId = PackageId AND Listed = 1) = rn) AND Listed = 1
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
      ROW_NUMBER() OVER (PARTITION BY FeedId, PackageIdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE pk.FeedId = @feedId AND (@partialId IS NULL OR PackageId LIKE '%' + @partialId + '%')
)

SELECT rn, * FROM CTE as orig
WHERE (@includePrerelease = 1 AND orig.rn = 1 AND orig.Listed = 1) OR (@includePrerelease = 0 AND orig.rn = (SELECT TOP(1) rn FROM cte as ctee where ctee.IsPrerelease = 0 AND ctee.PackageId = orig.PackageId AND ctee.Listed = 1))
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
      ROW_NUMBER() OVER (PARTITION BY FeedId, PackageIdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE pk.FeedId = @feedId AND pk.PackageId = @packageId AND (@includePrerelease = 1 OR pk.IsPrerelease = 0)
)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.PackageId = PackageId AND Listed = 1
) = rn THEN 1 ELSE 0 END, * FROM cte as ctee
GO