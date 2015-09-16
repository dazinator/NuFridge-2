import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';
import {AuthService} from 'paulvanbladel/aurelia-auth';
import {authUser} from './authuser';

@inject(Router, HttpClient, AuthService, authUser)
export class Users {

    pageNumber = 1;
    pageSize = 10;
    totalPages = new Array();

    constructor(router, http, auth, authUser) {
        this.router = router;
        this.http = http;
        this.auth = auth;
        this.authUser = authUser;
    }

    activate() {
        var self = this;
        self.loadUsers();
    }

    loadUsers() {
        var self = this;
        self.http.get("/api/accounts?page=" + self.pageNumber + "&size=" + self.pageSize).then(message => {
            self.accountData = JSON.parse(message.response);
            self.totalPages =  new Array(Math.ceil(self.accountData.Total / self.pageSize));
        });
    }

    userClick(user) {
        var self = this;
        var route = self.router.generate("userview", {id: user.Id});
        self.router.navigate(route);
    }

    attached() {

    }

    previousPageClick() {
        var self = this;

        if (self.pageNumber <= 1) {
            return;
        }

        self.pageNumber--;

        self.loadUsers();
    }

    goToPageClick(page) {
        var self = this;

        self.pageNumber = page;
        
        self.loadUsers();
    }

    nextPageClick() {
        var self = this;

        if (self.pageNumber >= self.totalPages.length) {
            return;
        }

        self.pageNumber++;

        self.loadUsers();
    }
}