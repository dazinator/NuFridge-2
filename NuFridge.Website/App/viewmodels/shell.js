define(['plugins/router', 'durandal/app', 'auth'], function (router, app, auth) {
    var ctor = function () {
        var self = this;
        self.router = router;
    };

    ctor.prototype.activate = function() {
        router.map([
            { route: '', title: 'Home', moduleId: 'viewmodels/home', nav: true, icon: "home icon" },
            { route: 'feeds', title: 'Feeds', moduleId: 'viewmodels/feeds', nav: true, icon: "cubes icon" },
            { route: 'profile', title: 'Profile', moduleId: 'viewmodels/profile', nav: true, icon: "user icon" },
            { route: 'settings', title: 'Settings', moduleId: 'viewmodels/settings', nav: true, icon: "setting icon"},
            { route: 'feeds/view/:id', title: 'View Feed', moduleId: 'viewmodels/editfeed', nav: false },
            { route: 'feeds/create', title: 'Create Feed', moduleId: 'viewmodels/addfeed', nav: false },
            { route: 'signin', title: 'Sign in', moduleId: 'viewmodels/registersignin', nav: false },
            { route: 'signout', title: 'Sign out', nav: true, icon: "sign out icon" }
        ]).buildNavigationModel();

        router.on('router:navigation:processing').then(function() {
            var self = this;
        });

        router.on('router:navigation:viewLoaded').then(function() {
            var self = this;
        });

        router.guardRoute = function (model, route) {
            if (route.fragment === "signin")
                return true;

            return new auth().loggedIn() || '#signin';
        };


        return router.activate();
    };

    return ctor;
});