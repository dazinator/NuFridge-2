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

        self.packages.removeAll();

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

    ctor.prototype.fileUploadAction = function () {
        var self = this;

        $('#fileUploadModal').find(".ui.button").removeClass("disabled").removeClass("loading");

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                $(modal).find("label.ui.icon.button.btn-file").addClass("disabled");
                self.startFileUpload();
                return false;
            },
            transition: 'horizontal flip'
        };

        $('#attachmentName').removeAttr('name');
        $('#_attachmentName').val("");

        $('#fileUploadModal').modal(options).modal('show');
    };

    ctor.prototype.feedUploadAction = function () {

    };

    function progressHandlingFunction(e) {
        if (e.lengthComputable) {
            console.log("Loaded: " + e.loaded);
            console.log("Total: " + e.total);
        }
    }

    ctor.prototype.startFileUpload = function () {
        var self = this;

        var formData = new FormData($('#fileUpload')[0]);
        $.ajax({
            url: 'Feeds/' + self.feed().Name() + "/api/v2/package",
            type: 'POST',
            headers: new auth().getAuthHttpHeader(),
            xhr: function () {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    myXhr.upload.addEventListener('progress', progressHandlingFunction, false);
                }
                return myXhr;
            },
            success: function (data) {
                self.loadPackages(0);
                $('#fileUploadModal').modal('hide');
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $('#fileUploadModal').modal('hide');

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