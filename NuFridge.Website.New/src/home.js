import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';


@inject(HttpClient, AuthService)
export class Home {

    heading = 'Dashboard';
    dashboard = null;
    chartist = null;


    constructor(http, auth) {
        this.http = http;
        this.auth = auth;
    }

    attached() {
        var self = this;

    }

    loadGraphs() {
        var self = this;

        self.http.get("/api/stats/feedpackagecount").then(message => {
            self.feedpackagecount = JSON.parse(message.response);

            var data = {
                labels: ['abc11', 'hsdhdhggh', '444444444444444', 'def'],
                series: [
                    [5, 409, 308008, 7000041]
                ]
            };

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

            self.chartist.Bar('.ct-chart.packagesChart', self.feedpackagecount, options, overrides);
            self.chartist.Bar('.ct-chart.downloadsChart', self.feedpackagecount, options, overrides);
        });
    }
    

    activate() {
        var self = this;

        this.http.get("/api/dashboard").then(message => {
            self.dashboard = JSON.parse(message.response);
        }, function(message) {
            if (message.statusCode === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            } 
        });

        System.import('chartist').then((chartist) => {
            self.chartist = chartist;
            self.loadGraphs();
        });


    }
}