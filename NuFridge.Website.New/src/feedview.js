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

    packagesSearchText = "";
    packagesSortOrder = 0;
    packagesPageSize = 10;
    packagesTotalPages = 1;
    packagesCurrentPage = 1;
    packagesShowPrerelease = false;
    packagesTotalMatchingQuery = 0;
    packagesRecords = new Array();

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

    reindexPackagesClick() {
        var self = this;

        this.http.post("/api/feeds/" + self.feed.Id + "/reindex");
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

    resetPackageListingClick() {
        var self = this;

        self.packagesPageSize = 10;
        $('.ui.dropdown.pageSizeDropdown').dropdown('set selected', 10);

        self.packagesSortOrder = 0;
        $('.ui.dropdown.pageOrderDropdown').dropdown('set selected', 0);

        self.packagesSearchText = "";

        self.loadFeedPackages();
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

    loadFeedPackages() {
        var self = this;

        var skip = self.packagesPageSize * (self.packagesCurrentPage - 1);
        var take = self.packagesPageSize;

        var filter = "$filter=IsLatestVersion";

        if (self.packagesShowPrerelease === true) {
            filter = "$filter=IsAbsoluteLatestVersion";
        }

        var order = "$orderby=";

        if (self.packagesSortOrder === 0) {
            order += "DownloadCount desc";
        } else  if (self.packagesSortOrder === 1) {
            order += "DownloadCount asc";
        } else  if (self.packagesSortOrder === 2) {
            order += "Id desc";
        }  else  if (self.packagesSortOrder === 3) {
            order += "Id asc";
        }

        var search = "";

        if (self.packagesSearchText) {
            search = "&searchTerm=" + self.packagesSearchText;
        }

        var url = "/Feeds/" + self.feed.Name + "/api/v2/Search()?$inlinecount=allpages" + "&" +
                  "$skip=" + skip + "&$top=" + take + "&" + filter + "&" + order + search;

        self.http.get(url).then(message => {
            var xmlDoc = $.parseXML(message.response), $xml = $(xmlDoc);
            $($xml).each(function() {
                self.packagesTotalMatchingQuery = $(this).find("feed>count").text();
                self.packagesTotalPages = Math.ceil(self.packagesTotalMatchingQuery / self.packagesPageSize);
                self.packagesRecords = $.map($(this).find("feed>entry"), function(value, index) {
                    var jvalue = $(value);
                    return {
                        Title: jvalue.find("title").text(),
                        Authors: $.map($(value).find("author>name"), function(authorValue) {
                            return $(authorValue).text();
                        }),
                        Description: jvalue.find("properties").find("Description").text()
                    }
                });
            });
        });
    }

    loadFeedHistory() {
        var self = this;

        self.isLoadingHistory = true;

        var startDate = new Date();

        self.http.get("/api/feeds/" + self.feed.Id + "/history").then(message => {
            var func = function() {
                self.isLoadingHistory = false;
                self.historyRecords = JSON.parse(message.response).Results;
            };

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 0.5) {
                setTimeout(function() { func(); }, (0.5 - secondsDifference) * 1000);
            } else {
                func();
            }
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

                if (tabPath === "third") {
                    self.loadFeedPackages();
                }
                else if (tabPath === "fourth") {
                    self.loadFeedHistory();
                }
            }
        });

        $('.ui.dropdown.pageSizeDropdown').dropdown({
            onChange: function(value) {
                self.packagesPageSize = value;
            }
        });

        $('.ui.dropdown.pageOrderDropdown').dropdown({
            onChange: function(value) {
                self.packagesSortOrder = value;
            }
        });
    }
}