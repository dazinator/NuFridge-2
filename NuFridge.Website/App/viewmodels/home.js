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
            // Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
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


            var options = {
                legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>",
                animation : false

            };



            var ctx = document.getElementById("feedPackageCountChart").getContext("2d");
            self.feedPackageCountChart = new Chart(ctx).Pie(data, options);

            $(window).on('resize.feedpackagechart', function () {
                self.feedPackageCountChart.destroy();
                $('#feedPackageCountChart').css("height", "");
                $('#feedPackageCountChart').css("width", "");
                var width = $('#feedPackageCountChart').parent().width();
                $('#feedPackageCountChart').attr("width", width);
                self.feedPackageCountChart = new Chart(ctx).Pie(data, options);
            });

            var sttrrr = self.feedPackageCountChart.generateLegend();

            $("#feedPackageCountChartLegend").html(sttrrr);

            self.feedPackageCountDataUpdatedAt(iso8601(new Date()));

        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            // Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
        });
    };

    ctor.prototype.configureFeedDownloadCountChart = function () {
        var self = this;

        var data = {
            labels: [],
            datasets: [
            {
                label: "Downloads",
                fillColor: "rgba(220,220,220,0.5)",
                strokeColor: "rgba(220,220,220,0.8)",
                highlightFill: "rgba(220,220,220,0.75)",
                highlightStroke: "rgba(220,220,220,1)",
                data: []
            }]
        };

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

                data.labels.push(feedStat.FeedName);
                data.datasets[0].data.push(feedStat.DownloadCount);
            });


            var options = {animation : false};

            var ctx = document.getElementById("feedDownloadCountChart").getContext("2d");
            var width = $('#feedDownloadCountChart').parent().width();
            $('#feedDownloadCountChart').attr("width", width);



            self.feedDownloadCountChart = new Chart(ctx).Bar(data, options);

            $(window).on('resize.feeddownloadchart', function () {
                self.feedDownloadCountChart.destroy();
                $('#feedDownloadCountChart').css("height", "");
                $('#feedDownloadCountChart').css("width", "");
                var width = $('#feedDownloadCountChart').parent().width();
                $('#feedDownloadCountChart').attr("width", width);
                self.feedDownloadCountChart = new Chart(ctx).Bar(data, options);
            });

            self.feedDownloadCountDataUpdatedAt(iso8601(new Date()));


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            // Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
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