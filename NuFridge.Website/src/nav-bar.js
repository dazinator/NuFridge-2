import {bindable } from 'aurelia-framework';
import {inject} from 'aurelia-framework';
import {computedFrom} from 'aurelia-framework';
import {AuthService} from 'aurelia-auth';
import {authUser} from './authuser';

@inject(AuthService, authUser)
export class NavBar {
    _isAuthenticated = false;
  @bindable router = null;

    constructor(auth, authUser) {
        this.auth = auth;
        this.authUser = authUser;
    }

    signOut() {
        var self = this;
        var loginRoute = self.auth.auth.getLoginRoute();
        this.auth.logout("#" + loginRoute)
            .then(response => {

            })
            .catch(err => {

            });
    }

    viewMyProfile() {
        var self = this;
        self.router.navigate("profile");
    }



    attached() {
      
    }

    get isAuthenticated() {
        return this.auth.isAuthenticated();
    }
}