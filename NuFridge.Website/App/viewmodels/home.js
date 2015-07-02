define(['plugins/router', 'databinding-dashboard', 'cookie', 'api', 'chart', 'databinding-feedpackagecountstatistic', 'databinding-feeddownloadcountstatistic', 'timeago', 'auth'], function (router, dashboard, cookie, api, chart, feedPackageCount, feedDownloadCount, timeago, auth) {
    var ctor = function () {
        var self = this;
        self.displayName = 'Dashboard';
        self.dashboard = ko.observable(dashboard());
        self.feedPackageCountData = ko.observableArray();
        self.feedDownloadCountData = ko.observableArray();
        self.feedPackageCountDataUpdatedAt = ko.observable();
        self.feedDownloadCountDataUpdatedAt = ko.observable();
        self.feedPackageCountChart = null;
        self.feedDownloadCountChart = null;
    };

    function zeropad(num) {
        return ((num < 10) ? '0' : '') + num;
    };

    function iso8601(date) {
        return date.getUTCFullYear()
          + "-" + zeropad(date.getUTCMonth() + 1)
          + "-" + zeropad(date.getUTCDate())
          + "T" + zeropad(date.getUTCHours())
          + ":" + zeropad(date.getUTCMinutes())
          + ":" + zeropad(date.getUTCSeconds()) + "Z";
    };

    ctor.prototype.activate = function () {
        var self = this;

        $.ajax({
            url: api.get_dashboard,
            headers: new auth().getAuthHttpHeader(),
            cache: false,
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return dashboard(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.dashboard);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.deactivate = function () {
        $(window).off('resize.feedpackagechart');
        $(window).off('resize.feeddownloadchart');
    };

    ctor.prototype.goToFeeds = function () {
        router.navigate("#feeds");
    };


    ctor.prototype.refreshFeedPackageCountChart = function () {
        var self = this;

        if (self.feedPackageCountChart && typeof self.feedPackageCountChart.destory === "function") {
            self.feedPackageCountChart.destory();
        }

        self.configureFeedPackageCountChart();
    };

    ctor.prototype.refreshFeedDownloadCountChart = function () {
        var self = this;

        if (self.feedDownloadCountChart && typeof self.feedDownloadCountChart.destory === "function") {
            self.feedDownloadCountChart.destory();
        }

        self.configureFeedDownloadCountChart();
    };

    ctor.prototype.configureFeedPackageCountChart = function () {
        var self = this;

        var data = [];

        $.ajax({
            url: api.get_stats_feedpackagecount,
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return feedPackageCount(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.feedPackageCountData);

            $.each(self.feedPackageCountData(), function () {
                var feedStat = this;
                data.push({
                    label: feedStat.FeedName,
                    value: feedStat.PackageCount,
                    color: feedStat.Color,
                    labelColor: 'white',
                    labelFontSize: '16'
                });
            });


            var options = { animation: true, responsive: true, animationEasing: "easeOutCubic" };



            var ctx = document.getElementById("feedPackageCountChart").getContext("2d");
            var width = $('#feedPackageCountChart').parent().width();
            $('#feedPackageCountChart').attr("width", width);

            self.feedPackageCountChart = new Chart(ctx).Pie(data, options);

            self.feedPackageCountDataUpdatedAt(iso8601(new Date()));

        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.configureFeedDownloadCountChart = function () {
        var self = this;

        var data = [];

        $.ajax({
            url: api.get_stats_feeddownloadcount,
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return feedDownloadCount(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.feedDownloadCountData);


            $.each(self.feedDownloadCountData(), function () {
                var feedStat = this;
                data.push({
                    label: feedStat.FeedName,
                    value: feedStat.DownloadCount,
                    color: feedStat.Color,
                    labelColor: 'white',
                    labelFontSize: '16'
                });
            });


            var options = { animation: true, responsive: true, animationEasing: "easeOutCubic" };

            var ctx = document.getElementById("feedDownloadCountChart").getContext("2d");
            var width = $('#feedDownloadCountChart').parent().width();
            $('#feedDownloadCountChart').attr("width", width);

            self.feedDownloadCountChart = new Chart(ctx).Pie(data, options);

            self.feedDownloadCountDataUpdatedAt(iso8601(new Date()));


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.configureCharts = function () {

        var self = this;

        self.configureFeedPackageCountChart();
        self.configureFeedDownloadCountChart();
    }

    ctor.prototype.compositionComplete = function () {
        var self = this;

        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);


        $('.cookie.nag')
            .nag({
                key: 'accepts-cookies',
                value: true
            });

        self.configureCharts();

        $('.titlePopup')
            .popup({
                inline: true,
                hoverable: true,
                position: 'right center'
            });
    };

    return ctor;
});