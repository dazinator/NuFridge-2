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
        this.auth.logout("#/signin")
            .then(response => {

            })
            .catch(err => {

            });
    }

    viewMyProfile() {
        var self = this;
        self.router.navigate("profile");
    }

    activate() {

    }

    attached() {
        $("#settingsMenuItem").dropdown();
        var self = this;

        if (self.isAuthenticated) {
            self.auth.getMe().then(function(profile) {
                self.authUser.set(profile);
            }, 
            function(message) {
                if (message.statusCode === 401) {
                    var loginRoute = self.auth.auth.getLoginRoute();
                    self.auth.logout(loginRoute);
                }
            });
        }
    }

    get isAuthenticated() {
        return this.auth.isAuthenticated();
    }
}