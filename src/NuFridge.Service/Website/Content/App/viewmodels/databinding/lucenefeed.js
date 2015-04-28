define(['knockoutvalidation'], function () {
    return function (config) {
        var self = {}, data;

        data = $.extend({
            name: ko.observable("").extend({
                required: {message: 'The feed name is mandatory.'},
                minLength: {message: 'The feed name must be at least 3 characters long.', params: 3}
            }),
            id: ko.observable(""),
            synchronizeOnStart: ko.observable(true),
            enablePackageFileWatcher: ko.observable(false),
            groupPackageFilesById: ko.observable(true),
            allowPackageOverwrite: ko.observable(true),
            apiKey: ko.observable(""),
            host: ko.observable('localhost'),
            virtualDirectory: ko.observable("/"),
            port: ko.observable("80"),
            packages: ko.observableArray()
        }, config);

        data.errors = ko.validation.group({
            id: data.id
        });

        data = $.extend({
            viewFeedUrl: function () {
                return '#feeds/view/' + data.id;
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});