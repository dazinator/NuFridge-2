define(['plugins/router', 'databinding/systeminfo'], function (router, systeminfo) {
    var ctor = function () {
        var self = this;
        self.displayName = 'Settings';
        self.systemInfo = ko.observable(systeminfo());
    };

    ctor.prototype.compositionComplete = function () {
        $('#settingsTabs').tabs();

        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    }

    ctor.prototype.activate = function () {
        var self = this;

        $.ajax({
            url: "/api/diagnostics",
            cache: false,
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return systeminfo(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.systemInfo);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            router.navigate("#");
            Materialize.toast(errorThrown, 7500);
        });
    }
    return ctor;
});