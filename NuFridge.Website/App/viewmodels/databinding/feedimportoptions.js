define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedUrl: ko.observable("https://www.nuget.org/api/v2"),
            SpecificPackageId: ko.observable(""),
            SearchPackageId: ko.observable(""),
            IncludePrerelease: ko.observable(true),
            Version: ko.observable(""),
            VersionSelector: ko.observable(1),
            CheckLocalCache: ko.observable(true)
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});