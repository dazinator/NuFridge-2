import {AuthService} from 'aurelia-auth';
import {inject} from 'aurelia-framework';
@inject(AuthService)
export class Signin {

    constructor(auth){
        this.auth = auth;
    };

    username='';
    password='';

    login(){
        return this.auth.login(this.username, this.password)
        .then(response=>{
            console.log("success logged " + response);
        })
        .catch(err=>{
            console.log("login failure");
        });
    };

    authenticate(name){
        return this.auth.authenticate(name, false, null)
        .then((response)=>{
            console.log("auth response " + response);
        });
    }

    activate() {
        // called when the VM is activated
    }

    attached() {
        // called when View is attached, you are safe to do DOM operations here
    }
}