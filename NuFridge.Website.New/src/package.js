import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import moment from 'moment';
import {authUser} from './authuser';
import {Claims} from './claims';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';

@inject(HttpClient, Router, authUser, errorParser)
export class Package {

    constructor(http, router, authUser, errorParser) {
        this.http = http;
        this.router = router;
        this.authUser = authUser;
        this.errorParser = errorParser;
    }

    feed = null;
    pkg = null;
    versionsOfPackage = new Array();

    activate(params, routeConfig) {
        var self = this;
        self.routeConfig = routeConfig;

        var feedId = params.id;
        var packageId = params.packageid;
        var packageVersion = params.packageversion;

        self.canViewPage = self.authUser.hasClaim(Claims.CanViewPackages, Claims.SystemAdministrator);

        if (self.canViewPage) {
            self.http.get("/api/feeds/" + feedId).then(message => {
                    self.feed = JSON.parse(message.response);

                    self.http.get("/feeds/" + self.feed.Name + "/api/v2/Packages(Id='" + packageId + "',Version='" + packageVersion + "')").then(message => {
                        self.package = JSON.parse(message.response).d;
                    });

                    self.http.get("/feeds/" + self.feed.Name + "/api/v2/Search()?$filter=Id eq '" + packageId + "'").then(message => {
                        self.versionsOfPackage = JSON.parse(message.response).d.results;
                    });
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout(loginRoute);
                    }
                });
        }
    }

    attached() {

    }
}