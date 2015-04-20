define(['databinding/LuceneFeed'], function () {
    var ctor = function () {
        this.displayName = 'Feeds';
        this.feeds = ko.observableArray();
    };

    ctor.prototype.activate = function () {
        var self = this;

        $.ajax({
            url: "/api/Feeds",
            cache: false,
            dataType: 'json'
        }).then(function (response) {
            ko.mapping.fromJS(response, LuceneFeed.mapping, self.feeds);
        }).fail(function (response) {
            alert("Errors are not handled yet.");
        });
    }

    return ctor;
});