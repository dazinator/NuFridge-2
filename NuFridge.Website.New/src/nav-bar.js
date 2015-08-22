import {bindable } from 'aurelia-framework';
import {inject} from 'aurelia-framework';
import {AuthService} from 'aurelia-auth';
@inject(AuthService)
export class NavBar {
    _isAuthenticated = false;
  @bindable router = null;

    constructor(auth) {
        this.auth = auth;

    }

    signOut() {
        this.auth.logout("#/login")
    .then(response=>{
        console.log("ok logged out on  logout.js");
    })
    .catch(err=>{
        console.log("error logged out  logout.js");

    });
    }

    get isAuthenticated() {
        return this.auth.isAuthenticated();
    }
}