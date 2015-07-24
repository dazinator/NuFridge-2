define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Key: ko.observable(),
            Value: ko.observable()
        }, config);

        var name = data.Value.Job.Method.ClassName;

        name = name.substring(name.lastIndexOf('.') + 1);

        data.Name = ko.observable(name);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});