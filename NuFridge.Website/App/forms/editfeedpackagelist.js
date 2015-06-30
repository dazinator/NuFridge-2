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
    };

    ctor.prototype.activate = function(activationData) {
        var self = this;

        activationData.loaded.then(function() {

            self.feed = activationData.feed;

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

    ctor.prototype.loadPackages = function(pageNumber) {
        var self = this;

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.currentPage()) {
                return;
            }
        }

        var url = "Feeds/" + self.feed().Name() + "/api/v2/Search()?" + self.getInlineCountParam() + "&" + self.getPagingParams(pageNumber);

        if (self.searchTerm()) {
        var searchParam = self.getSearchTermParam();
            url += "&" + searchParam;
        }

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

            if (!Array.isArray(entry)) {
                entry = [];
                entry.push(json.entry);
            }

            var totalPages = Math.ceil(json.count / self.pageSize());

            self.pageCount(totalPages);
            self.currentPage(pageNumber);

            self.totalCount(json.count);

            ko.mapping.fromJS(entry, mapping, self.packages);

        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            router.navigate("#");
            //Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
        });
    };

    ctor.prototype.getSearchTermParam = function() {
        var self = this;
        return "searchTerm=" + self.searchTerm();
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
                debugger;
                $('#fileUploadModal').modal('hide');
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                debugger;
                $('#fileUploadModal').modal('hide');
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