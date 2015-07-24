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

        var name = data.Job.Method.ClassName;

        name = name.substring(name.lastIndexOf('.') + 1);

        data.Name = ko.observable(name);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});