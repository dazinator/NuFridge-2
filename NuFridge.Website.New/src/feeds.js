import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'aurelia-auth';
import {authUser} from './authuser';
import {Claims} from './claims';

@inject(Router, HttpClient, AuthService, authUser)
export class Feeds {
    feedGroups = new Array();
    addFeedGroup(e){
        this.router.navigate("feedgroup/create");
    }

    constructor(router, http, auth, authUser) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
    }

    addFeedClick(group) {
        this.router.navigate("feeds/create/" + group.Id);
    }

    groupClick(group) {
        this.router.navigate("feedgroup/view/" + group.Id);
    }

    feedClick(feed) {
        this.router.navigate("feeds/view/" + feed.Id);
    }

    attached() {
        var self = this;

        var searchOptions = {
            apiSettings: {
                url: '/api/feeds/search/?name={query}',
                beforeXHR: function (xhr) {
                    xhr.setRequestHeader('Authorization', 'Token ' + self.auth.auth.getToken());
                }
            },
            type: 'category'
        };

        $('.ui.search.feedSearch').search(searchOptions);
    }

    activate() {
        var self = this;

        
        self.canViewPage = self.authUser.hasClaim(Claims.CanViewFeeds, Claims.SystemAdministrator);
        self.canInsertFeed =  self.authUser.hasClaim(Claims.CanInsertFeed, Claims.SystemAdministrator);
        self.canUpdateFeedGroup = self.authUser.hasClaim(Claims.CanUpdateFeedGroup, Claims.SystemAdministrator);

        if (self.canViewPage) {
            this.http.get("/api/feeds").then(message => {
                    self.feedGroups = JSON.parse(message.response);
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout(loginRoute);
                    }
                });
        }
    }
}