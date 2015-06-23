define(['plugins/router', 'databinding-feed', 'databinding-package', 'api', 'auth'], function (router, databindingFeed, databindingPackage, api, auth) {
    var ctor = function () {
        var self = this;

        self.feed = ko.validatedObservable(databindingFeed());
        self.showSuccessMessageOnLoad = ko.observable(false);
        self.packages = ko.observableArray();
    };

    ctor.prototype.activate = function () {

        var self = this;

        if (router.activeInstruction().params.length >= 1) {
            $.ajax({
                url: api.get_feed + "/" + router.activeInstruction().params[0],
                cache: false,
                headers: new auth().getAuthHttpHeader(),
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return databindingFeed(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feed);

            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                router.navigate("#");
                //Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
            });
        } else {
            alert("This scenario is not handled.");
        }

        if (router.activeInstruction().params.length >= 2) {
            var param = router.activeInstruction().params[1];
            if (param && param.ss && param.ss === "1") {
                self.showSuccessMessageOnLoad(true);
            }
        }

        self.feedOptions = {
            mode: "Update",
            feed: self.feed,
            showSuccessMessageOnLoad: self.showSuccessMessageOnLoad()
        };
    };

    function progressHandlingFunction(e) {
        if (e.lengthComputable) {
            console.log("Loaded: " + e.loaded);
            console.log("Total: " + e.total);
        }
    }

    ctor.prototype.startFileUpload = function() {
        var self = this;

        var formData = new FormData($('#fileUpload')[0]);
        $.ajax({
            url: 'Feeds/' + self.feed().Name() + "/api/v2/package",
            type: 'POST',
            headers: new auth().getAuthHttpHeader(),
            xhr: function() {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    myXhr.upload.addEventListener('progress', progressHandlingFunction, false);
                }
                return myXhr;
            },
            success: function(data) {
                debugger;
                $('#fileUploadModal').modal('hide');
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                debugger;
                $('#fileUploadModal').modal('hide');
            },
            data: formData,
            cache: false,
            contentType: false,
            processData: false
        }, 'json');

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

    ctor.prototype.feedUploadAction = function() {

    };


    ctor.prototype.compositionComplete = function() {
        var self = this;

        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $(".feedMenu .item").tab();

        if (self.showSuccessMessageOnLoad() === true) {
            $(".feedMenu .item").removeClass("active");
            $(".ui.bottom.attached.tab.segment").removeClass("active");
            $(".settingTab").addClass("active");
        }

        $('.ui.dropdown.uploadPackage').dropdown({
            action: function() {
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

        $(document).on('change', '.btn-file :file', function() {
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

        $('.btn-file :file').on('fileselect', function(event, numFiles, label, size) {
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