import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';
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

    loadJobs(paginationMessage) {
        var self = this;
        self.http.get("/api/jobs?page=" + paginationMessage.pagenumber + "&size=" + paginationMessage.pagesize).then(message => {
            var data = JSON.parse(message.response);
            self.jobs = data.Jobs;
            self.totalResults = data.Total;
            paginationMessage.resolve();
        });
    }

    attached() {

    }
}