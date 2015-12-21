import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {notificationType} from 'notifications';
import {AuthService} from 'paulvanbladel/aurelia-auth';
import {errorParser} from 'errorparser';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser, errorParser)
export class Feedgroup {

    isUpdatingFeedGroup = false;
    isLoadingFeedGroup = true;

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    constructor(router, http, auth, authUser, errorParser) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
        this.isNew = true;
        this.errorParser = errorParser;
    }
    activate(params) {
        var self = this;
        self.GroupId = params.id;
    }
    deleteClick() {
        var self = this;

        self.shownotification = false;

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                self.deleteGroup();
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#deleteConfirmModal').modal(options).modal('show');
    }
    cancelClick() {
        var self = this;
        self.router.navigateBack();
    }
    deleteGroup() {
        var self = this;

        self.http.delete("api/feedgroups/" + self.GroupId).then(message => {
            $('#deleteConfirmModal').modal("hide");
            self.router.navigate("feeds");
        }, message => {
            $('#deleteConfirmModal').modal("hide");
            if (message.statusCode === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            } else {

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
    createSaveClick() {
        var self = this;

        self.shownotification = false;

        if (self.isNew) {
            self.http.post("api/feedgroups", self.feedGroup).then(message => {
                    self.router.navigate("feeds");
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                    else {
                        self.isUpdatingFeedGroup = false;
                        var parsedError = self.errorParser.parseResponse(message);
                        self.notificationmessage =  parsedError.Message;

                        if (parsedError.StackTrace) {
                            self.notificationmessage += "<br><br>Detailed Error:<br>" + parsedError.StackTrace;
                        }

                        self.shownotification = true;
                        self.notificationtype = notificationType.Warning.value;
                    }
                });
        } else {
            self.http.put("api/feedgroups/" + self.GroupId, self.feedGroup).then(message => {
                    self.router.navigate("feeds");
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                    else {
                        self.isUpdatingFeedGroup = false;
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
    attached() {
        var self = this;
        if (self.router.currentInstruction.config.route.indexOf("/create") > 0) {
            self.pageTitle = "Create Feed Group";
            self.isLoadingFeedGroup = false;
        } else {
            self.isNew = false;
            self.http.get("api/feedgroups/" + self.GroupId).then(message => {
                    self.feedGroup = JSON.parse(message.response);
                    self.pageTitle = self.feedGroup.Name;
                    self.isLoadingFeedGroup = false;
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    } else {
                        self.isLoadingFeedGroup = false;
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
}