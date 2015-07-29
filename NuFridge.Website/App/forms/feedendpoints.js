define(['plugins/router', 'databinding-feed'], function (router, databindingFeed) {
    var ctor = function () {
        var self = this;
        self.feed = ko.validatedObservable(databindingFeed());
    };

    ctor.prototype.activate = function (activationData) {
        var self = this;

        activationData.loaded.then(function () {

            self.feed(activationData.feed());

            if (!self.feed()) {
                throw "A feed must be provided when using the feed endpoints view.";
            }
        });
    };

    return ctor;
});