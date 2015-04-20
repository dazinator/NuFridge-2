define(function () {
    return function (config) {
        var self = this, data;

        data = $.extend({
            firstName: ko.observable(""),
            lastName: ko.observable(""),
            email: ko.observable(""),
            emailConfirmed: ko.observable(false),
            id: ko.observable(),
            userName: ko.observable("")
        }, config);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});