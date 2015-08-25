import {AuthService} from 'aurelia-auth';
import {inject} from 'aurelia-framework';
@inject(AuthService)
export class Signin {

    constructor(auth){
        this.auth = auth;
    };

    isSigningIn = false;

    username='';
    password='';

    login() {
        var self = this;

        self.isSigningIn = true;

        return this.auth.login(this.username, this.password)
        .then(message=> {

            })
        .catch(err=> {
                self.isSigningIn = false;
            });
    };

    authenticate(name){
        return this.auth.authenticate(name, false, null)
        .then((response)=>{
            console.log("auth response " + response);
        });
    }

    activate() {
        var self = this;

        var queryStringUsername = $.QueryString["username"];
        if (queryStringUsername) {
            self.username = queryStringUsername;
        }
    }

    attached() {
        // called when View is attached, you are safe to do DOM operations here
    }
}