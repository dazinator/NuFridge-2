import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
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

        if (self.canViewPage) {
            self.http.get("/api/feeds/" + feedId).then(message => {
                    self.feed = JSON.parse(message.response);

                    self.http.get("/feeds/" + self.feed.Name + "/api/v2/Packages(Id='" + packageId + "',Version='" + packageVersion + "')").then(message => {
                        var pkg = JSON.parse(message.response).d;
                        pkg.Tags = pkg.Tags ? pkg.Tags.replace(/^\s+|\s+$/g, '').split(" ") : new Array();
                        pkg.Owners = pkg.Owners ? pkg.Owners.replace(/^\s+|\s+$/g, '').split(",") : new Array();
                        pkg.Authors = pkg.Authors ? pkg.Authors.replace(/^\s+|\s+$/g, '').split(",") : new Array();
                        pkg.DownloadUrl = "/feeds/" + self.feed.Name + "/packages/" + pkg.Id + "/" + pkg.NormalizedVersion;
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

                    self.http.get("/feeds/" + self.feed.Name + "/api/v2/FindPackagesById()?$top=100&id='" + packageId + "'").then(message => {
                        self.versionsOfPackage = JSON.parse(message.response).d.results;
                    });
                },
                function(message) {
                    if (message.statusCode === 401) {
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    }
                });
        }
    }

    packageHistoryVersionClick(pkg) {
        var self = this;
        if (self.package.NormalizedVersion === pkg.NormalizedVersion) {
            return;
        }

        self.isLoadingPackage = true;

        self.router.navigate("feeds/view/" + self.feed.Id + "/package/" + pkg.Id + "/" + pkg.Version);
    }

    attached() {

    }
}