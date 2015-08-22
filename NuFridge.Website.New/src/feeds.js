import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';

@inject(Router, HttpClient)
export class Feeds {
    feedGroups = new Array();
    addFeedGroup(e){
        this.router.navigate("feedgroup/create");
    }

    constructor(router, http) {
        this.router = router;
        this.http = http;
    }

    addFeedClick(group) {
        this.router.navigate("feeds/create/" + group.Id);
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