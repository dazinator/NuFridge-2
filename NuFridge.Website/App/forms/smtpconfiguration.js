define(['plugins/router', 'api', 'auth', 'databinding-feed'], function (router, api, auth, databindingFeed) {
    var ctor = function () {
        var self = this;
        self.isSaving = ko.observable(false);
        self.isCancelNavigating = ko.observable(false);
    };

    ctor.prototype.activate = function (activationData) {
        var self = this;

    }


    ctor.prototype.compositionComplete = function () {
        var self = this;

    };

    ctor.prototype.submitClick = function() {
        var self = this;
    };

    ctor.prototype.cancelClick = function() {
        var self = this;

    };


    return ctor;
});