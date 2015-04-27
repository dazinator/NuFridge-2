define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            domain: ko.observable(),
            machineName: ko.observable(),
            logicalProcessorCount: ko.observable(),
            processorCount: ko.observable(),
            partOfDomain: ko.observable(),
            systemType: ko.observableArray(),
            startDate: ko.observable(),
            executablePath: ko.observable(),
            threadCount: ko.observable(),
            workingSetSize: ko.observable(),
            processName: ko.observable(),
            uptime: ko.observable()
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});