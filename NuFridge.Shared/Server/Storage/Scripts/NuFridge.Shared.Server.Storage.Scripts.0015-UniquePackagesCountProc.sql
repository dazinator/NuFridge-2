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