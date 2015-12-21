import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser)
export class Feedgroup {

    constructor(router, http, auth, authUser) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
        this.isNew = true;
    }
    activate(params) {
        var self = this;
        self.GroupId = params.id;
    }
    createSaveClick() {
        var self = this;

        if (self.isNew) {
            self.http.post("/api/feedgroups", self.feedGroup).then(message => {
                    self.router.navigate("feeds");
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        } else {
            self.http.put("/api/feedgroups/" + self.GroupId, self.feedGroup).then(message => {
                    self.router.navigate("feeds");
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }
    attached() {
        var self = this;
        if (self.router.currentInstruction.config.route.indexOf("/create") > 0) {
            self.pageTitle = "Create Feed Group";
        } else {
            self.isNew = false;
            self.http.get("/api/feedgroups/" + self.GroupId).then(message => {
                    self.feedGroup = JSON.parse(message.response);
                    self.pageTitle = self.feedGroup.Name;
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }
}