define(['plugins/router', 'databinding-lucenefeed', 'databinding-lucenepackage', 'readmore'], function (router, luceneFeed, lucenePackage) {
    var ctor = function () {
        var self = this;

        self.feed = ko.observable(luceneFeed());
        self.packages = ko.observableArray();
        self.pageCount = ko.observable(1);
        self.currentPage = ko.observable(0);
        self.displayName = ko.observable('');
        self.pageSize = ko.observable(5);
        self.searchTerm = ko.observable('');
        self.searchTimeout = null;
        self.isSearching = ko.observable(false);
        self.searchSubscription = null;
        self.searchError = ko.observable();
        self.searchTerm.subscribe(function () {
            clearTimeout(self.searchTimeout);
            self.searchTimeout = setTimeout(function () {
                self.loadPackages();
            }, 500);
        });
    };

    ctor.prototype.activate = function() {

        var self = this;

        if (router.activeInstruction().params.length == 1) {
            $.ajax({
                url: "/api/Feeds/" + router.activeInstruction().params[0],
                cache: false,
                dataType: 'json'
            }).then(function(response) {

                var mapping = {
                    create: function(options) {
                        return luceneFeed(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feed);
                self.displayName('Edit ' + self.feed().name());
                self.loadPackages();
            }).fail(function(xmlHttpRequest, textStatus, errorThrown) {
                router.navigate("#");
                Materialize.toast(errorThrown, 7500);
            });
        } else {
            alert("This scenario is not handled.");
        }
    };

    ctor.prototype.loadPackages = function(pageNumber) {
        var self = this;

        clearTimeout(self.searchTimeout);

        if (self.searchSubscription) {
            self.searchSubscription.dispose();
            self.searchSubscription = null;
        }

        if (self.isSearching()) {

            self.searchSubscription = self.isSearching.subscribe(function(hasLoaded) {
                if (hasLoaded === true) {
                    self.loadPackages(pageNumber);
                }
            });

            return;
        }

        self.isSearching(true);

        if (!pageNumber) {
            pageNumber = 0;
        }

        var url = "/api/packages/" + self.feed().id() + "/" + pageNumber + "/" + self.pageSize();

        if (self.searchTerm() !== '') {
            url += "/" + self.searchTerm();
        }

        $.ajax({
            url: url,
            cache: false,
            dataType: 'json'
        }).then(function(response) {

            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function(options) {
                    return lucenePackage(options.data);
                }
            };
            self.searchError(null);
            ko.mapping.fromJS(response.results, mapping, self.packages);
            self.isSearching(false);
        }).fail(function(xmlHttpRequest, textStatus, errorThrown) {
            var parsedError = JSON.parse(xmlHttpRequest.responseText);
            self.isSearching(false);
            self.searchError(parsedError);

            $('.packagesExceptionStackTrace').readmore({
                speed: 75,
                lessLink: '<a href="#">Collapse</a>',
                moreLink: '<a href="#">Expand</a>',
                collapsedHeight: 65,
                embedCSS: true
            });
        });
    };

    ctor.prototype.deleteClick = function() {
        var feed = this;

        $("#deleteFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'DELETE',
            dataType: 'json',
            cache: false,
            success: function(result) {
                $("#deleteFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully deleted.', 7500);
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                $("#deleteFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    };

    ctor.prototype.updateClick = function() {
        var feed = this;

        $("#editFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'PUT',
            data: ko.toJS(feed),
            dataType: 'json',
            cache: false,
            success: function(result) {
                $("#editFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully updated.', 7500);
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                $("#editFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    };

    ctor.prototype.changePageSize = function(data, event) {
        var self = this;

        var target;

        if (event.target) {
            target = event.target;
        } else if (event.srcElement) {
            target = event.srcElement;
        }

        if (target.nodeType == 3) {
            target = target.parentNode;
        }

        var newPageSize = parseInt($(target).text(), 10);

        self.pageSize(newPageSize);

        $(".viewFeedsPageSize").text(newPageSize + ' Packages Per Page');

        self.loadPackages(0);
    };

    ctor.prototype.compositionComplete = function() {

        $('#viewFeedTabs').tabs();
        $('.viewFeedsPageSize').dropdown({
                inDuration: 300,
                outDuration: 225,
                constrain_width: true, // Does not change width of dropdown to that of the activator
                hover: false, // Activate on hover
                gutter: 0, // Spacing from edge
                belowOrigin: true // Displays dropdown below the button
            }
        );

        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

    };

    ctor.prototype.goToPage = function(pageNumber) {
        var self = this;

        self.loadPackages(pageNumber);
    };

    ctor.prototype.nextPage = function(data, event) {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() + 1);
    };

    ctor.prototype.previousPage = function() {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() - 1);
    };

    return ctor;
});