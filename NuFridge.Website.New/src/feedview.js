import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';

@inject(HttpClient)
export class Feeds {

    feed = null;
    feedName = "";

    constructor(http) {
        this.http = http;
    }

    activate(params, routeConfig) {
        var self = this;

        var feedId = params.id;

        this.http.get("/api/feeds/" + feedId).then(message => {
            self.feed = JSON.parse(message.response);
            routeConfig.navModel.setTitle("Viewing " + self.feed.Name);
            self.feedName = self.feed.Name;
        });
    }

    attached() {
        $(".feedMenu .item").tab();
    }
}