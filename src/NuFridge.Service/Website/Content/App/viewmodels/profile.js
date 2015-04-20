define(['plugins/router', 'databinding/user'], function(router, user) {
    var ctor = function () {
        this.displayName = 'Profile';
        this.user = ko.observable(user());
    };

    ctor.prototype.compositionComplete = function() {
        $('#profileTabs').tabs();
    }

    ctor.prototype.activate = function () {
        var self = this;

        $.ajax({
            url: "/api/Account/administrator",
            cache: false,
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return user(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.user);
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            router.navigate("#");
            Materialize.toast(errorThrown, 7500);
        });
    }

    ctor.prototype.updateClick = function() {
        Materialize.toast('Not implemented.', 7500);
    }

    return ctor;
});