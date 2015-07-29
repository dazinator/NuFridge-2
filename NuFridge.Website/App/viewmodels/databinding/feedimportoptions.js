define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedUrl: ko.observable(""),
            SpecificPackageId: ko.observable(""),
            SearchPackageId: ko.observable(""),
            IncludePrerelease: ko.observable(true),
            Version: ko.observable(""),
            VersionSelector: ko.observable(1)
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});