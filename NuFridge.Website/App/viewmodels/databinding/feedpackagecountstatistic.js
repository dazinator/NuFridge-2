define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedName: ko.observable(""),
            PackageCount: ko.observable(0),
            Color: ko.observable("")
        }, config);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});