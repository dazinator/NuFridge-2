ALTER PROCEDURE [NuFridge].[GetAllPackages]
	@feedId int
AS

with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY FeedId, PackageIdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE @feedId IS NULL OR pk.FeedId = @feedId
)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.PackageId = PackageId AND Listed = 1 ORDER BY rn ASC
) = rn THEN 1 ELSE 0 END, * FROM cte as ctee