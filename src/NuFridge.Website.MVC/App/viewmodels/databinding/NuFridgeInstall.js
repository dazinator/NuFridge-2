define(['knockoutvalidation'], function (validation) {

    ko.validation.init({
        registerExtenders: true,
        insertMessages: false,
    });

    window.NuFridgeInstall = function (config) {
        var self = this           

        var obj = {
            SupportedDatabaseSystems: ko.observableArray(['Mongo', 'Sql Compact']).extend({ required: true }),
            DatabaseSystem: ko.observable('Mongo').extend({ required: true }),  
            MongoDBServer: ko.observable(""),
            MongoDBDatabase: ko.observable(""),
            IISWebsiteName: ko.observable("").extend({ required: true }),
            PortNumber: ko.observable("80").extend({
                required: true,
                pattern: { message: 'Port number must be numeric', params: /^\d+$/ }
            }),
            PhysicalDirectory: ko.observable("").extend({
                required: true,
                pattern: { message: 'Please provide a valid directory path', params: /^([A-Za-z]:)(\\[A-Za-z_\-\s0-9\.]+)+$/ }
            })
        }

        var data = $.extend(obj, config);
        ko.mapping.fromJS(data, {}, self);

        self.MongoDBServer.extend({
            required: {
                onlyIf: function () {
                    return self.DatabaseSystem() == "Mongo";
                }
            }
        });
       
        self.MongoDBDatabase.extend({
            required: {
                onlyIf: function () {
                    return self.DatabaseSystem() == "Mongo";
                }
            }
        });   
       
    };

    window.NuFridgeInstall.mapping = {
        create: function (options) {
            var fd = new NuFridgeInstall(options.data);
            return fd;
        }

    };
});