define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            feedCount: ko.observable(0),
            userCount: ko.observable(0),
            packageCount: ko.observable(0)
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});