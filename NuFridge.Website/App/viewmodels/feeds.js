define(['plugins/router', 'databinding-feed', 'api', 'auth'], function (router, databindingFeed, api, auth) {
    var ctor = function () {
        this.displayName = 'Feeds';
        this.feeds = ko.observableArray();
        this.pageCount = ko.observable(1);
        this.currentPage = ko.observable(0);
        this.feedsLoaded = ko.observable(false);
        this.isAddFeedNavigating = ko.observable(false);
    };

    ctor.prototype.previousPage = function (sender, event) {
        var self = this;

        if ($(event.target).closest("a").hasClass("disabled")) {
            return;
        }

        self.loadFeeds(self.currentPage() - 1);
    };

    ctor.prototype.addFeed = function () {
        var self = this;

        self.isAddFeedNavigating(true);

        router.navigate("#feeds/create");
    };

    ctor.prototype.feedClick = function(feed) {

        var url = feed.viewFeedUrl();

        router.navigate(url);
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
            url: api.get_feeds + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function(response) {
            self.feedsLoaded(true);
            self.pageCount(response.TotalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function(options) {
                    return databindingFeed(options.data);
                }
            };

            ko.mapping.fromJS(response.Results, mapping, self.feeds);


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            self.feedsLoaded(true);

            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.goToPage = function(pageNumber) {
        var self = this;

        self.loadFeeds(pageNumber);
    };

    ctor.prototype.nextPage = function(data, event) {
        var self = this;

        if ($(event.target).closest("a").hasClass("disabled")) {
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

        var searchOptions = {
            apiSettings: {
                url: api.search_feeds + '/?name={query}',
                beforeXHR: function (xhr) {
                    xhr.setRequestHeader('Authorization', 'Token ' + new auth().getAuthToken());
                }
            },
            type: 'category'
        };

        $('.ui.search.feedSearch').search(searchOptions);
    };

    return ctor;
});