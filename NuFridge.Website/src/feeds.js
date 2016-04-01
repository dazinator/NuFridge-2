import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
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

    editGroupClick(group) {
        this.router.navigate("feedgroup/view/" + group.Id);
    }

    attached() {
        $('.ui.button.actionButton')
            .popup({
                inline: true,
                hoverable: true,
                position: 'bottom left',
                delay: {
                    show: 50,
                    hide: 80
                }
            });
    }

    activate() {
        var self = this;

        
        self.canViewPage = self.authUser.hasClaim(Claims.CanViewFeeds, Claims.SystemAdministrator);
        self.canInsertFeed =  self.authUser.hasClaim(Claims.CanInsertFeed, Claims.SystemAdministrator);
        self.canUpdateFeedGroup = self.authUser.hasClaim(Claims.CanUpdateFeedGroup, Claims.SystemAdministrator);
        self.canInsertFeedGroup = self.authUser.hasClaim(Claims.CanInsertFeedGroup, Claims.SystemAdministrator);

        if (self.canViewPage) {
            self.http.fetch("api/feeds").then(response => response.json()).then(message => {
                    self.feedGroups = message;
                },
                function(message) {
                    if (message.status === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }
}