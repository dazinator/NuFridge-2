define(['plugins/router', 'durandal/app'], function (router, app) {
    return {
        router: router,
        activate: function () {
            router.map([
                { route: '', title: 'Home', moduleId: 'viewmodels/home', nav: true, icon: "mdi-action-home" },
                { route: 'feeds', title: 'Feeds', moduleId: 'viewmodels/feeds', nav: true, icon: "mdi-navigation-apps" },
                { route: 'profile', title: 'Profile', moduleId: 'viewmodels/profile', nav: true, icon: "mdi-action-account-box" },
                { route: 'settings', title: 'Settings', moduleId: 'viewmodels/settings', nav: true, icon: "mdi-action-settings-applications" },
                { route: 'feeds/view/:id', title:'View Feed', moduleId: 'viewmodels/editfeed', nav: false }, 
                { route: 'feeds/create', title: 'Create Feed', moduleId: 'viewmodels/addfeed', nav: false }
            ]).buildNavigationModel();

            router.on('router:navigation:complete').then(function() {

            });

            router.on('router:navigation:processing').then(function () {
                $("#progressBar").attr("aria-busy", true);
            });

            return router.activate();
        }
    };
});