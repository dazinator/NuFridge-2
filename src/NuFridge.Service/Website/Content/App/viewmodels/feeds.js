define(['databinding/LuceneFeed'], function (luceneFeed) {
    var ctor = function () {
        this.displayName = 'Feeds';
        this.feeds = ko.observableArray();
        this.pageCount = ko.observable(1);
        this.currentPage = ko.observable(0);
        this.feedsLoaded = ko.observable(false);
    };

    ctor.prototype.previousPage = function () {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() - 1);
    }

    ctor.prototype.compositionComplete = function () {
        var self = this;
        if (!self.feedsLoaded()) {
            $("#progressBar").attr("aria-busy", true);
        }
    }

    ctor.prototype.loadFeeds = function(pageNumber) {
        var self = this;

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber == self.currentPage()) {
                return;
            }
        }

        $("#progressBar").attr("aria-busy", true);

        $.ajax({
            url: "/api/Feeds?page=" + pageNumber,
            cache: false,
            dataType: 'json'
        }).then(function (response) {
            self.feedsLoaded(true);
            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function (options) {
                    return luceneFeed(options.data);
                }
            };

            ko.mapping.fromJS(response.results, mapping, self.feeds);

            $("#progressBar").attr("aria-busy", false);
        }).fail(function (response) {
            self.feedsLoaded(true);
            $("#progressBar").attr("aria-busy", false);
            alert("Errors are not handled yet.");
        });
    }

    ctor.prototype.goToPage = function(pageNumber) {
        var self = this;

        self.loadFeeds(pageNumber);
    }

    ctor.prototype.nextPage = function(data, event) {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() + 1);
    }

    ctor.prototype.activate = function () {
        var self = this;

        self.loadFeeds();
    }

    ctor.prototype.compositionComplete = function () {
        $('#feedsTabs').tabs();
    }

    return ctor;
});