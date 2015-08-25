import {bindable } from 'aurelia-framework';
import {inject} from 'aurelia-framework';
import {computedFrom} from 'aurelia-framework';
import {AuthService} from 'aurelia-auth';
@inject(AuthService)
export class NavBar {
    _isAuthenticated = false;
  @bindable router = null;

    profile = null;

    constructor(auth) {
        this.auth = auth;
    }

    signOut() {
        this.auth.logout("#/signin")
            .then(response => {
                console.log("ok logged out on  logout.js");
            })
            .catch(err => {
                console.log("error logged out  logout.js");

            });
    }

    activate() {

    }

    attached() {
        $("#settingsMenuItem").dropdown();
        var self = this;

        if (self.isAuthenticated) {
            self.auth.getMe().then(function(profile) {
                self.profile = profile;
            });
        }
    }

    get isAuthenticated() {
        return this.auth.isAuthenticated();
    }
}