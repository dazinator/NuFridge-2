define(['plugins/router', 'api', 'auth', 'databinding-package', 'xml2json', 'databinding-feed'], function (router, api, auth, databindingPackage, xml2Json, databindingFeed) {
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
    };

    ctor.prototype.activate = function(activationData) {
        var self = this;

        activationData.loaded.then(function() {

            self.feed(activationData.feed());

            if (!self.feed()) {
                throw "A feed must be provided when using the edit feed package list.";
            }

            self.loadPackages();
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

    ctor.prototype.getFilterParam = function() {
        return "$filter=IsLatestVersion";
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
    }

    ctor.prototype.fileUploadAction = function () {
        var self = this;

        self.resetProgressForFileUpload();

  
        var options = {
            closable: false,
            onApprove: function (sender) {
                if (!self.isUploadingPackage()) {
                    self.startFileUpload();
                }
                return false;
            },
            transition: 'horizontal flip'
        };

        $('#_attachmentName').val("");

        $('#fileUploadModal').modal(options).modal('show');
    };

    ctor.prototype.feedUploadAction = function () {

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



    ctor.prototype.compositionComplete = function () {
        var self = this;


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

        var fileExtentionRange = '.nupkg';

        $(document).on('change', '.btn-file :file', function () {
            var input = $(this);

            if (navigator.appVersion.indexOf("MSIE") != -1) {
                var label = input.val();

                input.trigger('fileselect', [1, label, 0]);
            } else {
                var label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
                var numFiles = input.get(0).files ? input.get(0).files.length : 1;
                var size = input.get(0).files[0].size;

                input.trigger('fileselect', [numFiles, label, size]);
            }
        });

        $('.btn-file :file').on('fileselect', function (event, numFiles, label, size) {
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

                $('#attachmentName').removeAttr('name');
            }
        });
    };



    return ctor;
});