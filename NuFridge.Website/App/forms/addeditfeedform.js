define(['plugins/router', 'api', 'auth', 'databinding-feed'], function (router, api, auth, databindingFeed) {
    var ctor = function () {
        var self = this;
        self.mode = ko.observable("Please wait");
        self.isSaving = ko.observable(false);
        self.isCancelNavigating = ko.observable(false);
        self.showSuccessMessageOnLoad = ko.observable(false);
        self.feed = ko.validatedObservable(databindingFeed());
    };

    ctor.prototype.activate = function(activationData) {
        var self = this;

        activationData.loaded.then(function() {

            self.feed(activationData.feed());

            if (activationData.mode === "Create") {
                self.mode("Create");
            } else if (activationData.mode === "Update") {
                self.mode("Update");

                if (!self.feed()) {
                    throw "A feed must be provided when using the add/edit feed form in update mode.";
                }

                self.showSuccessMessageOnLoad(activationData.showSuccessMessageOnLoad || false);

            } else {
                throw "Invalid activation data passed in to the add/edit feed form.";
            }
        });
    }

    ctor.prototype.clearApiKey = function() {
        var feed = this;
        feed.HasApiKey(false);
    };

    ctor.prototype.compositionComplete = function () {
        var self = this;

        if (self.showSuccessMessageOnLoad() === true) {
            self.showHideSaveSuccessMessage();
            self.showSuccessMessageOnLoad(false);
        }
    };


    ctor.prototype.cancelClick = function() {
        var self = this;

        self.isCancelNavigating(true);

        router.navigate("#feeds");
    };

    ctor.prototype.createFeed = function () {
        var self = this;

        self.showHideSavingMessage();
        self.isSaving(true);

        var startTime = new Date().getTime();

        $.ajax({
            url: api.post_feed,
            type: 'POST',
            data: ko.toJS(self.feed()),
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json',
            cache: false,
            success: function (result) {
                self.afterAjaxSavingMessage(startTime).done(function () {
                    router.navigate("#feeds/view/" + result.Id + "?ss=1");
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

    ctor.prototype.updateFeed = function () {
        var self = this;

        self.showHideSavingMessage();
        self.isSaving(true);

        var startTime = new Date().getTime();

        $.ajax({
            url: api.put_feed + "/" + self.feed().Id(),
            type: 'PUT',
            headers: new auth().getAuthHttpHeader(),
            data: ko.toJS(self.feed()),
            dataType: 'json',
            cache: false,
            success: function (result) {
                self.afterAjaxSavingMessage(startTime).done(function() {
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

        var isSuccessVisible = $('.feedSaveSuccessMessage').transition("is visible");
        if (isSuccessVisible) {
            self.showHideSaveSuccessMessage();
        }

        $('.feedSavingMessage').transition('scale');
    };

    ctor.prototype.showHideSaveSuccessMessage = function() {
        $('.feedSaveSuccessMessage').transition('scale');
    };

    ctor.prototype.deleteFeed = function () {
        var self = this;

        $.ajax({
            url: api.delete_feed + "/" + self.feed().Id(),
            type: 'DELETE',
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json',
            cache: false,
            success: function (result) {
                $('#deleteConfirmModal').modal('hide');

                router.navigate('#feeds');
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                $('#deleteConfirmModal').modal('hide');

                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
            }
        });
    };

    ctor.prototype.deleteClick = function () {

        var self = this;

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                self.deleteFeed();
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#deleteConfirmModal').modal(options).modal('show');
    };

    ctor.prototype.submitClick = function () {
        var self = this;

        if (!self.feed.isValid()) {
            return;
        }

        if (self.mode() === "Create") {
            self.createFeed();
        }
        else if (self.mode() === "Update") {
            self.updateFeed();
        }
    };

    return ctor;
});