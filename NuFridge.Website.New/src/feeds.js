import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'aurelia-auth';

@inject(Router, HttpClient, AuthService)
export class Feeds {
    feedGroups = new Array();
    addFeedGroup(e){
        this.router.navigate("feedgroup/create");
    }

    constructor(router, http, auth) {
        this.router = router;
        this.http = http;
        this.auth = auth;
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