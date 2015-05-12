define(['plugins/router', 'databinding-lucenefeed'], function (router, luceneFeed) {
    var ctor = function () {
        this.displayName = 'Feeds';
        this.feeds = ko.observableArray();
        this.pageCount = ko.observable(1);
        this.currentPage = ko.observable(0);
        this.feedsLoaded = ko.observable(false);
    };

    ctor.prototype.previousPage = function() {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() - 1);
    };

    ctor.prototype.loadFeeds = function(pageNumber) {
        var self = this;

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.currentPage()) {
                return;
            }
        }


        $.ajax({
            url: "/api/Feeds?page=" + pageNumber + "&pageSize=10",
            cache: false,
            dataType: 'json'
        }).then(function(response) {
            self.feedsLoaded(true);
            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function(options) {
                    return luceneFeed(options.data);
                }
            };

            ko.mapping.fromJS(response.results, mapping, self.feeds);


        }).fail(function(response) {
            self.feedsLoaded(true);

            alert("Errors are not handled yet.");
        });
    };

    ctor.prototype.goToPage = function(pageNumber) {
        var self = this;

        self.loadFeeds(pageNumber);
    };

    ctor.prototype.nextPage = function(data, event) {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() + 1);
    };

    ctor.prototype.activate = function() {
        var self = this;

        self.loadFeeds();
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $('#viewFeedTabs').tabs();
    };

    return ctor;
});