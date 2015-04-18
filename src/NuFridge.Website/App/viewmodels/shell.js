define(['plugins/router', 'durandal/app'], function (router, app) {
    return {
        router: router,
        activate: function () {
            router.map([
                { route: '', title: 'Home', moduleId: 'viewmodels/home', nav: true, icon: "mdi-action-home" },
                { route: 'feeds', moduleId: 'viewmodels/feeds', nav: true, icon: "mdi-navigation-apps" },
                { route: 'profile', moduleId: 'viewmodels/profile', nav: true, icon: "mdi-action-account-box" },
                { route: 'settings', moduleId: 'viewmodels/settings', nav: true, icon: "mdi-action-settings-applications" },
                { route: 'feeds/:id', moduleId: 'viewmodels/addeditfeed', nav: false }, 
                { route: 'feeds/create', moduleId: 'viewmodels/addeditfeed', nav: false }
            ]).buildNavigationModel();

            router.on('router:navigation:complete').then(function() {
                $("#progressBar").attr("aria-busy", false);
            });

            router.on('router:navigation:processing').then(function () {
                $("#progressBar").attr("aria-busy", true);
            });

            return router.activate();
        }
    };
});