define(['plugins/router', 'auth', 'databinding-schedulejob', 'moment'], function (router, auth, schedulejob, moment) {
    var ctor = function () {
        this.succeededPageCount = ko.observable(1);
        this.succeededTotalCount = ko.observable(0);
        this.succeededCurrentPage = ko.observable(0);
        this.succeededJobs = ko.observableArray();

        this.failedPageCount = ko.observable(1);
        this.failedTotalCount = ko.observable(0);
        this.failedCurrentPage = ko.observable(0);
        this.failedJobs = ko.observableArray();

        this.processingPageCount = ko.observable(1);
        this.processingTotalCount = ko.observable(0);
        this.processingCurrentPage = ko.observable(0);
        this.processingJobs = ko.observableArray();

        this.scheduledPageCount = ko.observable(1);
        this.scheduledTotalCount = ko.observable(0);
        this.scheduledCurrentPage = ko.observable(0);
        this.scheduledJobs = ko.observableArray();

        
    };

    ctor.prototype.formatDate = function(value) {
        return moment(value).format('DD/MM/YYYY HH:mm:ss');
    };

    ctor.prototype.compositionComplete = function () {
        router.trigger("router:navigation:viewLoaded", router.activeInstruction(), router);

        $(".schedulerMenu .item").tab();
    };

    ctor.prototype.loadFailedJobs = function (pageNumber) {
        var self = this;


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
                create: function (options) {
                    return schedulejob(options.data);
                }
            };

            self.failedTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.failedJobs);


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.loadProcessingJobs = function (pageNumber) {
        var self = this;


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
                create: function (options) {
                    return schedulejob(options.data);
                }
            };

            self.processingTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.processingJobs);


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.loadScheduledJobs = function (pageNumber) {
        var self = this;


        if (!pageNumber) {
            pageNumber = 0;
        } else {
            if (pageNumber === self.scheduledCurrentPage()) {
                return;
            }
        }

        $.ajax({
            url: "/api/scheduler/jobs/scheduled" + "?page=" + pageNumber + "&pageSize=10",
            cache: false,
            headers: new auth().getAuthHttpHeader(),
            dataType: 'json'
        }).then(function (response) {

            var mapping = {
                create: function (options) {
                    return schedulejob(options.data);
                }
            };

            self.scheduledTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.scheduledJobs);


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.loadSucceededJobs = function (pageNumber) {
        var self = this;


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
                create: function (options) {
                    return schedulejob(options.data);
                }
            };

            self.succeededTotalCount(response.TotalCount);

            ko.mapping.fromJS(response.Results, mapping, self.succeededJobs);


        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {
                router.navigate("#signin");
            }
        });
    };

    ctor.prototype.activate = function () {
        var self = this;

        self.loadSucceededJobs();
        self.loadFailedJobs();
        self.loadProcessingJobs();
        self.loadScheduledJobs();
    };

    return ctor;
});