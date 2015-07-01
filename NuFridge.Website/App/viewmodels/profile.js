define(['plugins/router', 'databinding-user', 'auth'], function(router, user, auth) {
    var ctor = function () {
        this.user = ko.observable(user());
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    };

    ctor.prototype.activate = function() {
        var self = this;

        $.ajax({
            url: "/api/account/administrator",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function(response) {

            var mapping = {
                create: function(options) {
                    return user(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.user);
        }).fail(function(xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.updateClick = function() {

    };

    return ctor;
});