define(['plugins/router', 'databinding-feed', 'databinding-feedconfig'], function (router, databindingFeed, databindingFeedConfig) {
    var ctor = function () {
        var self = this;

        self.feed = ko.validatedObservable(databindingFeed());
        self.feedconfig = ko.validatedObservable(databindingFeedConfig());
    };

    ctor.prototype.activate = function () {
        var self = this;

        self.feedOptions = {
            mode: "Create",
            feed: self.feed,
            feedconfig: self.feedconfig,
            loaded: new jQuery.Deferred().resolve().promise()
        };
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    };


    return ctor;
});