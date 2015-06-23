define(['knockoutvalidation'], function () {
    return function (config) {
        var self = {}, data;

        data = $.extend({
            Name: ko.observable("").extend({
                required: {message: 'The feed name is mandatory.'},
                minLength: {message: 'The feed name must be at least 3 characters long.', params: 3}
            }),
            Id: ko.observable(0),
            ApiKey: ko.observable("")
        }, config);

        data.errors = ko.validation.group({
            Id: data.Id
        });

        data = $.extend({
            viewFeedUrl: function () {
                return '#feeds/view/' + data.Id;
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});