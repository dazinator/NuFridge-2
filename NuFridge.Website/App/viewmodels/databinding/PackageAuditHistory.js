﻿define(function () {
    return function (config) {
        var self = {};

        var data = $.extend({
            Downloads: ko.observableArray(),
            Uploads: ko.observableArray()
        }, config);


        ko.mapping.fromJS(data, {}, self);

        return data;
    };
});