define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Key: ko.observable(),
            Value: ko.observable()
        }, config);

        if (data.Value.Job) {
            var name = data.Value.Job.Method.ClassName;

            name = name.substring(name.lastIndexOf('.') + 1);

            data.Name = ko.observable(name);
        } else {
            data.Name = ko.observable("Unknown");
        }

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});