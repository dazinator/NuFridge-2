﻿requirejs.config({
    paths: {
        'text': '../Scripts/text',
        'durandal': '../Scripts/durandal',
        'plugins': '../Scripts/durandal/plugins',
        'transitions': '../Scripts/durandal/transitions',
        'knockoutvalidation': '../Scripts/knockout.validation',
        'knockoutmapping': '../Scripts/knockout.mapping',
        'databinding-lucenefeed': 'viewmodels/databinding/lucenefeed',
        'databinding-lucenepackage': 'viewmodels/databinding/lucenepackage',
        'databinding-systeminfo': 'viewmodels/databinding/systeminfo',
        'databinding-user': 'viewmodels/databinding/user',
        'databinding-dashboard': 'viewmodels/databinding/dashboard',
        'readmore' :'../Scripts/readmore'
    }
});

define('jquery', function () { return jQuery; });
define('knockout', ko);

define(['durandal/system', 'durandal/app', 'durandal/viewLocator', 'knockoutmapping', 'knockoutvalidation'], function (system, app, viewLocator, komapping) {

    //>>excludeStart("build", true);
    system.debug(true);
    //>>excludeEnd("build");

    ko.mapping = komapping;

    ko.bindingHandlers.href = {
        update: function (element, valueAccessor) {
            ko.bindingHandlers.attr.update(element, function () {
                return { href: valueAccessor() };
            });
        }
    };

    ko.bindingHandlers.src = {
        update: function (element, valueAccessor) {
            ko.bindingHandlers.attr.update(element, function () {
                return { src: valueAccessor() };
            });
        }
    };

    ko.bindingHandlers.returnAction = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = ko.utils.unwrapObservable(valueAccessor());

            $(element).keydown(function (e) {
                if (e.which === 13) {
                    value(viewModel);
                }
            });
        }
    };

    ko.subscribable.fn.subscribeChanged = function (callback) {
        var oldValue;
        this.subscribe(function (_oldValue) {
            oldValue = _oldValue;
        }, this, 'beforeChange');

        this.subscribe(function (newValue) {
            callback(newValue, oldValue);
        });
    };

    ko.validation.init({
        insertMessages: false
    });

    app.title = 'NuFridge';

    app.configurePlugins({
        router: true,
        dialog: true
    });

    app.start().then(function() {
        viewLocator.useConvention();
        app.setRoot('viewmodels/shell', 'entrance');
    });
});