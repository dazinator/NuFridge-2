define(['plugins/router', 'api', 'auth'], function (router, api, auth) {
    var ctor = function () {

    };

    ctor.prototype.activate = function () {
        new auth().deleteCookie();

        router.navigate("#signin");
    };

    return ctor;
});