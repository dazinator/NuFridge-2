import 'chart'
import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';

@inject(HttpClient, AuthService)
export class Home {
    heading = 'Dashboard';
    dashboard = null;
    constructor(http, auth) {
        this.http = http;
        this.auth = auth;
    }

    attached() {
        var randomScalingFactor = function () {
            return Math.round(Math.random() * 100);
        };

        var ctx = document.getElementById("packageDownloadCanvas").getContext("2d");
        var myBar = new Chart(ctx).Line({
            labels: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
            datasets: [
              {
                  label: 'Package Downloads',
                  fillColor: "rgba(74, 137, 220,0.5)",
                  strokeColor: "rgba(74, 137, 220,0.8)",
                  highlightFill: "rgba(220,220,220,0.75)",
                  highlightStroke: "rgba(220,220,220,1)",
                  data: [randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor()]
              }
            ]
        }, {
            responsive: true
        });

        var ctx2 = document.getElementById("packageUploadCanvas").getContext("2d");
        var myBar2 = new Chart(ctx2).Line({
            labels: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
            datasets: [
              {
                  label: 'Package Uploads',
                  fillColor: "rgba(74, 137, 220,0.5)",
                  strokeColor: "rgba(74, 137, 220,0.8)",
                  highlightFill: "rgba(220,220,220,0.75)",
                  highlightStroke: "rgba(220,220,220,1)",
                  data: [randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor()]
              }
            ]
        }, {
            responsive: true
        });

        var ctx3 = document.getElementById("feedPackageCountCanvas").getContext("2d");
        var myBar3 = new Chart(ctx3).Bar({
            labels: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
            datasets: [
              {
                  label: "All Packages",
                  fillColor: "rgba(74, 137, 220,0.5)",
                  strokeColor: "rgba(74, 137, 220,0.8)",
                  highlightFill: "rgba(220,220,220,0.75)",
                  highlightStroke: "rgba(220,220,220,1)",
                  data: [randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor()]
              },
              {
                  label: "Unique Packages",
                  fillColor: "rgba(246, 187, 66, 0.5)",
                  strokeColor: "rgba(246, 187, 66, 0.8)",
                  highlightFill: "rgba(220,220,220,0.75)",
                  highlightStroke: "rgba(220,220,220,1)",
                  data: [randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor(), randomScalingFactor()]
              }
            ]
        }, {
            responsive: true
        });
    }

    ;

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
    }
}