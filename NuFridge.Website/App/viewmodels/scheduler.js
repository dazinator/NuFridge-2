define(['plugins/router', 'auth', 'databinding-schedulejobpaging', 'moment', 'databinding-schedulejob', 'timeago'], function (router, auth, schedulejobpaging, moment, schedulejob, timeago) {
    var ctor = function () {
        this.succeededPageCount = ko.observable(1);
        this.succeededTotalCount = ko.observable(0);
        this.succeededCurrentPage = ko.observable(0);
        this.succeededJobs = ko.observableArray();

        this.deletedPageCount = ko.observable(1);
        this.deletedTotalCount = ko.observable(0);
        this.deletedCurrentPage = ko.observable(0);
        this.deletedJobs = ko.observableArray();

        this.failedPageCount = ko.observable(1);
        this.failedTotalCount = ko.observable(0);
        this.failedCurrentPage = ko.observable(0);
        this.failedJobs = ko.observableArray();

        this.processingPageCount = ko.observable(1);
        this.processingTotalCount = ko.observable(0);
        this.processingCurrentPage = ko.observable(0);
        this.processingJobs = ko.observableArray();

        this.scheduledJobs = ko.observableArray();

        this.ajaxInternal = null;
        this.ajaxUpdatedAt = ko.observable();
        this.ajaxFailed = false;
    };

    function zeropad(num) {
        return ((num < 10) ? '0' : '') + num;
    };

    function iso8601(value) {
        var date = value;

        if (typeof date === "string") {
            date = new Date(date);
        }

        return date.getUTCFullYear()
          + "-" + zeropad(date.getUTCMonth() + 1)
          + "-" + zeropad(date.getUTCDate())
          + "T" + zeropad(date.getUTCHours())
          + ":" + zeropad(date.getUTCMinutes())
          + ":" + zeropad(date.getUTCSeconds()) + "Z";
    };

    ctor.prototype.failedJobExpandClick = function(item, event) {
        $('.failedJobExpandSection-' + item.Key).toggle();
        $(event.target).toggleClass('add minus');
    };

    ctor.prototype.deletedJobExpandClick = function (item, event) {
        $('.deletedJobExpandSection-' + item.Key).toggle();
        $(event.target).toggleClass('add minus');
    };

    ctor.prototype.succeededJobExpandClick = function (item, event) {
        $('.succeededJobExpandSection-' + item.Key).toggle();
        $(event.target).toggleClass('add minus');
    };

    ctor.prototype.processingJobExpandClick = function (item, event) {
        $('.processingJobExpandSection-' + item.Key).toggle();
        $(event.target).toggleClass('add minus');
    };

    ctor.prototype.formatDate = function (value) {
        return moment(value).format('DD/MM/YYYY HH:mm:ss');
    };

    ctor.prototype.compositionComplete = function () {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $(".schedulerMenu .item").tab();
    };

    ctor.prototype.loadFailedJobs = function (pageNumber) {
        var self = this;

        var dfd = jQuery.Deferred();

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.failedCurrentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/scheduler/jobs/failed" + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Key);
                },
                create: function (options) {
                    return schedulejobpaging(options.data);
                }
            };

            self.failedTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.failedJobs);

            dfd.resolve();
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }

            dfd.reject();
        });

        return dfd.promise();
    };

    ctor.prototype.loadProcessingJobs = function (pageNumber) {
        var self = this;

        var dfd = jQuery.Deferred();

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.processingCurrentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/scheduler/jobs/processing" + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Key);
                },
                create: function (options) {
                    return schedulejobpaging(options.data);
                }
            };

            self.processingTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.processingJobs);

            dfd.resolve();
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }

            dfd.reject();
        });

        return dfd.promise();
    };

    ctor.prototype.loadScheduledJobs = function () {
        var self = this;

        var dfd = jQuery.Deferred();

        $.ajax({
            url: "/api/scheduler/jobs/scheduled",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                },
                create: function (options) {
                    return schedulejob(options.data);
                }
            };

            ko.mapping.fromJS(response, mapping, self.scheduledJobs);

            dfd.resolve();
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
            dfd.reject();
        });

        return dfd.promise();
    };

    ctor.prototype.loadDeletedJobs = function (pageNumber) {
        var self = this;

        var dfd = jQuery.Deferred();

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.deletedCurrentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/scheduler/jobs/deleted" + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Key);
                },
                create: function (options) {
                    return schedulejobpaging(options.data);
                }
            };

            self.deletedTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.deletedJobs);

            dfd.resolve();
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
            dfd.reject();
        });

        return dfd.promise();
    };

    ctor.prototype.loadSucceededJobs = function (pageNumber) {
        var self = this;

        var dfd = jQuery.Deferred();

        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.succeededCurrentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/scheduler/jobs/succeeded" + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Key);
                },
                create: function (options) {
                    return schedulejobpaging(options.data);
                }
            };

            self.succeededTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.succeededJobs);

            dfd.resolve();
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
            dfd.reject();
        });

        return dfd.promise();
    };

    ctor.prototype.deactivate = function() {
        var self = this;
        
        if (self.ajaxInterval) {
            clearInterval(self.ajaxInterval);
        }
    };

    ctor.prototype.loadData = function () {
        var self = this;

        var dfd = jQuery.Deferred();

        if (!self.ajaxFailed) {
            var d1 = self.loadSucceededJobs();
            var d2 = self.loadFailedJobs();
            var d3 = self.loadProcessingJobs();
            var d4 = self.loadScheduledJobs();
            var d5 = self.loadDeletedJobs();

            $.when(d1, d2, d3, d4, d5).then(function() {
                self.ajaxUpdatedAt(iso8601(new Date()));
                dfd.resolve();
            }, function(e) {
                self.ajaxFailed = true;
                dfd.reject();
            });
        } else {
            dfd.reject();
        }

        return dfd.promise();
    };

    ctor.prototype.activate = function() {
        var self = this;


        self.loadData().then(function() {
            self.ajaxInterval = setInterval(function () {
                self.loadData();
            }, 180000);
        });
    };

    return ctor;
});