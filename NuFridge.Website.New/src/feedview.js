import '/styles/timeline.css!';
import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import moment from 'moment';

@inject(HttpClient, Router)
export class FeedView {

    feed = null;
    feedName = " ";
    isUpdatingFeed = false;
    isLoadingHistory = false;
    historyRecords = new Array();

    constructor(http, router) {
        this.http = http;
        this.router = router;
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
        }, message => {
            var t = 1;
        });
    }

    clearApiKey() {
        var self = this;
        self.feed.HasApiKey = false;
    }

    deleteFeed() {
        var self = this;

        this.http.delete("/api/feeds/" + self.feed.Id).then(message => {
            $('#deleteConfirmModal').modal("hide");
            self.router.navigate("feeds");
        }, message => {
            $('#deleteConfirmModal').modal("hide");
        });
    }

    deleteFeedClick() {
        var self = this;

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                self.deleteFeed();
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#deleteConfirmModal').modal(options).modal('show');
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
        var self = this;

        $(".feedMenu .item").tab({
            onFirstLoad: function(tabPath, parameterArray, historyEvent) {
                if (tabPath === "first") {
                    return;
                }

                if (tabPath === "fourth") {
                    self.isLoadingHistory = true;
                    self.http.get("/api/feeds/" + self.feed.Id + "/history").then(message => {
                        self.historyRecords = JSON.parse(message.response).Results;
                        self.isLoadingHistory = false;
                    });
                }
            }
        });
    }
}