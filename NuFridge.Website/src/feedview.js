import {inject,ObserverLocator} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
import {authUser} from './authuser';
import {Claims} from './claims';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';
import {AuthService} from 'aurelia-auth';

@inject(HttpClient, Router, authUser, errorParser, ObserverLocator, AuthService)
export class FeedView {

    feed = null;
    feedName = "Feed";

    previousPageName = "Feed Group";

    isUpdatingFeed = false;
    isLoadingFeed = true;
    isLoadingFeedConfig = true;

    isLoadingJobs = true;
    jobPageNumber = 1;
    jobPageSize = 5;
    jobsTotalPages = new Array();

    overviewPackageCount = 0;
    overviewUniquePackageCount = 0;

    isLoadingHistory = false;
    historyRecords = new Array();

    isLoadingPackages = false;
    packagesSearchTimeout = null;
    packagesSearchText = "";
    packagesSortOrder = 0;
    packagesPageSize = 10;
    packagesTotalPages = new Array();
    packagesCurrentPage = 1;
    packagesShowPrerelease = false;
    packagesTotalMatchingQuery = 0;
    packagesRecords = new Array();

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    constructor(http, router, authUser, errorParser, observerLocator, auth) {
        var self = this;
        this.http = http;
        this.router = router;
        this.auth = auth;
        this.authUser = authUser;
        this.errorParser = errorParser;

        observerLocator.getObserver(this, 'packagesSearchText').subscribe(function() {
            self.packagesSearchTextOnChange();
        });
    }

    packagesSearchTextOnChange(newValue, oldValue) {
        var self = this;
        if (self.packagesSearchTimeout) {
            window.clearTimeout(self.packagesSearchTimeout);
        }
        self.packagesSearchTimeout = setTimeout(function() {
            self.applyFilterClick();
        }, 400);
    }

    updateFeed() {
        var self = this;

        $('form.form').form("validate form");

        if ($('form.form').form("is valid") === false) {
            return false;
        }

        self.isUpdatingFeed = true;
        self.shownotification = false;

        var startDate = new Date();

        this.http.fetch("api/feeds/" + self.feed.Id, {method: 'put', body: json(self.feed)}).then(response => response.json()).then(message => {
            self.feed = response;
            self.refreshFeedName();

            this.http.fetch("api/feeds/" + self.feed.Id + "/config", {method: 'put', body: json(self.feedconfig)}).then(message => {
                var endDate = new Date();

                var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

                if (secondsDifference < 1) {
                    setTimeout(function() {
                        self.isUpdatingFeed = false;
                    }, (1 - secondsDifference) * 1000);
                } else {
                    self.isUpdatingFeed = false;
                }
            },
        function(message) {
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }else {
                self.isUpdatingFeed = false;
                var parsedError = self.errorParser.parseResponse(message);
                self.notificationmessage =  parsedError.Message;

                if (parsedError.StackTrace) {
                    self.notificationmessage += "<br><br>Detailed Error:<br>" + parsedError.StackTrace;
                }

                self.shownotification = true;
                self.notificationtype = notificationType.Warning.value;
            }
        });
        },
        function(message) {
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }else {
                self.isUpdatingFeed = false;
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

    clearApiKey() {
        var self = this;
        self.feed.HasApiKey = false;
    }

    deleteFeed() {
        var self = this;

        self.http.fetch("api/feeds/" + self.feed.Id, {method: 'delete'}).then(message => {
            $('#deleteConfirmModal').modal("hide");
            self.router.navigate("feeds");
        }, message => {
            $('#deleteConfirmModal').modal("hide");
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }
        });
    }

    reindexPackagesClick() {
        var self = this;

        this.http.fetch("api/feeds/" + self.feed.Id + "/reindex", {method: 'post'}).then(function() {
            
        },
        function(message) {
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }
        });
    }

    deleteFeedClick() {
        var self = this;

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                self.deleteFeed();
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#deleteConfirmModal').modal(options).modal('show');
    }

