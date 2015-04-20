define(['plugins/router', 'databinding/LuceneFeed'], function (router) {
    var ctor = function () {
        this.displayName = 'Edit Feed';
        this.feed = ko.observable(new LuceneFeed());
    };

    ctor.prototype.activate = function () {

        var self = this;

        if (router.activeInstruction().params.length == 1) {
            $.ajax({
                url: "/api/Feeds/" + router.activeInstruction().params[0],
                cache: false,
                dataType: 'json'
            }).then(function (response) {
                ko.mapping.fromJS(response, LuceneFeed.mapping, self.feed);
            }).fail(function (response) {
                alert("Errors are not handled yet.");
            });
        } else {
            alert("This scenario is not handled.");
        }
    }

    ctor.prototype.updateClick = function () {
        var feed = this;

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'PUT',
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