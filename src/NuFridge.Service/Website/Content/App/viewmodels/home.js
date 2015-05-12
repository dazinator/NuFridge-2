define(['plugins/router', 'databinding-dashboard'], function (router, dashboard) {
    var ctor = function () {
        var self = this;
        self.displayName = 'Dashboard';
        self.dashboard = ko.observable(dashboard());
    };

    ctor.prototype.activate = function () {
        var self = this;

        $.ajax({
            url: "/api/dashboard",
            cache: false,
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return dashboard(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.dashboard);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            Materialize.toast(errorThrown, 7500);
        });
    };

    ctor.prototype.goToFeeds = function() {
        router.navigate("#feeds");
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $('#dashboardTabs').tabs();
    };

    return ctor;
});