    resetPackageListingClick() {
        var self = this;

        self.packagesPageSize = 10;
        $('.ui.dropdown.pageSizeDropdown').dropdown('set selected', 10);

        self.packagesSortOrder = 0;
        $('.ui.dropdown.pageOrderDropdown').dropdown('set selected', 0);

        self.packagesSearchText = "";

        self.packagesTotalPages = new Array();
        self.packagesCurrentPage = 1;

        self.loadFeedPackages();
    }
    activate(params, routeConfig) {
        var self = this;
        self.routeConfig = routeConfig;

        var feedId = params.id;

        self.canViewPage = self.authUser.hasClaim(Claims.CanViewFeeds, Claims.SystemAdministrator);
        self.canReindexPackages = self.authUser.hasClaim(Claims.CanReindexPackages, Claims.SystemAdministrator);
        self.canChangePackageDirectory = self.authUser.hasClaim(Claims.CanChangePackageDirectory, Claims.SystemAdministrator);
        self.canExecuteRetentionPolicy = self.authUser.hasClaim(Claims.CanExecuteRetentionPolicy, Claims.SystemAdministrator);
        self.canViewPackages = self.authUser.hasClaim(Claims.CanViewPackages, Claims.SystemAdministrator);
        self.canViewFeedHistory = self.authUser.hasClaim(Claims.CanViewFeedHistory, Claims.SystemAdministrator);
        self.canUpdateFeed = self.authUser.hasClaim(Claims.CanUpdateFeed, Claims.SystemAdministrator);
        self.canUploadPackages = self.authUser.hasClaim(Claims.CanUploadPackages, Claims.SystemAdministrator);

        if (self.canViewPage) {
            self.loadGroup(params);
            self.http.fetch("api/feeds/" + feedId).then(message => {
                    self.feed = JSON.parse(message.response);
                    self.refreshFeedName();
                    self.loadFeedPackageCount();
                    self.loadFeedJobs();
                    self.loadFeedConfig();
                    self.isLoadingFeed = false;
                },
                function(message) {
                    if (message.status === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }
    loadGroup(params) {
        var self = this;

        self.http.fetch("api/feedgroups/" + params.groupid).then(response => response.json()).then(message => {
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
    loadFeedJobs() {
        var self = this;

        self.isLoadingJobs = true;

        self.http.fetch("api/feeds/" + self.feed.Id + "/jobs" + "?size=" + self.jobPageSize + "&page=" + self.jobPageNumber).then(response => response.json()).then(message => {
            self.jobData = message;
            self.jobsTotalPages = new Array(Math.ceil(self.jobData.Total / self.jobPageSize));
            self.isLoadingJobs = false;
            },
            function(message) {
                if (message.status === 401) {
                    var loginRoute = self.auth.auth.getLoginRoute();
                    self.auth.logout("#" + loginRoute);
                }
            });
    }

    jobClick(job) {
        var self = this;

        self.router.navigate("feeds/view/" + self.feed.Id + "/import/" + job.Id);
    }

    loadFeedConfig() {
        var self = this;

        self.http.fetch("api/feeds/" + self.feed.Id + "/config").then(response => response.json()).then(message => {
            self.feedconfig = message;
                self.isLoadingFeedConfig = false;
            },
            function(message) {
                if (message.status === 401) {
                    var loginRoute = self.auth.auth.getLoginRoute();
                    self.auth.logout("#" + loginRoute);
                }
            });
    }

    importPackagesClick() {
        var self = this;

        self.router.navigate("feeds/view/" + self.feed.Id + "/import");
    }

    jobsPreviousPageClick() {
        var self = this;

        if (self.jobPageNumber <= 1) {
            return;
        }

        self.jobPageNumber--;

        self.loadFeedJobs();
    }

    jobsGoToPageClick(page) {
        var self = this;

        self.jobPageNumber = page;
        
        self.loadFeedJobs();
    }

    jobsNextPageClick() {
        var self = this;

        if (self.jobPageNumber >= self.jobsTotalPages.length) {
            return;
        }

        self.jobPageNumber++;

        self.loadFeedJobs();
    }

    previousPageClick() {
        var self = this;

        if (self.packagesCurrentPage <= 1) {
            return;
        }

        self.packagesCurrentPage--;

        self.loadFeedPackages();
    }

    goToPageClick(page) {
        var self = this;

        self.packagesCurrentPage = page;
        
        self.loadFeedPackages();
    }

    nextPageClick() {
        var self = this;

        if (self.packagesCurrentPage >= self.packagesTotalPages.length) {
            return;
        }

        self.packagesCurrentPage++;

        self.loadFeedPackages();
    }

    applyFilterClick() {
        var self = this;

        self.packagesTotalPages = new Array();
        self.packagesCurrentPage = 1;

        self.loadFeedPackages();
    }

    loadFeedPackageCount() {
        var self = this;

        self.http.fetch("Feeds/" + self.feed.Name + "/api/v2/Search()/$count").then(response => response.json()).then(message => {
            self.overviewPackageCount = message;
        });

        self.http.fetch("Feeds/" + self.feed.Name + "/api/v2/Search()/$count?$filter=IsAbsoluteLatestVersion").then(response => response.json()).then(message => {
            self.overviewUniquePackageCount = message;
        });
    }

    viewPackageClick(pkg) {
        var self = this;

        var hash = "#feeds/view/" + self.feed.Id + "?tab=third&ps=" + self.packagesPageSize + "&pn=" + self.packagesCurrentPage + "&so=" + self.packagesSortOrder + "&st=" + self.packagesSearchText;

        window.history.replaceState(null, null, hash);

        self.router.navigate("feeds/view/" + self.feed.Id + "/package/" + pkg.Id + "/" + pkg.Version);
    }

    loadFeedPackages() {
        var self = this;

        if (self.isLoadingPackages === true) {
            return;
        }

        if (self.feed === null) {
            setTimeout(function() {
                self.loadFeedPackages();
            }, 50);
            return;
        }

        self.isLoadingPackages = true;

        var startDate = new Date();

        var skip = self.packagesPageSize * (self.packagesCurrentPage - 1);
        var take = self.packagesPageSize;

        var filter = "$filter=IsLatestVersion";

        if (self.packagesShowPrerelease === true) {
            filter = "$filter=IsAbsoluteLatestVersion";
        }

        var order = "$orderby=";

        if (self.packagesSortOrder == 0) {
            order += "DownloadCount desc";
        } else  if (self.packagesSortOrder == 1) {
            order += "DownloadCount asc";
        } else  if (self.packagesSortOrder == 2) {
            order += "Id desc";
        }  else  if (self.packagesSortOrder == 3) {
            order += "Id asc";
        }

        var search = "";

        if (self.packagesSearchText) {
            search = "&searchTerm=" + self.packagesSearchText;
        }

        var url = "Feeds/" + self.feed.Name + "/api/v2/Search()?$inlinecount=allpages" + "&" +
                  "$skip=" + skip + "&$top=" + take + "&" + filter + "&" + order + search;

        self.http.fetch(url).then(response => response.xml()).then(message => {
            var xmlDoc = $.parseXML(message), $xml = $(xmlDoc);

            var packagesTotalMatchingQuery, packagesTotalPages, packagesRecords;

            $($xml).each(function() {
                packagesTotalMatchingQuery = $(this).find("feed>count").text();
                packagesTotalPages = new Array(Math.ceil(packagesTotalMatchingQuery / self.packagesPageSize));
                packagesRecords = $.map($(this).find("feed>entry"), function(value, index) {
                    var jvalue = $(value);
                    var props = jvalue.find("properties");
                    return {
                        Title: jvalue.find("title").text(),
                        Authors: $.map($(value).find("author>name"), function(authorValue) {
                            return $(authorValue).text();
                        }),
                        Description: props.find("Description").text(),
                        IconUrl: props.find("IconUrl").text(),
                        NormalizedVersion: props.find("NormalizedVersion").text(),
                        Version: props.find("Version").text(),
                        Id: props.find("Id").text()
                    };
                });
            });

            var endDate = new Date();

            var func = function() {
                self.isLoadingPackages = false;
                self.packagesTotalMatchingQuery = packagesTotalMatchingQuery;
                self.packagesTotalPages = packagesTotalPages;
                self.packagesRecords = packagesRecords;
            };

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 0.5) {
                setTimeout(function() { func(); }, (0.5 - secondsDifference) * 1000);
            } else {
                func();
            }
        });
    }

    loadFeedHistory() {
        var self = this;

        if (self.feed === null) {
            setTimeout(function() {
                self.loadFeedHistory();
            }, 50);
            return;
        }

        self.isLoadingHistory = true;

        var startDate = new Date();

        self.http.fetch("api/feeds/" + self.feed.Id + "/history").then(response => response.json()).then(message => {
            var func = function() {
                self.isLoadingHistory = false;
                self.historyRecords = message.Results;
            };

            var endDate = new Date();

            var secondsDifference = Math.abs((startDate.getTime() - endDate.getTime()) / 1000);

            if (secondsDifference < 0.5) {
                setTimeout(function() { func(); }, (0.5 - secondsDifference) * 1000);
            } else {
                func();
            }
        },
        function(message) {
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }
        });
    }

    refreshFeedName() {
        var self = this;

        self.routeConfig.navModel.setTitle("Viewing " + self.feed.Name);
        self.feedName = self.feed.Name;
    }

    getUrlVars()
    {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for(var i = 0; i < hashes.length; i++)
        {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }

    getUrlQueryValue(index) {
        var hash = window.location.hash;
        var beginningOfQuery = hash.substr(index);
        var endOfQueryIndex = beginningOfQuery.indexOf("&");
        if (endOfQueryIndex < 0) {
            endOfQueryIndex = beginningOfQuery.length;
        }
        var value = beginningOfQuery.substr(0, endOfQueryIndex);
        return value;
    }

    attached() {
        var self = this;

        $(".feedMenu .item").tab({
            onFirstLoad: function(tabPath, parameterArray, historyEvent) {
                if (tabPath === "first") {
                    return;
                }

                if (tabPath === "third") {
                    self.loadFeedPackages();
                }
                else if (tabPath === "fourth") {
                    self.loadFeedHistory();
                }
            },
            onLoad: function(tabPath, parameterArray, historyEvent) {
                var hash = window.location.hash;
                if (hash.indexOf("?tab=") > 0) {
                    hash = hash.substr(0, hash.indexOf("?tab="));
                } 

                hash += "?tab=" + tabPath;
                window.history.replaceState(null, null, hash);
            }
        });

        var tabIndex = window.location.hash.indexOf("?tab=");
        if (tabIndex > 0) {
            var tabName = self.getUrlQueryValue(tabIndex + 5);

            if (tabName === "third") {
                var indexOfPageSize = window.location.hash.indexOf("&ps="),
                    indexOfSortOrder = window.location.hash.indexOf("&so="),
                    indexOfSearchTerm = window.location.hash.indexOf("&st="),
                    indexOfPageNumber = window.location.hash.indexOf("&pn=");

                var value;

                if (indexOfPageSize > 0) {
                    value = parseInt(self.getUrlQueryValue(indexOfPageSize + 4), 10);
                    if (!isNaN(value)) {
                        self.packagesPageSize = value;
                        $('.ui.dropdown.pageSizeDropdown').dropdown('set selected', value);
                    }
                }
                if (indexOfSortOrder > 0) {
                    value = parseInt(self.getUrlQueryValue(indexOfSortOrder + 4), 10);
                    if (!isNaN(value)) {
                        self.packagesSortOrder = value;
                        $('.ui.dropdown.pageOrderDropdown').dropdown('set selected', value);
                    }
                }
                if (indexOfSearchTerm > 0) {
                    self.packagesSearchText = self.getUrlQueryValue(indexOfSearchTerm + 4);
                }
                if (indexOfPageNumber > 0) {
                    value = parseInt(self.getUrlQueryValue(indexOfPageNumber + 4), 10);
                    if (!isNaN(value)) {
                        self.packagesCurrentPage = value;
                    }
                    
                }
            }

            $(".feedMenu .item").tab('change tab', tabName);
        }

        $('.ui.dropdown.pageSizeDropdown').dropdown({
            onChange: function(value) {
                self.packagesPageSize = value;
                self.applyFilterClick();
            }
        });

        $('.ui.dropdown.pageOrderDropdown').dropdown({
            onChange: function(value) {
                self.packagesSortOrder = value;
                self.applyFilterClick();
            }
        });

        $('.ui.checkbox.allowPackageOverwrite').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedconfig.AllowPackageOverwrite = true;
            },
            onUnchecked: function () {
                self.feedconfig.AllowPackageOverwrite = false;
            }
        });

        $('.ui.checkbox.retentionPolicyEnabled').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.feedconfig.RetentionPolicyEnabled = true;
            },
            onUnchecked: function () {
                self.feedconfig.RetentionPolicyEnabled = false;
            }
        });

        $('form.form').form({
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
                },
                maxreleasepackages: {
                    identifier: 'maxreleasepackages',
                    rules: [
                        {
                            type: 'integer[0..2147483647]',
                            prompt: 'Please enter an integer value'
                        }
                    ]
                },
                maxprereleasepackages: {
                    identifier: 'maxprereleasepackages',
                    rules: [
                        {
                            type: 'integer[0..2147483647]',
                            prompt: 'Please enter an integer value'
                        }
                    ]
                }
            }
        });
    }
}