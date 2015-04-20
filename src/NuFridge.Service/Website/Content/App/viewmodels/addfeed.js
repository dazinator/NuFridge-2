define(['plugins/router', 'databinding/LuceneFeed'], function (router) {
    var ctor = function () {
        this.displayName = 'Create Feed';
        this.feed = ko.observable(new LuceneFeed());
    };

    ctor.prototype.activate = function () {


    }

    ctor.prototype.createClick = function () {
        var feed = this;

        $.ajax({
            url: "/api/Feeds",
            type: 'POST',
            data: feed,
            dataType: 'json',
            cache: false,
            success: function (result) {
                router.navigate('#feeds');
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                alert('Errors are not handled yet.');
            }
        });
    }

    return ctor;
});