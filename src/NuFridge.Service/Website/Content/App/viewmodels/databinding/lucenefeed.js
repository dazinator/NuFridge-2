define(['knockoutvalidation'], function (validation) {


    ko.validation.init({
        registerExtenders: true,
        insertMessages: false,
    });

    window.LuceneFeed = function (config) {
        var self = this, data;

        // your default structure goes here
        data = $.extend({
            name: ko.observable("").extend({
                required: true,
                minLength: 4,
                maxLength: 64,
                pattern: { message: 'Only alphanumeric characters are allowed in the feed name', params: /^[A-Za-z\d\s]+$/ }
            }),
            id: ko.observable(),
            groupId: ko.observable(),
            runPackageCleaner: ko.observable(),
            keepXNumberOfPackageVersions: ko.observable(),
            packages: ko.observableArray()
        }, config);

        ko.mapping.fromJS(data, {}, self);


    };

    window.LuceneFeed.mapping = {
        create: function (options) {

            var fd = new LuceneFeed(options.data);

            fd.viewFeedUrl = ko.computed(function () {
                return '#feeds/' + fd.id();
            });

            return fd;
        }

    };
});