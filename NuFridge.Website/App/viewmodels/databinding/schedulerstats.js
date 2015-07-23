define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Failed: ko.observable(),
            Scheduled: ko.observable(),
            Succeeded: ko.observable(),
            Processing: ko.observable()
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});