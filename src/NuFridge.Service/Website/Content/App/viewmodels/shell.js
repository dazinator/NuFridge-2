define(['plugins/router', 'durandal/app'], function (router, app) {
    var ctor = function () {
        var self = this;
        self.router = router;

        self.router.navigateStartTime = new Date();
        self.router.navigateRenderTime = ko.observable('');
    };

    ctor.prototype.activate = function () {
        router.map([
    { route: '', title: 'Home', moduleId: 'viewmodels/home', nav: true, icon: "mdi-action-home" },
    { route: 'feeds', title: 'Feeds', moduleId: 'viewmodels/feeds', nav: true, icon: "mdi-navigation-apps" },
    { route: 'profile', title: 'Profile', moduleId: 'viewmodels/profile', nav: true, icon: "mdi-action-account-box" },
    { route: 'settings', title: 'Settings', moduleId: 'viewmodels/settings', nav: true, icon: "mdi-action-settings-applications" },
    { route: 'feeds/view/:id', title: 'View Feed', moduleId: 'viewmodels/editfeed', nav: false },
    { route: 'feeds/create', title: 'Create Feed', moduleId: 'viewmodels/addfeed', nav: false }
        ]).buildNavigationModel();

        router.on('router:navigation:processing').then(function () {
            var self = this;
            self.navigateStartTime = new Date();
            $("#progressBar").attr("aria-busy", true);
        });

        router.on('router:navigation:viewLoaded').then(function() {
            var self = this;

            $("#progressBar").attr("aria-busy", false);

            var dif = self.navigateStartTime.getTime() - new Date().getTime();
            var seconds = Math.abs(dif / 1000);

            if (seconds >= 1.0) {
                self.navigateRenderTime('Page created in ' + seconds + ' seconds.');
            } else {
                var ms = seconds * 1000;
                self.navigateRenderTime('Page generated in ' + ms + 'ms.');
            }
        });

        return router.activate();
    }

    return ctor;
});