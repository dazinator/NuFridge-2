import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser)
export class Jobs {

    pageNumber = 1;
    pageSize = 10;

    constructor(router, http, auth, authUser) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
    }

    activate() {
        var self = this;

        self.http.get("/api/jobs?page=" + self.pageNumber + "&size=" + self.pageSize).then(message => {
            self.jobs = JSON.parse(message.response);
        });
    }

    attached() {

    }
}