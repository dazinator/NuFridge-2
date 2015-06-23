define(function () {
    return function (config) {
        var self = this, data;

        data = $.extend({
            UserName: ko.observable(""),
            Password: ko.observable("")
        }, config);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});