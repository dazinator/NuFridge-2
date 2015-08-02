define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Counters: ko.observable({
                TotalCount: ko.observable(0),
                ImportedCount: ko.observable(0),
                FailedCount: ko.observable(0)
            }),
            Summary: ko.observable({
                IsCompleted: ko.observable(false),
                ImportedPackages: ko.observableArray(),
                FailedPackages: ko.observableArray()
            })
        }, config);

        data = $.extend({
            ProgressCount: function () {
                if (data.Counters()) {
                    return data.Counters().ImportedCount() + data.Counters().FailedCount();
                }
                return 0;
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});