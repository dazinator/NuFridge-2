import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser)
export class Users {

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

        self.http.get("/api/accounts?page=" + self.pageNumber + "&size=" + self.pageSize).then(message => {
            self.accountData = JSON.parse(message.response);
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