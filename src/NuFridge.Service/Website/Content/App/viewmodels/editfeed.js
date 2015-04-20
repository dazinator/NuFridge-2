define(['plugins/router', 'databinding/LuceneFeed'], function (router, luceneFeed) {
    var ctor = function () {
        var self = this;

        this.feed = ko.observable(luceneFeed());
        this.displayName = ko.computed(function() {
            if (self.feed()) {
                return self.feed().name();
            }
            return '';
        });
    };

    ctor.prototype.activate = function () {

        var self = this;

        if (router.activeInstruction().params.length == 1) {
            $.ajax({
                url: "/api/Feeds/" + router.activeInstruction().params[0],
                cache: false,
                dataType: 'json'
            }).then(function (response) {

                var mapping = {
                    create: function (options) {
                        return luceneFeed(options.data);
                    }
                };

                ko.mapping.fromJS(response, mapping, self.feed);
                self.displayName('Edit ' + self.feed().name);
            }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
                router.navigate("#");
                Materialize.toast(errorThrown, 7500);
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
                Materialize.toast(errorThrown, 7500);
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
                Materialize.toast(errorThrown, 7500);
            }
        });
    }

    ctor.prototype.compositionComplete = function () {
        $('#viewFeedTabs').tabs();
    }

    return ctor;
});