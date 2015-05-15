define(['plugins/router', 'databinding-lucenefeed', 'databinding-lucenepackage', 'readmore'], function (router, luceneFeed, lucenePackage) {
    var ctor = function () {
        var self = this;

        self.feed = ko.validatedObservable(luceneFeed());
        self.packages = ko.observableArray();
        self.pageCount = ko.observable(1);
        self.currentPage = ko.observable(0);
        self.displayName = ko.observable('');
        self.pageSize = ko.observable(10);
        self.searchTerm = ko.observable('');
        self.searchTimeout = null;
        self.isSearching = ko.observable(false);
        self.searchSubscription = null;
        self.searchError = ko.observable();
        self.hiddenPackageRowClassName = 'tablePackageExpandCollapseRow';
        self.visiblePackageRowClassName = 'tablePackageRow';
        self.searchTerm.subscribe(function () {
            clearTimeout(self.searchTimeout);
            self.searchTimeout = setTimeout(function () {
                self.loadPackages();
            }, 500);
        });
        self.thisWillCreateUrl = ko.observable("");
        self.setThisWillCreateUrlText();

        self.feed().name.subscribeChanged(function (newValue, oldValue) {
            var virtualDirectory = self.feed().virtualDirectory();
            if (!virtualDirectory || virtualDirectory === "/" || virtualDirectory === "/" + oldValue.toLowerCase()) {
                self.feed().virtualDirectory("/" + newValue.toLowerCase());
            }
        });

        self.feed().port.subscribe(function () {
            self.setThisWillCreateUrlText();
        });

        self.feed().host.subscribe(function () {
            self.setThisWillCreateUrlText();
        });

        self.feed().virtualDirectory.subscribe(function () {
            self.setThisWillCreateUrlText();
        });
    };

    ctor.prototype.expandCollapsePackageRow = function(index) {
        var self = this;

        var rowClass = self.visiblePackageRowClassName + index;
        var rowToShowHideClass = self.hiddenPackageRowClassName + index;

        var row = $('.' + rowClass);
        var rowToShowHide = $('.' + rowToShowHideClass);

        row.css('border', "1px solid #DCDCDC");
        row.css('border-width', "1px 1px 0px 1px");
        rowToShowHide.css('border', "1px solid #DCDCDC");
        rowToShowHide.css('border-width', "0px 1px 1px 1px");

        var updateParentRow = function (rowToShowHide, row) {
            if (rowToShowHide.is(':visible')) {
                row.addClass('grey lighten-4');
            } else {
                row.removeClass('grey lighten-4');
                row.css('border', "none");
                rowToShowHide.css('border', "none");
            }
        };

        rowToShowHide.slideToggle('fast', 'swing', updateParentRow.bind(this, rowToShowHide, row));
    };

    ctor.prototype.setThisWillCreateUrlText = function () {
        var self = this;

        var host = self.feed().host();
        var port = self.feed().port();
        var virtualDirectory = self.feed().virtualDirectory();

        if (port === "80") {
            port = "";
        } else {
            port = ":" + port;
        }

        self.thisWillCreateUrl("http://" + host + port + virtualDirectory);
    };

    ctor.prototype.cancelClick = function() {
        router.navigate('#feeds');
    };

    ctor.prototype.activate = function () {

        var self = this;

        if (router.activeInstruction().params.length === 1) {
            $.ajax({
                url: "/api/feeds/" + router.activeInstruction().params[0],
                cache: false,
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return luceneFeed(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feed);
                self.setThisWillCreateUrlText();
                self.displayName(self.feed().name());
                self.loadPackages();
            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                router.navigate("#");
                Materialize.toast(errorThrown, 7500);
            });
        } else {
            alert("This scenario is not handled.");
        }
    };

    ctor.prototype.uploadConfirmClick = function() {
        var self = this;

        $("#uploadPackageModal").closeModal({
            complete: function () {
                setTimeout(function () {
                    $("#uploadPackageWaitModal").openModal({
                        dismissible: false,
                        opacity: .6
                    });

                    var formElement = document.getElementById("uploadForm");

                    var formData = new FormData(formElement);

                    $.ajax({
                        url: '/api/packages/' + self.feed().id(),
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false
                    }).then(function (response) {
                        self.loadPackages(self.currentPage());
                        Materialize.toast("Successfully uploaded the '" + $(".file-path").val() + "' package.", 7500);
                        $("#uploadPackageWaitModal").closeModal();
                    }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                        Materialize.toast(errorThrown, 7500);
                       $("#uploadPackageWaitModal").closeModal();
                    });


                }, 1);
            }
        });
    };

    ctor.prototype.loadPackages = function (pageNumber) {
        var self = this;

        clearTimeout(self.searchTimeout);

        if (self.searchSubscription) {
            self.searchSubscription.dispose();
            self.searchSubscription = null;
        }

        if (self.isSearching()) {

            self.searchSubscription = self.isSearching.subscribe(function (hasLoaded) {
                if (hasLoaded === true) {
                    self.loadPackages(pageNumber);
                }
            });

            return;
        }

        self.isSearching(true);

        self.packages.removeAll();

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
        }).then(function (response) {

            self.pageCount(response.totalPages);
            self.currentPage(pageNumber);

            var mapping = {
                create: function (options) {
                    return lucenePackage(options.data);
                }
            };
            self.searchError(null);
            ko.mapping.fromJS(response.results, mapping, self.packages);
            self.isSearching(false);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.responseText) {
                var parsedError = JSON.parse(xmlHttpRequest.responseText);
                self.searchError(parsedError);
            } else {
                self.searchError({
                    message: "There was a problem querying the NuGet feed.",
                    stackTrace: null,
                    exceptionMessage: null
                });
            }
         
            self.isSearching(false);
        

            $('.packagesExceptionStackTrace').readmore({
                speed: 75,
                lessLink: '<a href="#">Collapse</a>',
                moreLink: '<a href="#">Expand</a>',
                collapsedHeight: 65,
                embedCSS: true
            });
        });
    };

    ctor.prototype.uploadClick = function () {
        var self = this;

        $("#uploadPackageModal").openModal({
            dismissible: false,
            opacity: .6
        });

        $("#fileToUpload").val("");
        $(".file-path").val("");

        $("#fileToUpload").on('change', function() {
            $(".file-path").val($(this).val().split('\\').pop());
        });

        $("#confirmUploadButton").on('click', function (event) {
            if ($(".file-path").val()) {
                if ($(".file-path").val().match(/.nupkg/)) {
                    self.uploadConfirmClick();
                } else {
                    Materialize.toast('Only NuGet packages can be uploaded (*.nupkg).', 7500);
                }
            }
        });
    };

    ctor.prototype.deleteClick = function () {
        $("#deleteFeedConfirmationModal").openModal({
            dismissible: false,
            opacity: .6
        });
    };

    ctor.prototype.confirmDeleteClick = function () {
        var self = this;


        $("#deleteFeedConfirmationModal").closeModal({
            complete: function () {
                setTimeout(function () {
                    $("#deleteFeedModal").openModal({
                        dismissible: false,
                        opacity: .6
                    });

                    $.ajax({
                        url: "/api/Feeds/" + self.feed().id(),
                        type: 'DELETE',
                        dataType: 'json',
                        cache: false,
                        success: function(result) {
                            $("#deleteFeedModal").closeModal();
                            router.navigate('#feeds');
                            Materialize.toast('The ' + self.feed().name() + ' feed was successfully deleted.', 7500);
                        },
                        error: function(xmlHttpRequest, textStatus, errorThrown) {
                            $("#deleteFeedModal").closeModal();
                            Materialize.toast(errorThrown, 7500);
                        }
                    });

                }, 1);
            }
        });




    };

    ctor.prototype.updateClick = function () {
        var self = this;

        if (!self.feed.isValid()) {
            return;
        }

        $("#editFeedModal").openModal({
            dismissible: false,
            opacity: .6
        });

        $.ajax({
            url: "/api/Feeds/" + self.feed().id(),
            type: 'PUT',
            data: ko.toJS(self.feed()),
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#editFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + self.feed().name() + ' feed was successfully updated.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#editFeedModal").closeModal();
                Materialize.toast(JSON.parse(xmlHttpRequest.responseText).message, 7500);
            }
        });
    };

    ctor.prototype.changePageSize = function (data, event) {
        var self = this;

        var target;

        if (event.target) {
            target = event.target;
        } else if (event.srcElement) {
            target = event.srcElement;
        }

        if (target.nodeType === 3) {
            target = target.parentNode;
        }

        var newPageSize = parseInt($(target).text(), 10);

        self.pageSize(newPageSize);

        $(".viewFeedsPageSize").text(newPageSize + ' Packages Per Page');

        self.loadPackages(0);
    };

    ctor.prototype.compositionComplete = function () {

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

    ctor.prototype.goToPage = function (pageNumber) {
        var self = this;

        self.loadPackages(pageNumber);
    };

    ctor.prototype.nextPage = function (data, event) {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() + 1);
    };

    ctor.prototype.previousPage = function () {
        var self = this;

        if ($(event.target).closest("li").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() - 1);
    };

    return ctor;
});