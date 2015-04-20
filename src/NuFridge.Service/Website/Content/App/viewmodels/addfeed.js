﻿define(['plugins/router', 'databinding/LuceneFeed'], function (router) {
    var ctor = function () {
        this.displayName = 'Create Feed';
        this.feed = ko.observable(new LuceneFeed());
    };

    ctor.prototype.activate = function () {


    }

    ctor.prototype.createClick = function () {
        var feed = this;

        $("#addFeedModal").openModal();

        $.ajax({
            url: "/api/Feeds",
            type: 'POST',
            data: feed,
            dataType: 'json',
            cache: false,
            success: function (result) {
                $("#addFeedModal").closeModal();
                router.navigate('#feeds');
                Materialize.toast('The ' + feed.name() + ' feed was successfully created.', 7500);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $("#addFeedModal").closeModal();
                alert('Errors are not handled yet.');
            }
        });
    }

    return ctor;
});