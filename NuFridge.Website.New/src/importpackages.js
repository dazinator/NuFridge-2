import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import moment from 'moment';
import {authUser} from './authuser';
import {Claims} from './claims';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';
import {computedFrom} from 'aurelia-framework';
import 'ms-signalr-client';

@inject(HttpClient, Router, authUser, errorParser)
export class ImportPackages {

    options = {
        FeedUrl: "",
        IncludePrerelease: true,
        CheckLocalCache: true
    };

    detailedJob = {
        Scheduled: 0,
        Processed: 0
    };

    job = {
        Name: "Import Packages"
    }

    connection = $.hubConnection();
    proxy = null;

    items = new Array();

    showForm = false;
    showProgress = false;

    cancelImport() {
        var self = this;

        self.http.delete("/api/feeds/" + self.feedId + "/import/" + self.jobId).then(message => {

        },
            function(message) {
                if (message.statusCode === 401) {

                }
            });
    }

    startImport() {
        var self = this;

        if (self.connection.state !== 1) {
            return;
        }

        self.http.post("/api/feeds/" + self.feedId + "/import", self.options).then(message => {
            self.jobId = message.response;

            window.history.replaceState(null, null, "#feeds/view/" + self.feedId + "/import/" + self.jobId);

            self.proxy.invoke('Subscribe', self.jobId);

            self.showForm = false;
            self.showProgress = true;
        },
        function(message) {
            if (message.statusCode === 401) {

            }
        });
    }

    arrayUnique(array) {
        var a = array.concat();
        for(var i=0; i<a.length; ++i) {
            for(var j=i+1; j<a.length; ++j) {
                if(a[i].Id === a[j].Id)
                    a.splice(j--, 1);
            }
        }

        return a;
    };

    parsePackageItem(pkg) {
        pkg.Items = JSON.parse(pkg.JSON).Items;
        pkg.JSON = undefined;
        return pkg;
    }

    packageClick(pkg) {
        var self = this;
        pkg.IsExpanded = !pkg.IsExpanded;
    }

    getLevelColour(log) {
        if (log.Level === 'Info') {
            return "grey";
        }
        if (log.Level === 'Error') {
            return "red";
        }
        if (log.Level === 'Warning') {
            return "orange";
        }
        return "";
    }

    configureSignalRProxy() {
        var self = this;

        var d = new $.Deferred();

        self.proxy = self.connection.createHubProxy('importPackagesHub');

        self.proxy.on('packageProcessed', function(message) {
            var pkg = self.parsePackageItem(message);

            var beforeCount = self.items.length;

            self.items = self.arrayUnique(self.items.concat([pkg]));

            var afterCount = self.items.length - beforeCount;

            if (afterCount > 0) {
                $('#importProgressBar').progress('increment', afterCount);
            }
        });

        self.proxy.on('loadDetailedJob', function(job) {
            if (job) {
                var scheduled = self.detailedJob.Scheduled; 

                self.detailedJob = job;

                if (scheduled < self.detailedJob.Scheduled) {
                    $('#importProgressBar').progress({
                        total: self.detailedJob.Scheduled,
                        text: {
                            active  : 'Importing {value} of {total} packages',
                            success : '{total} packages processed'
                        }
                    });
                }
            }
        });

        self.proxy.on('loadJob', function(job) {
            if (job) {
                self.job = job;
            }
        });

        self.proxy.on('importCancelled', function() {

        });

        self.proxy.on('loadPackages', function(message) {
            var items = message;

            $.each(items, function(index, element) {
                self.parsePackageItem(element);
            });

            var beforeCount = self.items.length;

            self.items = self.arrayUnique(self.items.concat(items));

            var afterCount = self.items.length - beforeCount;

            if (afterCount > 0) {
                $('#importProgressBar').progress('increment', afterCount);
            }
        });

        self.connection.start({ jsonp: false })
        .done(function() {
            d.resolve();
        })
        .fail(function() {
            d.reject();
        });

        return d.promise();
    }
    

    constructor(http, router, authUser, errorParser) {
        var self = this;
        this.http = http;
        this.router = router;
        this.authUser = authUser;
        this.errorParser = errorParser;
    }

    activate(params, routeConfig) {
        var self = this;
        self.feedId = params.id;
        self.jobId = params.jobid;

        if (self.jobId) {
            self.showForm = false;
            self.showProgress = true;

            self.configureSignalRProxy().then(function() {
                self.proxy.invoke('Subscribe', self.jobId);
            });
        } else {
            self.showForm = true;
            self.configureSignalRProxy();
        }
    }

    attached() {
        var self = this;

        $(".importMenu .item").tab({
            onFirstLoad: function(tabPath, parameterArray, historyEvent) {

            },
            onLoad: function(tabPath, parameterArray, historyEvent) {

            }
        });

        $('.ui.checkbox.includePrerelease').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.options.IncludePrerelease = true;
            },
            onUnchecked: function () {
                self.options.IncludePrerelease = false;
            }
        });

        $('.ui.checkbox.checkLocalCache').checkbox({
            fireOnInit: false,
            onChecked: function () {
                self.options.CheckLocalCache = true;
            },
            onUnchecked: function () {
                self.options.CheckLocalCache = false;
            }
        });

        $('#importProgressBar').progress({
            text: {
                active  : 'Importing {value} of {total} packages',
                success : '{total} packages processed'
            }
        });
    }
}