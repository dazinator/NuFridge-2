import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
import {AuthService} from 'aurelia-auth';
import {authUser} from './authuser';
import {EventAggregator} from 'aurelia-event-aggregator';
import {PaginationMessage} from './paginationmessage';

@inject(Router, HttpClient, AuthService, authUser, EventAggregator)
export class Jobs {

    totalResults = 0;

    constructor(router, http, auth, authUser, eventAggregator) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
        this.eventAggregator = eventAggregator;
    }

    activate() {
        var self = this;

        this.eventAggregator.subscribe(PaginationMessage, paginationMessage => {
            self.loadJobs(paginationMessage);
        });
    }

    jobClick(job) {
        var self = this;
        var route = self.router.generate("importpackages", {id: job.FeedId, jobid: job.Id});
        self.router.navigate(route);
    }

    loadJobs(paginationMessage) {
        var self = this;
        self.http.fetch("api/jobs?page=" + paginationMessage.pagenumber + "&size=" + paginationMessage.pagesize).then(response => response.json()).then(message => {
            var data = message;
            self.jobs = data.Jobs;
            self.totalResults = data.Total;
            paginationMessage.resolve();
        });
    }

    attached() {

    }
}