import {ObserverLocator, inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
import {notificationType} from 'notifications';
import {AuthService} from 'aurelia-auth';
import {errorParser} from 'errorparser';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser, errorParser, ObserverLocator)
export class Feedgroup {

    isUpdatingFeedGroup = false;
    isLoadingFeedGroup = true;

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    constructor(router, http, auth, authUser, errorParser, observerLocator) {
        this.router = router;
        this.observerLocator = observerLocator;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
        this.isNew = true;
        this.errorParser = errorParser;
    }
    populateGroupFeedDropdown() {
        var self = this;

        var hasLoadedDropdown = false;

        $('.search.dropdown.feedGroups')
            .dropdown({
                maxSelections: false,
                allowAdditions: false,
                fireOnInit: false,
                allowCategorySelection: false,
                onChange: function(value, text, choice) {
                    if (!hasLoadedDropdown)
                        return false;

                    if (typeof choice === 'string') {
                        self.feedGroup.Feeds.push({Id: text, Name: choice, GroupId: self.GroupId});
                    } else {
                        var id = choice.attr('data-value');
                        for(var i = self.feedGroup.Feeds.length - 1; i >= 0; i--) {
                            if(self.feedGroup.Feeds[i].Id == id) {
                                self.feedGroup.Feeds.splice(i, 1);
                                return false;
                            }
                        }
                    }
                }
            });

        var checkExist = setInterval(function() {
            if ($('.search.dropdown.feedGroups input.search').length) {
                for (var i = 0; i < self.feedGroup.Feeds.length; i++) {
                    $('.search.dropdown.feedGroups').dropdown("set selected", self.feedGroup.Feeds[i].Id);
                }
                clearInterval(checkExist);
                self.isLoadingFeedGroup = false;
                hasLoadedDropdown = true;
            }
        }, 100);
    }
    activate(params) {
        var self = this;
        self.GroupId = params.groupid;
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

        self.http.fetch("api/feedgroups/" + self.GroupId, {method: 'delete'}).then(message => {
            $('#deleteConfirmModal').modal("hide");
            self.router.navigate("feeds");
        }, message => {
            $('#deleteConfirmModal').modal("hide");
            if (message.status === 401) {
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
            self.http.fetch("api/feedgroups", {method: 'post', body: json(self.feedGroup)}).then(message => {
                self.router.navigate("feeds");
            },
                function(message) {
                    if (message.status === 401) {
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
            self.http.fetch("api/feedgroups/" + self.GroupId, {method: 'put', body: json(self.feedGroup)}).then(message => {
                self.router.navigate("feeds");
            },
                function(message) {
                    if (message.status === 401) {
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

            self.http.fetch("api/feeds").then(response => response.json()).then(message => {
                    var feedGroups = message;

                    self.feeds = [];

                    for (var i = 0; i < feedGroups.length; i++) {
                        if (feedGroups[i].Id == self.GroupId) {
                            self.feedGroup = feedGroups[i];
                            self.pageTitle = self.feedGroup.Name;
                        }
                        for (var j = 0; j < feedGroups[i].Feeds.length; j++) {
                            feedGroups[i].Feeds[j].GroupName = feedGroups[i].Name;
                        }
                        self.feeds = self.feeds.concat(feedGroups[i].Feeds);
                    }

                    self.populateGroupFeedDropdown();
                },
                function(message) {
                    if (message.status === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    } else {
                        self.isLoadingFeedGroup = false;
                        var parsedError = self.errorParser.parseResponse(message);
                        self.notificationmessage = parsedError.Message;

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