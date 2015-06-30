define(function() {
    return function(config) {
        var self = {}, data;

        function getValue(value, defaultValue) {
            if (value) {

                if (value["m:null"] != undefined) {
                    if (value["m:null"] === "false") {
                        return value.text;
                    }
                } else {
                    return value;
                }
            }
            return defaultValue;
        }

        data = $.extend({
            Title: ko.observable(getValue(config.properties.Title, "")),
            Created: ko.observable(getValue(config.properties.Created.text, "")),
            DownloadCount: ko.observable(getValue(config.properties.DownloadCount.text, "")),
            Id: ko.observable(getValue(config.properties.Id, "")),
            Copyright: ko.observable(getValue(config.properties.Copyright,  "")),
            Authors: ko.observable(getValue(config.properties.Authors, "")),
            LicenseUrl: ko.observable(getValue(config.properties.LicenseUrl, "")),
            LastUpdated: ko.observable(getValue(config.properties.LastUpdated.text, "")),
            ProjectUrl: ko.observable(getValue(config.properties.ProjectUrl, "")),
            RequireLicenseAcceptance: ko.observable(getValue(config.properties.RequireLicenseAcceptance.text, "")),
            Summary: ko.observable(getValue(config.properties.Summary, "")),
            Tags: ko.observable(getValue(config.properties.Tags, "")),
            Version: ko.observable(getValue(config.properties.Version, "")),
            PackageHash: ko.observable(getValue(config.properties.PackageHash, "")),
            IconUrl: ko.observable(getValue(config.properties.IconUrl, "")),
            IsLatestVersion: ko.observable(getValue(config.properties.IsLatestVersion.text, "")),
            IsAbsoluteLatestVersion: ko.observable(getValue(config.properties.IsAbsoluteLatestVersion.text, "")),
            Description: ko.observable(getValue(config.properties.Description, "")),
            IsPrerelease: ko.observable(getValue(config.properties.IsPrerelease, ""))
        }, {});


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});