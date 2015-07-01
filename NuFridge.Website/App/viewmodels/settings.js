define(['plugins/router', 'databinding-systeminfo', 'api', 'timeago', 'moment', 'auth'], function (router, systeminfo, api, timeago, moment, auth) {
    var ctor = function () {
        var self = this;
        self.systemInfo = ko.observable(systeminfo());
        self.systemInfoUpdatedAt = ko.observable();
        self.systemInfoStartedAt = ko.observable();
        self.formattedStartDate = ko.observable();
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $('.titlePopup')
            .popup({
                inline: true,
                hoverable: true,
                position: 'right center'
            });

        $(".settingMenu .item").tab();
    };

    function zeropad(num) {
        return ((num < 10) ? '0' : '') + num;
    };

    function iso8601(value) {
        var date = value;

        if (typeof date === "string") {
            date = new Date(date);
        }

        return date.getUTCFullYear()
          + "-" + zeropad(date.getUTCMonth() + 1)
          + "-" + zeropad(date.getUTCDate())
          + "T" + zeropad(date.getUTCHours())
          + ":" + zeropad(date.getUTCMinutes())
          + ":" + zeropad(date.getUTCSeconds()) + "Z";
    };

    ctor.prototype.refreshPageData = function() {
        var self = this;

        $.ajax({
            url: api.get_diagnostics,
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return systeminfo(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.systemInfo);

            self.systemInfoUpdatedAt(iso8601(self.systemInfo().LastUpdated()));
            self.systemInfoStartedAt(iso8601(self.systemInfo().StartDate()));
            self.formattedStartDate(moment(self.systemInfo().StartDate()).format('DD/MM/YYYY HH:mm:ss'));
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.manualRefreshPageData = function() {
        var self = this;

        self.refreshPageData();
    };

    ctor.prototype.activate = function() {
        var self = this;

        self.refreshPageData();
    };

    return ctor;
});