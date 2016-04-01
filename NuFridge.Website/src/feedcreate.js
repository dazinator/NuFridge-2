﻿import {inject} from 'aurelia-framework';
import {HttpClient, json} from 'aurelia-fetch-client';
import {Router} from 'aurelia-router';
import {authUser} from './authuser';
import {Claims} from './claims';
import {AuthService} from 'aurelia-auth';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';

@inject(HttpClient, Router, AuthService, authUser, errorParser)
export class FeedCreate {

    feed = {
        Id: 0,
        Name: "",
        ApiKey: "",
        GroupId: 0,
        Description: ""
    };

    previousPageName = "Feed Group";

    isCreatingFeed = false;
    feedName = "";

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    constructor(http, router, auth, authUser, errorParser) {
        this.http = http;
        this.router = router;
        this.auth = auth;
        this.authUser = authUser;
        this.errorParser = errorParser;
    }
    cancelClick() {
        var self = this;
        self.router.navigateBack();
    }
    insertFeed() {
        var self = this;

        $('form.segment.form').form("validate form");

        if ($('form.segment.form').form("is valid") === false) {
            return false;
        }

        self.isCreatingFeed = true;
        self.shownotification = false;

        var startDate = new Date();

    this.http.fetch("api/feeds/", {
        method: 'post', body: json(self.feed)
    }).then(response => response.json()).then(message => {
            self.feed = message;

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 1) {
                setTimeout(function() { self.router.navigate("feeds/view/" + self.feed.Id); }, (1 - secondsDifference) * 1000);
            } else {
                self.router.navigate("feeds/view/" + self.feed.Id);
            }
        },
        function(message) {
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            } else {
                self.isCreatingFeed = false;
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


    activate(params, routeConfig) {
        var self = this;

        var groupId = params.groupid;
        self.feed.GroupId = groupId;

        self.hasRequiredClaims = self.authUser.hasClaim(Claims.CanInsertFeed, Claims.SystemAdministrator);

        if (!self.hasRequiredClaims) {
            self.notificationmessage = "You are not authorized to create feeds.";
            self.notificationtype = notificationType.Warning.value;
            self.shownotification = true;
        } else {
            self.http.fetch("api/feedgroups/" + groupId).then(response => response.json()).then(message => {
                    self.feedGroup = message;
                    self.previousPageName = self.feedGroup.Name;
                },
                function(message) {
                    if (message.status === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }

    attached() {
        $('form.segment.form').form({
            inline: true,
            on: 'blur',
            fields: {
                feedname: {
                    identifier: 'feedname',
                    rules: [
                        {
                            type: 'empty',
                            prompt: 'Please enter a feed name'
                        },
                        {
                            type: 'minLength[3]',
                            prompt: 'The feed name must be at least 3 characters long'
                        }
                    ]
                }
            }
        });
    }
}