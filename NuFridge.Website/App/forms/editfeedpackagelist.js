﻿define(['plugins/router', 'api', 'auth', 'databinding-package', 'xml2json', 'databinding-feed', 'readmore', 'databinding-feedimportstatus', 'databinding-feedimportoptions', '/signalr/hubs'], function (router, api, auth, databindingPackage, xml2Json, databindingFeed, readmore, databindingfeedimportstatus, feedimportoptions) {
    var ctor = function () {
        var self = this;
        self.packages = ko.observableArray();
        self.totalCount = ko.observable(0);
        self.feed = ko.validatedObservable(databindingFeed());
        self.pageCount = ko.observable(1);
        self.currentPage = ko.observable(0);
        self.pageSize = ko.observable(10);
        self.searchTerm = ko.observable("");
        self.activeSearchTerms = ko.observableArray();
        self.isSearchingForPackages = ko.observable(false);
        self.isUploadingPackage = ko.observable(false);
        self.packageUploadPercentage = 0;
        self.successUploadingPackage = ko.observable(false);
        self.errorUploadingPackage = ko.observable(false);
        self.urlUploadValue = ko.observable("");
        self.showPrereleasePackages = ko.observable(false);
        self.feedimportstatus = ko.observable(databindingfeedimportstatus());
        self.feedimportoptions = ko.observable(feedimportoptions());
        self.showSuccessfulFeedImports = ko.observable(false);
        self.showFailedFeedImports = ko.observable(false);
        self.feedImportMode = ko.observable(null);

        self.showPrereleasePackages.subscribe(function(newValue) {
            self.loadPackages(0);
        });
    };

    ctor.prototype.toggleFailedFeedImports = function(item, event) {
        var self = this;
        self.showFailedFeedImports(!self.showFailedFeedImports());
        if (self.showFailedFeedImports()) {
            $(event.target).text('Hide failed packages');
        } else {
            $(event.target).text('Show failed packages');
        }
    };

    ctor.prototype.toggleSuccessfulFeedImports = function (item, event) {
        var self = this;
        self.showSuccessfulFeedImports(!self.showSuccessfulFeedImports());
        if (self.showSuccessfulFeedImports()) {
            $(event.target).text('Hide imported packages');
        } else {
            $(event.target).text('Show imported packages');
        }
    };

    ctor.prototype.getPreviousPageArray = function () {
        var self = this;


        var size = self.currentPage();
        if (size > 10) {
            size = 10;
        }
        else if (size < 0) {
            size = 0;
        }

        var arr = new Array(size);
        var itemCurrent = self.currentPage();
        var arrayLength = arr.length;
        if (arrayLength > 0) {
            for (var i = arrayLength; i > 0; --i)
                arr[i - 1] = (itemCurrent--);
        }
        return arr;
    };

    ctor.prototype.getNextPageArray = function () {
        var self = this;

        var size = self.pageCount() - (self.currentPage() + 1);

        if (size > 10) {
            size = 10;
        }
        else if (size < 0) {
            size = 0;
        }

        var arr = new Array(size);
        var itemCurrent = self.currentPage() + 2;
        var arrayLength = arr.length;
        for (var i = 0; i < arrayLength; i++) {
            arr[i] = (itemCurrent++);
        }
        return arr;
    };

    ctor.prototype.activate = function(activationData) {
        var self = this;

        activationData.loaded.then(function() {

            self.feed(activationData.feed());

            if (!self.feed()) {
                throw "A feed must be provided when using the edit feed package list.";
            }
        });
    };

    ctor.prototype.performSearch = function() {
        var self = this;
        self.loadPackages(0);
    };

    ctor.prototype.arrayContains = function (arr, item) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] === item) return true;
        }
        return false;
    };

    ctor.prototype.getUniqueItemsInArray = function (extArr) {
        var self = this;

        var arr = [];
        for (var i = 0; i < extArr.length; i++) {
            var item = extArr[i].toLowerCase();
            if (!self.arrayContains(arr, item)) {
                if (item) {
                    arr.push(item);
                }
            }
        }
        return arr;
    }

    ctor.prototype.loadPackages = function(pageNumber) {
        var self = this;

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.currentPage()) {
                return;
            }
        }

        var url = "Feeds/" + self.feed().Name() + "/api/v2/Search()?" + self.getInlineCountParam() + "&" + self.getPagingParams(pageNumber) + "&" + self.getFilterParam();

        if (self.searchTerm()) {
            self.activeSearchTerms(self.getUniqueItemsInArray(self.searchTerm().split(" ")));
            var searchParam = self.getSearchTermParam();
            url += "&" + searchParam;
        } else {
            self.activeSearchTerms.removeAll();
        }


        self.isSearchingForPackages(true);

        $.ajax({
            url: url,
            cache: false,
            //headers: new auth().getAuthHttpHeader()
        }).then(function (response) {

   

            var mapping = {
                create: function (options) {
                    return databindingPackage(options.data);
                }
            };

            var json = $.xml2json(response);

            var entry = json.entry;

            if (entry && !Array.isArray(entry)) {
                entry = [];
                entry.push(json.entry);
            }
            else if (!entry) {
                entry = [];
            }

            var totalPages = Math.ceil(json.count / self.pageSize());

            self.pageCount(totalPages);
            self.currentPage(pageNumber);

            self.totalCount(json.count);

            ko.mapping.fromJS(entry, mapping, self.packages);

            self.isSearchingForPackages(false);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            self.isSearchingForPackages(false);

            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
       
            //Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
        });
    };

    ctor.prototype.deleteSearchTerm = function(searchTermToDelete) {
        var self = this;

        var splitTerms = self.activeSearchTerms();
        var index = splitTerms.indexOf(searchTermToDelete);

        if (index > -1) {
            splitTerms.splice(index, 1);
        }

        self.searchTerm(splitTerms.join(" "));
        self.performSearch();
    };

    ctor.prototype.getFilterParam = function () {
        var self = this;
        if (self.showPrereleasePackages()) {
            return "$filter=IsAbsoluteLatestVersion";
        } else {
            return "$filter=IsLatestVersion";
        }
    };

    ctor.prototype.getSearchTermParam = function() {
        var self = this;
        return "searchTerm=" + self.activeSearchTerms().join(" ");
    };

    ctor.prototype.goToPage = function (pageNumber) {
        var self = this;

        self.loadPackages(pageNumber);
    };

    ctor.prototype.nextPage = function (data, event) {
        var self = this;

        if ($(event.target).closest("a").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() + 1);
    };

    ctor.prototype.previousPage = function (sender, event) {
        var self = this;

        if ($(event.target).closest("a").hasClass("disabled")) {
            return;
        }

        self.loadPackages(self.currentPage() - 1);
    };

    ctor.prototype.getPagingParams = function (pageNumber) {
        var self = this;

        var skip = self.pageSize() * pageNumber;
        var take = self.pageSize();
        return "$skip=" + skip + "&$top=" + take;
    };

    ctor.prototype.getInlineCountParam = function() {
        return "$inlinecount=allpages";
    };

    ctor.prototype.resetProgressForFileUpload = function() {
        var self = this;

        $(".fileUploadProgress").find("div.label").text("Your NuGet package is being uploaded.");
        self.packageUploadPercentage = 0;
        $('.fileUploadProgress').progress('reset').removeClass('success').removeClass('error').addClass('indicating');
        self.successUploadingPackage(false);
        self.errorUploadingPackage(false);
    };

    ctor.prototype.startUrlUpload = function() {
        var self = this;

        $(".urlUploadMessage").text("Please wait while the package is pushed to the feed.");

        self.isUploadingPackage(true);

        $.ajax({
            url: '/api/feeds/' + self.feed().Id() + "/upload?url=" + self.urlUploadValue(),
            type: 'POST',
            headers: new auth().getAuthHttpHeader(),
            success: function (data) {
                self.successUploadingPackage(true);
                self.isUploadingPackage(false);
                self.loadPackages(0);
                $(".urlUploadMessage").text("The package has been pushed to the feed.");
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                self.errorUploadingPackage(true);
                self.isUploadingPackage(false);

                var message = "The package was not pushed to the server.";

                if (xmlHttpRequest.responseText) {
                    message = xmlHttpRequest.responseText;
                }

                $(".urlUploadMessage").text(message);

                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
            },
            cache: false,
            contentType: false,
            processData: false
        }, 'json');
    };

    ctor.prototype.startFeedUpload = function () {
        var self = this;



        $.connection.hub.url = "/signalr";
        var hub = $.connection.importPackagesHub;

        hub.client.importPackagesUpdate = function (response) {
            var mapping = {
                create: function (options) {
                    return databindingfeedimportstatus(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.feedimportstatus);

            if (self.feedimportstatus().IsCompleted() === true) {
                self.successUploadingPackage(true);
                self.isUploadingPackage(false);
                self.loadPackages(0);
            }
        };

        $.connection.hub.start().done(function () {
            hub.server.subscribe(self.feed().Id());

            $.ajax({
                url: "/api/feeds/" + self.feed().Id() + "/import",
                type: 'POST',
                data: ko.toJS(self.feedimportoptions()),
                headers: new auth().getAuthHttpHeader(),
                cache: false,
                success: function (result) {
                    self.isUploadingPackage(true);
                },
                error: function (xmlHttpRequest, textStatus, errorThrown) {
                    if (xmlHttpRequest.status === 401) {
                        router.navigate("#signin");
                    }
                }
            });
        });
    };

    ctor.prototype.feedUploadAction = function () {
        var self = this;

        self.feedimportoptions(feedimportoptions());
        self.feedimportstatus(databindingfeedimportstatus());
        self.feedImportMode(null);
        $('.ui.checkbox.specificNuGetPackageModeCheckBox').checkbox('uncheck');
        $('.ui.checkbox.searchTermNuGetPackageModeCheckBox').checkbox('uncheck');
        $('.ui.checkbox.allNuGetPackageModeCheckBox').checkbox('uncheck');

        if (self.feedimportoptions().IncludePrerelease() === true) {
            $('.ui.checkbox.importFeedIncludePrereleaseCheckBox').checkbox('check');
        }

        $('.ui.dropdown.importFeedVersionSelectorDropDown').dropdown('set selected', self.feedimportoptions().VersionSelector());

        self.successUploadingPackage(false);
        self.errorUploadingPackage(false);

        var options = {
            closable: false,
            onApprove: function (sender) {
                if (!self.isUploadingPackage()) {
                    self.startFeedUpload();
                }
                return false;
            },
            transition: 'horizontal flip',
            detachable: false,
            observeChanges: true
        };

        $('#feedUploadModal').modal(options).modal('show');
    };

    ctor.prototype.urlUploadAction = function () {
        var self = this;

        self.successUploadingPackage(false);
        self.errorUploadingPackage(false);

        self.urlUploadValue("");
        $(".urlUploadMessage").text("");

        var options = {
            closable: false,
            onApprove: function (sender) {
                if (!self.isUploadingPackage()) {
                    self.startUrlUpload();
                }
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#urlUploadModal').modal(options).modal('show');
    };

    ctor.prototype.fileUploadAction = function () {
        var self = this;

        self.resetProgressForFileUpload();

        var fileExtentionRange = '.nupkg';

        $(document).off('change.packagefileupload', '.btn-file :file').on('change.packagefileupload', '.btn-file :file', function () {
            var input = $(this);

            if (navigator.appVersion.indexOf("MSIE") != -1) {
                var label = input.val();

                input.trigger('fileselect.packagefileupload', [1, label, 0]);
            } else {
                var label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
                var numFiles = input.get(0).files ? input.get(0).files.length : 1;
                var size = input.get(0).files[0].size;

                input.trigger('fileselect.packagefileupload', [numFiles, label, size]);
            }
        });

        $('.btn-file :file').off('fileselect.packagefileupload').on('fileselect.packagefileupload', function (event, numFiles, label, size) {
            $('#attachmentName').attr('name', 'attachmentName');
            var isErrorVisible;
            var postfix = label.substr(label.lastIndexOf('.'));
            if (fileExtentionRange.indexOf(postfix.toLowerCase()) > -1) {
                $('#_attachmentName').val(label);

                isErrorVisible = $('.invalidFileExtensionMessage').transition("is visible");
                if (isErrorVisible) {
                    $('.invalidFileExtensionMessage').transition('scale');
                }
            } else {
                isErrorVisible = $('.invalidFileExtensionMessage').transition("is visible");
                if (!isErrorVisible) {
                    $('.invalidFileExtensionMessage').transition('scale');
                }
            }
        });

  
        var options = {
            closable: false,
            onApprove: function (sender) {
                if (!self.isUploadingPackage()) {
                    self.startFileUpload();
                }
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#_attachmentName').val("");

        $('#fileUploadModal').modal(options).modal('show');
    };



    ctor.prototype.progressHandlingFunction = function (e, instance) {
        var self = instance;

        if (e.lengthComputable) {
            var percentComplete = (e.loaded / e.total) * 100;
            var diff = percentComplete - self.packageUploadPercentage;

            if (diff >= 1 || percentComplete === 100) {
                $(".fileUploadProgress").progress('increment', diff);
                self.packageUploadPercentage = percentComplete;
            }

            if (percentComplete >= 99) {
                self.packageUploadPercentage = 100;
                if (percentComplete === 99) {
                    $(".fileUploadProgress").progress('increment', 1);
                }
                $(".fileUploadProgress").find("div.label").text("Please wait while the server processes the package.");
            }
       

        }
    }

    ctor.prototype.startFileUpload = function () {
        var self = this;

        self.resetProgressForFileUpload();

        self.isUploadingPackage(true);
        var formData = new FormData($('#fileUpload')[0]);
        $.ajax({
            url: 'Feeds/' + self.feed().Name() + "/api/v2/package",
            type: 'POST',
            headers: new auth().getAuthHttpHeader(),
            xhr: function () {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    var func = function(e) {
                        self.progressHandlingFunction(e, self);
                    }
                    myXhr.upload.addEventListener('progress', func, false);
                }
                return myXhr;
            },
            success: function (data) {
                self.successUploadingPackage(true);
                self.isUploadingPackage(false);
                self.loadPackages(0);
                $(".fileUploadProgress").find("div.label").text("The package has been pushed to the feed.");
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                self.errorUploadingPackage(true);
                self.isUploadingPackage(false);
                $('.fileUploadProgress').removeClass('success').removeClass('indicating').addClass('error');

                var message = "The package was not pushed to the server.";

                if (xmlHttpRequest.responseText) {
                    message = xmlHttpRequest.responseText;
                }

                $(".fileUploadProgress").find("div.label").text(message);

                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
            },
            data: formData,
            cache: false,
            contentType: false,
            processData: false
        }, 'json');

    };

    ctor.prototype.downloadLatestPackageVersion = function(pkg) {
        var self = this;

        window.location = pkg.GetDownloadLink(self.feed().Name());
    };

    ctor.prototype.compositionComplete = function () {
        var self = this;

        self.loadPackages();

        $('.ui.checkbox.importFeedIncludePrereleaseCheckBox').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedimportoptions().IncludePrerelease(true);
            },
            onUnchecked: function () {
                self.feedimportoptions().IncludePrerelease(false);
            }
        });

        $('.ui.checkbox.specificNuGetPackageModeCheckBox').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedImportMode(1);
            }
        });

        $('.ui.checkbox.searchTermNuGetPackageModeCheckBox').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedImportMode(2);
            }
        });

        $('.ui.checkbox.allNuGetPackageModeCheckBox').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedImportMode(3);
            }
        });

        $('.ui.dropdown.importFeedVersionSelectorDropDown').dropdown({
            onChange: function(value) {
                self.feedimportoptions().VersionSelector(value);
            }
        });
        

        $(".fileUploadProgress").progress({
            total: 100,
            value: 0
        });

        $(".packageSearch .prompt").on("keypress", function (e) {
            if (e.keyCode === 13) {
                self.performSearch();
                return false;
            }
        });

        $('.ui.dropdown.uploadPackage').dropdown({
            action: function () {
                var actionName = $(this).text().trim();

                switch (actionName) {
                    case "File":
                        self.fileUploadAction();
                        break;
                    case "URL":
                        self.urlUploadAction();
                        break;
                    case "NuGet Feed":
                        self.feedUploadAction();
                        break;
                    default:
                        alert("Not handled");
                        break;
                };
            }
        });

        $('.uploadPackage.button.titlePopup')
            .popup({
                hoverable: true,
                title: 'Add Package'
            });

    };



    return ctor;
});