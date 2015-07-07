define(['api', 'cookie'], function (api, cookie) {
    var ctor = function () {

    };

    ctor.prototype.trySignIn = function (requestData) {
        var self = this;

        var dfd = jQuery.Deferred();

        $.ajax({
            url: api.signin,
            type: 'POST',
            data: ko.toJS(requestData),
            dataType: 'json',
            cache: false,
            success: function (result) {

                $.cookie('AuthToken', result.Token, { path: '/', expires: 7 });

                dfd.resolve();
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {

                self.deleteCookie();

                dfd.reject();
            }
        });

        return dfd.promise();
    };

    ctor.prototype.deleteCookie = function() {
        $.removeCookie('AuthToken', { path: '/' });
    }

    ctor.prototype.getAuthToken = function() {
        return $.cookie('AuthToken');
    };

    ctor.prototype.loggedIn = function() {
        var self = this;
        return self.getAuthToken() != null;
    };

    ctor.prototype.getAuthHttpHeader= function() {
        var self = this;

        return {
            Authorization: "Token " + self.getAuthToken()
        };
    };

    return ctor;
});