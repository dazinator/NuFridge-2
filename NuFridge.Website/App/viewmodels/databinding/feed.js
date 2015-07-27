define(['knockoutvalidation'], function () {
    return function (config) {
        var self = {}, data;

        data = $.extend({
            Name: ko.observable("").extend({
                required: {message: 'The feed name is mandatory.'},
                minLength: {message: 'The feed name must be at least 3 characters long.', params: 3}
            }),
            Id: ko.observable(0),
            ApiKey: ko.observable(""),
            HasApiKey: ko.observable(false),
            RootUrl: ko.observable(false)
        }, config);

        data = $.extend({
            viewFeedUrl: function () {
                var id;

                if (typeof(data.Id) === "function") {
                    id = data.Id();
                } else {
                    id = data.Id;
                }

                return '#feeds/view/' + id;
            },
            GetLegacyPushPackagesUrl: function() {
                return data.RootUrl() + "/api/packages";
            },
            GetPushPackagesUrl: function() {
                return data.RootUrl() + "/api/v2/package";
            },
            GetODataUrl: function() {
                return data.RootUrl() + "/api/v2";
            },
            GetLegacyODataUrl: function() {
                return data.RootUrl() + "/api/odata";
            },
            GetSymbolsUrl: function () {
                return data.RootUrl() + "/api/symbols";
            }
        }, data);

        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});