import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import AppRouterConfig from 'app.router.config';
import HttpClientConfig from 'paulvanbladel/aurelia-auth/app.httpClient.config';
import 'jquery';
import 'semanticui/semantic';
import {authUser} from './authuser';
import {AuthService} from 'paulvanbladel/aurelia-auth';

@inject(Router,HttpClientConfig,AppRouterConfig, authUser, AuthService)
export class App {

    constructor(router, httpClientConfig, appRouterConfig, authUser, auth){
        this.router = router;
        this.httpClientConfig = httpClientConfig;
        this.appRouterConfig = appRouterConfig;
        this.authUser = authUser;
        this.auth = auth;
    }

    activate() {
        var self = this;

        self.httpClientConfig.configure();
        self.appRouterConfig.configure();

        return new Promise((resolve, reject) => {

            if (self.auth.isAuthenticated()) {
                self.auth.getMe().then(function(profile) {
                        self.authUser.set(profile);
                        resolve();
                    },
                    function(message) {
                        resolve();
                    });
            } else {
                resolve();
            }
        });
    }
}