define(['plugins/router', 'databinding-feed', 'databinding-package', 'api', 'auth', 'databinding-feedconfig'], function (router, databindingFeed, databindingPackage, api, auth, databindingFeedConfig) {
    var ctor = function () {
        var self = this;

        self.feedconfig = ko.validatedObservable(databindingFeedConfig());
        self.feed = ko.validatedObservable(databindingFeed());
        self.showSuccessMessageOnLoad = ko.observable(false);

        self.isSaving = ko.observable(false);
        self.isCancelNavigating = ko.observable(false);

    };

    ctor.prototype.ReindexPackages = function() {
        var self = this;

        $.ajax({
            url: "/api/feeds/" + self.feed().Id() + "/reindex",
            type: 'POST',
            headers: new auth().getAuthHttpHeader(),
            cache: false,
            success: function (result) {

            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
            }
        });

    };


    ctor.prototype.activate = function () {

        var self = this;

        var feedDfd = jQuery.Deferred();
        var feedConfigDfd = jQuery.Deferred();

        if (router.activeInstruction().params.length >= 1) {



            $.ajax({
                url: api.get_feed + "/" + router.activeInstruction().params[0],
                cache: false,
                headers: new auth().getAuthHttpHeader(),
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return databindingFeed(options.data);
                    }
                };
     
                ko.mapping.fromJS(response, mapping, self.feed);
                feedDfd.resolve();
            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                feedDfd.reject();
                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
                //Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
            });

            $.ajax({
                url: api.get_feed + "/" + router.activeInstruction().params[0] + "/config",
                cache: false,
                headers: new auth().getAuthHttpHeader(),
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return databindingFeedConfig(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feedconfig);
                feedConfigDfd.resolve();
            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                feedConfigDfd.reject();
                if (xmlHttpRequest.status === 401) {
                    router.navigate("#signin");
                }
                //Materialize.toast(errorThrown ? errorThrown : "An unknown error has occurred.", 7500);
            });

        } else {
            alert("This scenario is not handled.");
        }

        self.feedOptions = {
            mode: "Update",
            feed: self.feed,
            feedconfig: self.feedconfig,
            showSuccessMessageOnLoad: self.showSuccessMessageOnLoad(),
            loaded: $.when(feedDfd.promise(), feedConfigDfd.promise())
        };

        if (router.activeInstruction().params.length >= 2) {
            var param = router.activeInstruction().params[1];
            if (param && param.ss && param.ss === "1") {
                self.showSuccessMessageOnLoad(true);
            }
        }
    };




    ctor.prototype.compositionComplete = function() {
        var self = this;

        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $(".feedMenu .item").tab();

        if (self.showSuccessMessageOnLoad() === true) {
            $(".feedMenu .item").removeClass("active");
            $(".ui.bottom.attached.tab.segment").removeClass("active");
            $(".settingTab").addClass("active");
        }
    };

    return ctor;
});