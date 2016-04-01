﻿import {inject} from 'aurelia-framework';
import {authUser} from './authuser';
import {AuthService} from 'aurelia-auth';
import {Claims} from './claims';
import {HttpClient, json} from 'aurelia-fetch-client';
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

    activate(params) {
        var self = this;
        self.canUpdateUsers = self.authUser.hasClaim(Claims.CanUpdateUsers, Claims.SystemAdministrator);

        var userId = params.id;
        if (userId) {
            self.http.fetch("api/account/" + userId).then(response => response.json()).then(message => {
                self.user = message;
            });
        } else {
            self.user = self.authUser.data;
        }
    }

    attached()
    {
        $('form.segment.form')
            .form({
                inline: true,
                on: 'blur',
                fields: {
                    displayname: {
                        identifier: 'displayname',
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Please enter a display name'
                            },
                            {
                                type: 'minLength[3]',
                                prompt: 'Your display name must be at least 3 characters long'
                            }
                        ]
                    },
                    email: {
                        identifier: 'email',
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Please enter an email address'
                            },
                            {
                                type: 'email',
                                prompt: 'Please enter a valid email address'
                            }
                        ]
                    }
                }
            });
    }

    updateUser() {
        var self = this;

        $('form.segment.form').form("validate form");

        if ($('form.segment.form').form("is valid") === false) {
            return false;
        }

        self.isUpdatingUser = true;
        self.shownotification = false;

        var startDate = new Date();

        this.http.fetch("api/account/" + self.user.Id, {method: 'post', body: json(self.user)}).then(message => {

            var endDate = new Date();

            if (self.user.Id === self.authUser.data.Id) {
                self.authUser.SetDisplayName(self.user.DisplayName);
            }

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
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
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