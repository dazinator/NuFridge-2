define(function() {
    return function(config) {
        var self = {};

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

        var data = new function() {
            this.Title = ko.observable(getValue(config.properties.Title, ""));
            this.Created = ko.observable(getValue(config.properties.Created.text, ""));
            this.DownloadCount = ko.observable(getValue(parseInt(config.properties.DownloadCount.text), 0));
            this.Id = ko.observable(getValue(config.properties.Id, ""));
            this.Copyright = ko.observable(getValue(config.properties.Copyright, ""));
            this.Authors = ko.observable(getValue(config.properties.Authors, ""));
            this.Owners = ko.observable(getValue(config.properties.Owners, ""));
            this.LicenseUrl = ko.observable(getValue(config.properties.LicenseUrl, ""));
            this.LastUpdated = ko.observable(getValue(config.properties.LastUpdated.text, ""));
            this.ProjectUrl = ko.observable(getValue(config.properties.ProjectUrl, ""));
            this.RequireLicenseAcceptance = ko.observable(getValue(config.properties.RequireLicenseAcceptance.text, ""));
            this.Summary = ko.observable(getValue(config.properties.Summary, ""));
            this.Tags = ko.observable(getValue(config.properties.Tags, ""));
            this.Version = ko.observable(getValue(config.properties.Version, ""));
            this.PackageHash = ko.observable(getValue(config.properties.PackageHash, ""));
            this.IconUrl = ko.observable(getValue(config.properties.IconUrl, ""));
            this.IsLatestVersion = ko.observable(getValue(config.properties.IsLatestVersion.text, ""));
            this.IsAbsoluteLatestVersion = ko.observable(getValue(config.properties.IsAbsoluteLatestVersion.text, ""));
            this.Description = ko.observable(getValue(config.properties.Description, ""));
            this.IsPrerelease = ko.observable(getValue(config.properties.IsPrerelease, ""));
            this.OwnersArray = ko.computed(function () {
                var innerSelf = this;
                return innerSelf.Owners().split(",");
            }, this);
            this.GetDownloadLink = function(feedName) {
                var innerSelf = this;
                innerSelf.DownloadCount(innerSelf.DownloadCount() + 1);
                return window.location.origin + "/feeds/" + feedName + "/packages/" + innerSelf.Id() + "/" + innerSelf.Version();
            }
        }();


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});