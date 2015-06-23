define(function() {
    return function(config) {
        var self = {};

        var data = $.extend({
            Title: ko.observable(),
            Created: ko.observable(),
            DownloadCount: ko.observable(),
            Id: ko.observable(),
            Copyright: ko.observable(),
            Authors: ko.observableArray(),
            LicenseUrl: ko.observable(),
            LastUpdated: ko.observable(),
            ProjectUrl: ko.observable(),
            Path: ko.observable(),
            RequireLicenseAcceptance: ko.observable(),
            Summary: ko.observable(),
            Tags: ko.observable(),
            Version: ko.observable(),
            PackageHash: ko.observable(),
            PackageSize: ko.observable(),
            SearchTitle: ko.observable(),
            SymbolsAvailable: ko.observable(),
            IconUrl: ko.observable(""),
            IsLatestVersion: ko.observable(),
            Description: ko.observable(),
            Published: ko.observable(),
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});