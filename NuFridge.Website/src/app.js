import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import AppRouterConfig from 'app.router.config';
import 'jquery';
import 'semanticui/semantic';
import {authUser} from './authuser';
import {AuthService, FetchConfig} from 'aurelia-auth';

@inject(Router,AppRouterConfig, FetchConfig, authUser, AuthService)
export class App {

    constructor(router, appRouterConfig, fetchConfig, authUser, auth){
        this.router = router;
        this.fetchConfig = fetchConfig;
        this.appRouterConfig = appRouterConfig;
        this.authUser = authUser;
        this.auth = auth;
    }

    activate() {
        var self = this;

        self.appRouterConfig.configure();
        this.fetchConfig.configure();

        return new Promise((resolve, reject) => {

            if (self.auth.isAuthenticated()) {
                self.auth.getMe().then(function(profile) {
                        self.authUser.set(profile);
                        resolve();
                    },
                    function(message) {
                        resolve();
                        var loginRoute = self.auth.auth.getLoginRoute();
                        self.auth.logout("#" + loginRoute);
                    });
            } else {
                resolve();
                var loginRoute = self.auth.auth.getLoginRoute();
                self.router.navigate("#" + loginRoute);
            }
        });
    }
}