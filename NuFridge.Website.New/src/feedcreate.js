import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {Router} from 'aurelia-router';
import {authUser} from './authuser';
import {Claims} from './claims';
import {AuthService} from 'aurelia-auth';

@inject(HttpClient, Router, AuthService, authUser)
export class FeedCreate {

    feed = {
        Id: 0,
        Name: "",
        ApiKey: "",
        GroupId: 0,
        Description: ""
    };

    isCreatingFeed = false;
    feedName = "";

    constructor(http, router, auth, authUser) {
        this.http = http;
        this.router = router;
        this.auth = auth;
        this.authUser = authUser;
    }

    insertFeed() {
        var self = this;

        self.isCreatingFeed = true;

        var startDate = new Date();

        this.http.post("/api/feeds/", self.feed).then(message => {
            self.feed = JSON.parse(message.response);

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 1) {
                setTimeout(function() { self.router.navigate("feeds/view/" + self.feed.Id); }, (1 - secondsDifference) * 1000);
            } else {
                self.router.navigate("feeds/view/" + self.feed.Id);
            }
        },
        function(message) {
            if (message.statusCode === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout(loginRoute);
            }
        });
    }


    activate(params, routeConfig) {
        var self = this;

        var groupId = params.id;
        self.feed.GroupId = groupId;

        self.hasRequiredClaims = self.authUser.hasClaim(Claims.CanInsertFeed, Claims.SystemAdministrator);
    }

    attached() {

    }
}