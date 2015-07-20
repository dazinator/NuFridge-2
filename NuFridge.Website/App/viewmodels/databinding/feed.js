define(['knockoutvalidation'], function () {
    return function (config) {
        var self = {}, data;

        data = $.extend({
            Name: ko.observable("").extend({
                required: {message: 'The feed name is mandatory.'},
                minLength: {message: 'The feed name must be at least 3 characters long.', params: 3}
            }),
            Id: ko.observable(0),
            ApiKey: ko.observable(""),
            HasApiKey: ko.observable(false),
            RootUrl: ko.observable(false)
        }, config);

        data = $.extend({
            viewFeedUrl: function () {
                var id;

                if (typeof(data.Id) === "function") {
                    id = data.Id();
                } else {
                    id = data.Id;
                }

                return '#feeds/view/' + id;
            },
            GetLegacyPushPackagesUrl: function() {
                return data.RootUrl() + "/api/packages";
            },
            GetPushPackagesUrl: function() {
                return data.RootUrl() + "/api/v2/package";
            },
            GetODataUrl: function() {
                return data.RootUrl() + "/api/v2";
            },
            GetLegacyODataUrl: function() {
                return data.RootUrl() + "/api/odata";
            },
            GetODataMetadataUrl: function() {
                return data.RootUrl() + "/api/v2/$metadata";
            },
            GetLegacyODataMetadataUrl: function () {
                return data.RootUrl() + "/api/odata/$metadata";
            },
            GetODataSearchUrl: function () {
                return data.RootUrl() + "/api/v2/Search()";
            },
            GetLegacyODataSearchUrl: function () {
                return data.RootUrl() + "/api/odata/Search()";
            },
            GetODataPackageUrl: function () {
                return data.RootUrl() + "/api/v2/Packages(Id='{PackageId}',Version='{PackageVersion}')";
            },
            GetLegacyODataPackageUrl: function () {
                return data.RootUrl() + "/api/odata/Packages(Id='{PackageId}',Version='{PackageVersion}')";
            },
            GetODataPackagesUrl: function () {
                return data.RootUrl() + "/api/v2/Packages()";
            },
            GetLegacyODataPackagesUrl: function () {
                return data.RootUrl() + "/api/odata/Packages()";
            },
            GetODataBatchUrl: function () {
                return data.RootUrl() + "/api/v2/$batch";
            },
            GetLegacyODataBatchUrl: function () {
                return data.RootUrl() + "/api/odata/$batch";
            },
            GetDownloadUrl: function() {
                return data.RootUrl() + "/api/v2/packages/{id}/{version}";
            },
            GetDeleteUrl: function() {
                return data.RootUrl() + "/api/v2/package/{id}/{version}";
            },
            GetFindPackagesByIdUrl: function() {
                return data.RootUrl() + "/api/v2/FindPackagesById()";
            },
            GetLegacyFindPackagesByIdUrl: function () {
                return data.RootUrl() + "/api/odata/FindPackagesById()";
            },
            GetTabCompletionPackageIdsUrl: function () {
                return data.RootUrl() + "/api/v2/package-ids";
            },
            GetTabCompletionPackageVersionsUrl: function () {
                return data.RootUrl() + "/api/v2/package-versions/{packageId}";
            },
            GetGetUpdatesUrl: function() {
                return data.RootUrl() + "/api/v2/GetUpdates()";
            },
            GetLegacyGetUpdatesUrl: function () {
                return data.RootUrl() + "/api/odata/GetUpdates()";
            },
            GetSymbolsUrl: function () {
                return data.RootUrl() + "/api/v2/symbols";
            },
            GetLegacySymbolsUrl: function () {
                return data.RootUrl() + "/api/odata/symbols";
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});