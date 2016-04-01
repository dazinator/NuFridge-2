import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient, json} from 'aurelia-fetch-client';
import {AuthService} from 'aurelia-auth';
import {authUser} from './authuser';
import {EventAggregator} from 'aurelia-event-aggregator';
import {PaginationMessage} from './paginationmessage';

@inject(Router, HttpClient, AuthService, authUser, EventAggregator)
export class Users {

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

        self.eventAggregator.subscribe(PaginationMessage, paginationMessage => {
            self.loadUsers(paginationMessage);
        });
    }

    loadUsers(paginationMessage) {
        var self = this;
        self.http.fetch("api/accounts?page=" + paginationMessage.pagenumber + "&size=" + paginationMessage.pagesize).then(response => response.json()).then(message => {
            self.accountData = message;
            self.totalResults = self.accountData.Total;
            paginationMessage.resolve();
        });
    }

    userClick(user) {
        var self = this;
        var route = self.router.generate("userview", {id: user.Id});
        self.router.navigate(route);
    }

    attached() {

    }
}