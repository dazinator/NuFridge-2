import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';


@inject(HttpClient, AuthService)
export class Home {

    heading = 'Dashboard';
    dashboard = null;
    chartist = null;

    isLoadingDashboard = true;
    isLoadingCountChart = true;
    isLoadingDownloadChart = true;


    constructor(http, auth) {
        this.http = http;
        this.auth = auth;
    }

    attached() {
        var self = this;
        self.loadGraphs();
    }

    loadGraphs() {
        var self = this;

        var options = {
            // Default mobile configuration
            stackBars: true,
            axisX: {
                labelInterpolationFnc: function(value) {
                    return value.split(/\s+/).map(function(word) {
                        return word[0];
                    }).join('');
                }
            },
            axisY: {
                offset: 20
            },
            height: '250px'
        };

        var overrides = [
            // Options override for media > 400px
            [
                'screen and (min-width: 400px)', {
                    reverseData: true,
                    horizontalBars: true,
                    axisX: {
                        labelInterpolationFnc: self.chartist.noop
                    },
                    axisY: {
                        offset: 60
                    }
                }
            ],
            // Options override for media > 800px
            [
                'screen and (min-width: 800px)', {
                    stackBars: false,
                    seriesBarDistance: 10
                }
            ],
            // Options override for media > 1000px
            [
                'screen and (min-width: 1000px)', {
                    reverseData: false,
                    horizontalBars: false,
                    seriesBarDistance: 15

                }
            ]
        ];

        self.http.get("/api/stats/feedpackagecount").then(message => {
            self.feedpackagecount = JSON.parse(message.response);
            self.chartist.Bar('.ct-chart.packagesChart', self.feedpackagecount, options, overrides);
            self.isLoadingCountChart = false;
        });

        self.http.get("/api/stats/feeddownloadcount").then(message => {
            self.feeddownloadcount = JSON.parse(message.response);
            self.chartist.Bar('.ct-chart.downloadsChart', self.feeddownloadcount, options, overrides);
            self.isLoadingDownloadChart = false;
        });
    }
    

    activate() {
        var self = this;

        this.http.get("/api/dashboard").then(message => {
            self.dashboard = JSON.parse(message.response);
            self.isLoadingDashboard = false;
        }, function(message) {
            if (message.statusCode === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            } 
        });

        return System.import('chartist').then((chartist) => {
            self.chartist = chartist;
        });
    }
}