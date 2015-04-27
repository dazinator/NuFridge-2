define(['plugins/router'], function (router) {
    var ctor = function () {
        this.displayName = 'Welcome!';
    };

    ctor.prototype.activate = function () {

    }
    ctor.prototype.compositionComplete = function () {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    }

    return ctor;
});