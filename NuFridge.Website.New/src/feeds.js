import {inject} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/custom.css!';
import {Router} from 'aurelia-router';
import {AuthService} from 'aurelia-auth';
import {HttpClient} from 'aurelia-http-client';

@inject(Router, AuthService, HttpClient)
export class Feeds {
    hello = 'Welcome to Aurelia!';
    feedGroups = new Array();
    addFeedGroup(e){
        this.router.navigate("feedgroup/create");
    }

    constructor(router, auth, http) {
        this.router = router;
        this.auth = auth;
        this.http = http;
    }

    feedClick(feed) {
        this.router.navigate("feeds/view/" + feed.Id);
    }

    activate() {
        var self = this;

        this.http.get("/api/feeds").then(message => {
            self.feedGroups = JSON.parse(message.response);
        });
    }
}