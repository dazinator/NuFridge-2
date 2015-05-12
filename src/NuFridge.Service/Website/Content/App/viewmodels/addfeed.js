define(['plugins/router', 'databinding-lucenefeed'], function (router, luceneFeed) {
    var ctor = function () {
        var self = this;

        self.displayName = "Create Feed";
        self.feed = ko.validatedObservable(luceneFeed());
        self.thisWillCreateUrl = ko.observable("");
        self.setThisWillCreateUrlText();
        self.step = ko.observable(0);
    };

    ctor.prototype.activate = function () {
        var self = this;
        self.feed().name.subscribeChanged(function (newValue, oldValue) {
            var virtualDirectory = self.feed().virtualDirectory();
            if (!virtualDirectory || virtualDirectory === "/" || virtualDirectory === "/" + oldValue.toLowerCase()) {
                self.feed().virtualDirectory("/" + newValue.toLowerCase());
            }
        });

        self.feed().port.subscribe(function() {
            self.setThisWillCreateUrlText();
        });

        self.feed().host.subscribe(function () {
            self.setThisWillCreateUrlText();
        });

        self.feed().virtualDirectory.subscribe(function () {
            self.setThisWillCreateUrlText();
        });
    };

    ctor.prototype.cancelClick = function () {
        router.navigate('#feeds');
    };

    ctor.prototype.setThisWillCreateUrlText = function() {
        var self = this;

        var host = self.feed().host();
        var port = self.feed().port();
        var virtualDirectory = self.feed().virtualDirectory();

        if (port === "80") {
            port = "";
        } else {
            port = ":" + port;
        }

        self.thisWillCreateUrl("http://" + host + port + virtualDirectory);
    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $('#addFeedTabs').tabs();
    };

    ctor.prototype.nextClick = function () {
        var self = this;
        self.step(self.step() + 1);
    };

    ctor.prototype.previousClick = function () {
        var self = this;
        self.step(self.step() - 1);
    };

    ctor.prototype.createClick = function() {
        var self = this;

        if (!self.feed.isValid()) {
            return;
        }

        $("#addFeedModal").openModal({
            dismissible: false,
            opacity: .6
        });

        $.ajax({
            url: "/api/Feeds",
            type: 'POST',
            data: ko.toJS(self.feed()),
            dataType: 'json',
            cache: false,
            success: function(result) {
                $("#addFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + self.feed().name() + ' feed was successfully created.', 7500);
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                $("#addFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    };

    return ctor;
});