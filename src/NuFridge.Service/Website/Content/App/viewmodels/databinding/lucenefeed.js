define(function () {
    return function (config) {
        var self = this, data;

        data = $.extend({
            name: ko.observable(""),
            id: ko.observable(),
            groupId: ko.observable(),
            runPackageCleaner: ko.observable(false),
            keepXNumberOfPackageVersions: ko.observable(0),
            apiKey: ko.observable(""),
            packages: ko.observableArray()
        }, config);

        data = $.extend({
            viewFeedUrl: function () {
                return '#feeds/view/' + data.id;
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});