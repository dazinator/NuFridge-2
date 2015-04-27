define(['plugins/router', 'databinding-lucenefeed'], function (router, luceneFeed) {
    var ctor = function () {
        this.displayName = 'Create Feed';
        this.feed = ko.observable(luceneFeed());
    };

    ctor.prototype.activate = function() {

    };

    ctor.prototype.compositionComplete = function() {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);
    };

    ctor.prototype.createClick = function() {
        var feed = this;

        $("#addFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds",
            type: 'POST',
            data: ko.toJS(feed),
            dataType: 'json',
            cache: false,
            success: function(result) {
                $("#addFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully created.', 7500);
            },
            error: function(xmlHttpRequest, textStatus, errorThrown) {
                $("#addFeedModal").closeModal();
                Materialize.toast(errorThrown, 7500);
            }
        });
    };

    return ctor;
});