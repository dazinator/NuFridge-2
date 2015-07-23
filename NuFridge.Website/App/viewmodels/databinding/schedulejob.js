define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Key: ko.observable(),
            Value: ko.observable()
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});