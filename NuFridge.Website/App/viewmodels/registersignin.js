define(['plugins/router', 'api', 'databinding-signinrequest', 'auth'], function (router, api, signinrequest, auth) {
    var ctor = function () {
        var self = this;

        self.signInRequest = ko.validatedObservable(signinrequest());
    };

    ctor.prototype.activate = function () {
        var self = this;

        new auth().deleteCookie();

        self.signInRequest().UserName("admin");
        self.signInRequest().Password("password");
    };

    ctor.prototype.deactivate = function () {
        var self = this;

        $(".left.sidebar.menu").show();
        $(".page-host").removeClass("signInRegisterPage");
    };

    ctor.prototype.trySignIn = function() {
        var self = this;

        new auth().trySignIn(self.signInRequest()).done(function() {
            router.navigate("#");
        }).fail(function() {

        });
    };

    ctor.prototype.compositionComplete = function () {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $(".page-host").addClass("signInRegisterPage");
        $(".left.sidebar.menu").hide();
    };

    return ctor;
});