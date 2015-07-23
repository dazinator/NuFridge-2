define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Domain: ko.observable(),
            MachineName: ko.observable(),
            LogicalProcessorCount: ko.observable(),
            ProcessorCount: ko.observable(),
            PartOfDomain: ko.observable(),
            SystemType: ko.observableArray(),
            StartDate: ko.observable(),
            ExecutablePath: ko.observable(),
            ServerThreadCount: ko.observable(),
            SchedulerThreadCount: ko.observable(),
            WorkingSetSize: ko.observable(),
            ProcessName: ko.observable(),
            FreeDiskSpace: ko.observable(),
            LastUpdated: ko.observable()
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});