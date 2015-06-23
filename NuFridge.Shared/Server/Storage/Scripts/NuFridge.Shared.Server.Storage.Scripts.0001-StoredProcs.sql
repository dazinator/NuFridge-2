CREATE PROCEDURE [dbo].[GetPackages](
	@allowPreRelease bit = 0,
	@feedId int = 0,
	@packageId nvarchar(255) = '',
	@latestOnly bit = 0,
	@minRow int = 0,
	@maxRow int = 30,
	@partialMatch bit = 0
) AS
BEGIN	
	WITH PackagesContext AS
	(
		SELECT * FROM 
		(
			SELECT *, ROW_NUMBER() OVER (PARTITION BY PackageId ORDER BY VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC, CASE WHEN VersionSpecial = '' THEN 0 ELSE 1 END, VersionSpecial DESC) Recency,
		    ROW_NUMBER() OVER (ORDER BY PackageId, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC, CASE WHEN VersionSpecial = '' THEN 0 ELSE 1 END, VersionSpecial DESC) AS RowNumber
			FROM [NuFridge].[Package] WHERE ((@allowPreRelease = 1) or (@allowPreRelease = 0 and VersionSpecial = '')) and ((@packageId is null or @packageId = '') or (@partialMatch = 0 and PackageId = @packageId) or (@partialMatch = 1 and PackageId LIKE '%' + @packageId + '%')) and FeedId = @feedId
		) Packages WHERE (@latestOnly = 0 OR (@latestOnly = 1 and Recency = 1))
	)
	SELECT *, (SELECT TC=COUNT(*) FROM PackagesContext) as TotalCount
		FROM PackagesContext
		WHERE RowNumber >= @minRow AND RowNumber <= @maxRow ORDER BY RowNumber
END