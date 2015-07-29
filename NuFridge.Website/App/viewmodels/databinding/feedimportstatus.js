define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            FeedId: ko.observable(0),
            RemainingCount: ko.observable(0),
            CompletedCount: ko.observable(0),
            FailedCount: ko.observable(0),
            TotalCount: ko.observable(0),
            IsCompleted: ko.observable(false)
        }, config);

        data = $.extend({
            ProgressCount: function () {
                return data.CompletedCount() + data.FailedCount();
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});