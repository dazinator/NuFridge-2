define(['databinding/LuceneFeed'], function () {
    var ctor = function () {
        this.displayName = 'Feeds';
        this.feeds = ko.observableArray();
        this.pageCount = ko.observable(1);
        this.currentPage = ko.observable(0);
    };

    ctor.prototype.previousPage = function () {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() - 1);
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

        $.ajax({
            url: "/api/Feeds?page=" + pageNumber,
            cache: false,
            dataType: 'json'
        }).then(function (response) {
            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);
            ko.mapping.fromJS(response.results, LuceneFeed.mapping, self.feeds);
        }).fail(function (response) {
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

    return ctor;
});