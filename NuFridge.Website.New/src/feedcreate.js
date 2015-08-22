import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {Router} from 'aurelia-router';

@inject(HttpClient, Router)
export class FeedCreate {

    feed = {
        Id: 0,
        Name: "",
        ApiKey: "",
        GroupId: 0,
        Description: ""
    };

    feedName = "";

    constructor(http, router) {
        this.http = http;
        this.router = router;
    }

    insertFeed() {
        var self = this;

        this.http.post("/api/feeds/", self.feed).then(message => {
            self.feed = JSON.parse(message.response);
            self.router.navigate("feeds/view/" + self.feed.Id);
        });
    }


    activate(params, routeConfig) {
        var self = this;
        self.routeConfig = routeConfig;

        var groupId = params.id;
        self.feed.GroupId = groupId;
    }

    attached() {

    }
}