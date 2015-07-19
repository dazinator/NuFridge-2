define(['plugins/router', 'api', 'auth', 'databinding-feedconfig'], function (router, api, auth, databindingFeedConfig) {
    var ctor = function () {
        var self = this;
        self.mode = "Update";
        self.isSaving = ko.observable(false);
        self.isCancelNavigating = ko.observable(false);
        self.feedconfig = ko.validatedObservable(databindingFeedConfig());
    };

    ctor.prototype.activate = function (activationData) {
        var self = this;

        activationData.loaded.then(function() {

            self.feedconfig(activationData.feedconfig());

            if (activationData.mode === "Update") {

                if (!self.feedconfig()) {
                    throw "A feed config must be provided when using the edit feed package retention form.";
                }

            } else {
                throw "Invalid activation data passed in to the edit feed package retention form.";
            }
        });
    }

    ctor.prototype.compositionComplete = function () {
        var self = this;
        $('.ui.checkbox.retentionPolicyEnabled').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedconfig().RetentionPolicyEnabled(true);
            },
            onUnchecked: function () {
                self.feedconfig().RetentionPolicyEnabled(false);
            }
        });

        if (self.feedconfig().RetentionPolicyEnabled() === true) {
            $('.ui.checkbox.retentionPolicyEnabled').checkbox('check');
        }

        $('.ui.checkbox.retentionPolicyDeleteEnabled').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedconfig().RpDeletePackages(true);
            },
            onUnchecked: function () {
                self.feedconfig().RpDeletePackages(false);
            }
        });

        if (self.feedconfig().RpDeletePackages() === true) {
            $('.ui.checkbox.retentionPolicyDeleteEnabled').checkbox('check');
        }
    };


    ctor.prototype.cancelClick = function () {
        var self = this;

        self.isCancelNavigating(true);

        router.navigate("#feeds");
    };


    ctor.prototype.updateFeedConfig = function () {
        var self = this;

        self.showHideSavingMessage();
        self.isSaving(true);

        var startTime = new Date().getTime();

        $.ajax({
            url: api.put_feed + "/" + self.feedconfig().FeedId() + "/config",
            type: 'PUT',
            headers: new auth().getAuthHttpHeader(),
            data: ko.toJS(self.feedconfig()),
            dataType: 'json',
            cache: false,
            success: function (result) {
                self.afterAjaxSavingMessage(startTime).done(function () {
                    self.showHideSaveSuccessMessage();
                    self.isSaving(false);
                });
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                self.afterAjaxSavingMessage(startTime);
                self.isSaving(false);

                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
            }
        });
    };

    ctor.prototype.afterAjaxSavingMessage = function (startTime) {

        var self = this;

        var endTime = new Date().getTime();

        var dfd = jQuery.Deferred();

        var diff = (startTime - endTime) / 1000;
        var seconds = Math.abs(diff);

        if (seconds < 3) {
            setTimeout(function () {
                self.showHideSavingMessage();
                dfd.resolve();
            }, (3 - seconds) * 1000);
        } else {
            self.showHideSavingMessage();
            dfd.resolve();
        }

        return dfd.promise();
    };

    ctor.prototype.showHideSavingMessage = function () {
        var self = this;

        var isSuccessVisible = $('.feedConfigSaveSuccessMessage').transition("is visible");
        if (isSuccessVisible) {
            self.showHideSaveSuccessMessage();
        }

        $('.feedConfigSavingMessage').transition('scale');
    };

    ctor.prototype.showHideSaveSuccessMessage = function () {
        $('.feedConfigSaveSuccessMessage').transition('scale');
    };


    ctor.prototype.submitClick = function () {
        var self = this;

        if (!self.feedconfig.isValid()) {
            return;
        }

        if (self.mode === "Update") {
            self.updateFeedConfig();
        }
    };

    return ctor;
});