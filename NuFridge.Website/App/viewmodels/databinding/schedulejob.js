define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Id: ko.observable(),
            Cron: ko.observable(),
            Job: ko.observable(),
            NextExecution: ko.observable(),
            LastJobState: ko.observable(),
            LastExecution: ko.observable(),
            TimeZoneId: ko.observable()
        }, config);

        if (data.Job) {
            var name = data.Job.Method.ClassName;

            name = name.substring(name.lastIndexOf('.') + 1);

            data.Name = ko.observable(name);
        } else {
            data.Name = ko.observable("Unknown");
        }

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});