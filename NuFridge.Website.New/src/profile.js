import {inject} from 'aurelia-framework';
import {authUser} from './authuser';
import {AuthService} from 'aurelia-auth';
import {Claims} from './claims';
import {HttpClient} from 'aurelia-http-client';
import {Router} from 'aurelia-router';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';

@inject(AuthService, authUser, HttpClient, Router, errorParser)
export class Profile {

    isUpdatingUser = false;

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    constructor(auth, authUser, http, router, errorParser) {
        this.authUser = authUser;
        this.auth = auth;
        this.http = http;
        this.errorParser = errorParser;
        this.router = router;
    }

    activate() {
        var self = this;
        self.canUpdateUsers = self.authUser.hasClaim(Claims.CanUpdateUsers, Claims.SystemAdministrator);
    }

    attached() {

    }

    updateUser() {
        var self = this;

        self.isUpdatingUser = true;
        self.shownotification = false;

        var startDate = new Date();

        this.http.post("/api/account/", self.authUser.data).then(message => {

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 1) {
                setTimeout(function() {
                    self.isUpdatingUser = false;
                }, (1 - secondsDifference) * 1000);
            } else {
                self.isUpdatingUser = false;
            }
        },
        function(message) {
            if (message.statusCode === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout(loginRoute);
            } else {
                self.isUpdatingUser = false;
                var parsedError = self.errorParser.parseResponse(message);
                self.notificationmessage =  parsedError.Message;

                if (parsedError.StackTrace) {
                    self.notificationmessage += "<br><br>Detailed Error:<br>" + parsedError.StackTrace;
                }

                self.shownotification = true;
                self.notificationtype = notificationType.Warning.value;
            }
        });
    }
}