import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
import moment from 'moment';
import {authUser} from './authuser';
import {Claims} from './claims';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';

@inject(HttpClient, Router, authUser, errorParser)
export class Package {

    constructor(http, router, authUser, errorParser) {
        this.http = http;
        this.router = router;
        this.authUser = authUser;
        this.errorParser = errorParser;
    }

    feed = null;
    pkg = null;
    versionsOfPackage = new Array();
    isLoadingPackage = true;

    activate(params, routeConfig) {
        var self = this;
        self.routeConfig = routeConfig;

        var feedId = params.id;
        var packageId = params.packageid;
        var packageVersion = params.packageversion;

        self.canViewPage = self.authUser.hasClaim(Claims.CanViewPackages, Claims.SystemAdministrator);
        self.canDeletePackages = self.authUser.hasClaim(Claims.CanDeletePackages, Claims.SystemAdministrator);

        if (self.canViewPage) {
            self.loadPackage(feedId, packageId, packageVersion);
        }
    }

    loadPackage(feedId, packageId, packageVersion) {
        var self = this;

        self.isLoadingPackage = true;

        self.http.fetch("api/feeds/" + feedId).then(response => response.json()).then(message => {
            self.feed = message;

            self.http.fetch("feeds/" + self.feed.Name + "/api/v2/Packages(Id='" + packageId + "',Version='" + packageVersion + "')").then(response => response.json()).then(message => {
                var pkg = message.d;
                pkg.Tags = pkg.Tags ? pkg.Tags.replace(/^\s+|\s+$/g, '').split(" ") : new Array();
                pkg.Owners = pkg.Owners ? pkg.Owners.replace(/^\s+|\s+$/g, '').split(",") : new Array();
                pkg.Authors = pkg.Authors ? pkg.Authors.replace(/^\s+|\s+$/g, '').split(",") : new Array();
                pkg.DownloadUrl = "feeds/" + self.feed.Name + "/packages/" + pkg.Id + "/" + pkg.NormalizedVersion;
                pkg.Dependencies = pkg.Dependencies ? pkg.Dependencies.split("|").map(function(value) {
                    var versionIndex = value.indexOf(":");
                    var version = value.substr(versionIndex + 1, value.length - versionIndex - 2);
                    var id = value.substr(0, versionIndex);
                    return {
                        Id: id,
                        Version: version
                    }
                }) : new Array();
                pkg.InstallCommand = "Install-Package " + pkg.Id;

                if (pkg.IsLatestVersion) {
                    //Do nothing
                }
                else if (pkg.IsAbsoluteLatestVersion && !pkg.IsLatestVersion) {
                    pkg.InstallCommand += " -Pre";
                }
                else {
                    pkg.InstallCommand += " -Version " + pkg.NormalizedVersion;
                    if (pkg.IsPrerelease) {
                        pkg.InstallCommand += " -Pre";
                    }
                }

                self.package = pkg;
                self.isLoadingPackage = false;
            });

            self.http.fetch("feeds/" + self.feed.Name + "/api/v2/FindPackagesById()?$top=100&id='" + packageId + "'").then(response => response.json()).then(message => {
                self.versionsOfPackage = message.d.results;
            });
        },
    function(message) {
        if (message.status === 401) {
            var loginRoute = self.auth.auth.getLoginRoute();
            self.auth.logout("#" + loginRoute);
        }
    });
    }

    packageHistoryVersionClick(pkg) {
        var self = this;
        if (self.package.NormalizedVersion === pkg.NormalizedVersion) {
            return;
        }

        var hash = "#feeds/view/" + self.feed.Id + "/package/" + pkg.Id + "/" + pkg.Version;

        window.history.replaceState(null, null, hash);

        self.loadPackage(self.feed.Id, pkg.Id, pkg.Version);
    }

    deletePackage(pkg) {
        var self = this;

        self.http.fetch("feeds/" + self.feed.Name + "/api/v2/package/" + pkg.Id + "/" + pkg.Version, {method: 'delete'}).then(message => {
            $('#deleteConfirmModal').modal("hide");
            self.packageToUnlist = null;
        }, message => {
            $('#deleteConfirmModal').modal("hide");
            self.packageToUnlist = null;
            if (message.status === 401) {
                var loginRoute = self.auth.auth.getLoginRoute();
                self.auth.logout("#" + loginRoute);
            }
        });
    }

    unlistPackageClick(pkg) {
        var self = this;
        self.packageToUnlist = pkg;

        var options = {
            closable: false,
            onApprove: function (sender) {
                var modal = this;
                $(modal).find(".ui.button.deny").addClass("disabled");
                $(sender).addClass("loading").addClass("disabled");
                self.deletePackage(pkg);
                return false;
            },
            transition: 'horizontal flip',
            detachable: false
        };

        $('#deleteConfirmModal').modal(options).modal('show');
    }
}