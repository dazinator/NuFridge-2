define(['knockoutvalidation'], function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedId: ko.observable(0),
            Id: ko.observable(0),
            PackagesDirectory: ko.observable(""),
            SymbolsDirectory: ko.observable(""),
            Directory: ko.observable(""),
            RetentionPolicyEnabled: ko.observable(false),
            MaxPrereleasePackages: ko.observable(0).extend({ required: true }),
            MaxReleasePackages: ko.observable(0).extend({ required: true })
    }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});