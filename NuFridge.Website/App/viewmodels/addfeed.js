define(['plugins/router', 'databinding-feed'], function (router, databindingFeed) {
    var ctor = function () {
        var self = this;

        self.feed = ko.validatedObservable(databindingFeed());
    };

    ctor.prototype.activate = function () {
        var self = this;

        self.feedOptions = {
            mode: "Create",
            feed: self.feed
        };
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    };


    return ctor;
});