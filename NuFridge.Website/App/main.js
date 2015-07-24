requirejs.config({
    paths: {
        'text': '../Scripts/text',
        'durandal': '../Scripts/durandal',
        'plugins': '../Scripts/durandal/plugins',
        'transitions': '../Scripts/durandal/transitions',
        'xml2json': '../Scripts/xml2json',
        'knockoutvalidation': '../Scripts/knockout.validation',
        'knockoutmapping': '../Scripts/knockout.mapping',
        'databinding-feed': 'viewmodels/databinding/feed',
        'databinding-feedconfig': 'viewmodels/databinding/feedconfig',
        'databinding-package': 'viewmodels/databinding/package',
        'databinding-systeminfo': 'viewmodels/databinding/systeminfo',
        'databinding-user': 'viewmodels/databinding/user',
        'databinding-signinrequest': 'viewmodels/databinding/signinrequest',
        'databinding-dashboard': 'viewmodels/databinding/dashboard',
        'databinding-feedpackagecountstatistic': 'viewmodels/databinding/feedpackagecountstatistic',
        'databinding-feeddownloadcountstatistic': 'viewmodels/databinding/feeddownloadcountstatistic',
        'databinding-schedulejob': 'viewmodels/databinding/schedulejob',
        'databinding-schedulejobpaging': 'viewmodels/databinding/schedulejobpaging',
        'readmore': '../Scripts/readmore',
        'cookie': '../Scripts/jquery.cookie',
        'api': 'system/api',
        'chart': '../Scripts/Chart',
        'timeago': '../Scripts/timeago',
        'moment': '../Scripts/moment',
        'auth': 'system/auth'
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
            element.onerror = function () {
                element.src = 'Semantic/images/cube.png';
            };
            
            ko.bindingHandlers.attr.update(element, function () {
                return { src: valueAccessor()() + "?"  };
            });
        }
    };

    ko.bindingHandlers.placeholder = {
        update: function (element, valueAccessor, allBindingsAccessor) {
            var underlyingObservable = valueAccessor();
            ko.applyBindingsToNode(element, { attr: { placeholder: underlyingObservable } });
        }
    };

    ko.bindingHandlers.tooltip = {
        init: function (element, valueAccessor) {
            var value = valueAccessor() || {};

            $(element)
                .popup({
                    hoverable: true,
                    title: value
                });

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).popup('destroy');
            });
        }
    };

    ko.bindingHandlers.readmore = {
        init: function (element, valueAccessor) {
            var value = valueAccessor() || {};
            $(element).readmore(value);

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).readmore('destroy');
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

    ko.bindingHandlers.timeago = {
        update: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var $this = $(element);

            $this.attr('title', value);

            if ($this.data('timeago')) {
                var datetime = $.timeago.datetime($this);

                if (datetime instanceof Date && isFinite(datetime)) {
                    var distance = (new Date().getTime() - datetime.getTime());
                    var inWords = $.timeago.inWords(distance);

                    $this.data('timeago', { 'datetime': datetime });
                    $this.text(inWords);
                } else {
                    $this.text("Loading...");
                }
            } else {
                $this.timeago();
            }
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
        router: true
    });

    app.start().then(function() {
        viewLocator.useConvention();
        app.setRoot('viewmodels/shell', 'entrance');
    });
});