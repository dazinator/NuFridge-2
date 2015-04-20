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

    ctor.prototype.deleteClick = function () {
        var feed = this;

        $("#deleteFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'DELETE',
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#deleteFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully deleted.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#deleteFeedModal").closeModal();
                alert('Errors are not handled yet.');
            }
        });
    }

    ctor.prototype.updateClick = function () {
        var feed = this;

        $("#editFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds/" + feed.id(),
            type: 'PUT',
            data: feed,
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#editFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully updated.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#editFeedModal").closeModal();
                alert('Errors are not handled yet.');
            }
        });
    }

    return ctor;
});