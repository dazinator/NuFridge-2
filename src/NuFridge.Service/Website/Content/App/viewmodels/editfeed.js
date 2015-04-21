define(['plugins/router', 'databinding/LuceneFeed', 'databinding/lucenepackage'], function (router, luceneFeed, lucenePackage) {
    var ctor = function () {
        var self = this;

        this.feed = ko.observable(luceneFeed());
        this.packages = ko.observableArray();
        this.pageCount = ko.observable(1);
        this.currentPage = ko.observable(0);
        this.displayName = ko.observable('');
        this.pageSize = ko.observable(5);
    };

    ctor.prototype.activate = function () {

        var self = this;

        if (router.activeInstruction().params.length == 1) {
            $.ajax({
                url: "/api/Feeds/" + router.activeInstruction().params[0],
                cache: false,
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return luceneFeed(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feed);
                self.displayName('Edit ' + self.feed().name());
                self.loadPackages();
            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                router.navigate("#");
                Materialize.toast(errorThrown, 7500);
            });
        } else {
            alert("This scenario is not handled.");
        }
    }

    ctor.prototype.loadPackages = function (pageNumber) {
        var self = this;

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber == self.currentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/FeedPackages?id=" + self.feed().id() + "&page=" + pageNumber + "&pageSize=" + self.pageSize(),
            cache: false,
            dataType: 'json'
        }).then(function (response) {
            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function (options) {
                    return lucenePackage(options.data);
                }
            };

            ko.mapping.fromJS(response.results, mapping, self.packages);
        }).fail(function (response) {
            alert("Errors are not handled yet.");
        });
    }

    ctor.prototype.deleteClick = function () {
        var feed = this;

        $("#deleteFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'DELETE',
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#deleteFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully deleted.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#deleteFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    }

    ctor.prototype.updateClick = function () {
        var feed = this;

        $("#editFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'PUT',
            data: ko.toJS(feed),
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#editFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully updated.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#editFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    }

    ctor.prototype.changePageSize = function(data, event) {
        var self = this;

        var target;

        if (event.target) {
            target = event.target;
        }
        else if (event.srcElement) {
            target = event.srcElement;
        }

        if (target.nodeType == 3) {
            target = target.parentNode;
        }

        var newPageSize = parseInt($(target).text());

        self.pageSize(newPageSize);

        $(".viewFeedsPageSize").text(newPageSize + ' Packages Per Page');

        self.loadPackages(0);
    }

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

            $("#progressBar").attr("aria-busy", false);
        
    }

    ctor.prototype.goToPage = function (pageNumber) {
        var self = this;

        self.loadPackages(pageNumber);
    }

    ctor.prototype.nextPage = function (data, event) {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() + 1);
    }

    ctor.prototype.previousPage = function () {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() - 1);
    }

    return ctor;
});