import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';

@inject(HttpClient)
export class FeedView {

    feed = null;
    feedName = " ";
    isUpdatingFeed = false;

    constructor(http) {
        this.http = http;
    }

    updateFeed() {
        var self = this;

        self.isUpdatingFeed = true;

        var startDate = new Date();

        this.http.put("/api/feeds/" + self.feed.Id, self.feed).then(message => {
            self.feed = JSON.parse(message.response);
            self.refreshFeedName();

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 1) {
                setTimeout(function() { self.isUpdatingFeed = false; }, (1 - secondsDifference) * 1000);
            } else {
                self.isUpdatedFeed = false;
            }
        });
    }

    clearApiKey() {
        var self = this;
        self.feed.HasApiKey = false;
    }

    activate(params, routeConfig) {
        var self = this;
        self.routeConfig = routeConfig;

        var feedId = params.id;

        this.http.get("/api/feeds/" + feedId).then(message => {
            self.feed = JSON.parse(message.response);
            self.refreshFeedName();
        });
    }

    refreshFeedName() {
        var self = this;

        self.routeConfig.navModel.setTitle("Viewing " + self.feed.Name);
        self.feedName = self.feed.Name;
    }

    attached() {
        $(".feedMenu .item").tab();
    }
}