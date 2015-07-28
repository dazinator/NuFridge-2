define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedId: ko.observable(0),
            RemainingCount: ko.observable(0),
            CompletedCount: ko.observable(0),
            FailedCount: ko.observable(0),
            TotalCount: ko.observable(0)
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});