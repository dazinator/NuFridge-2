import {AuthService} from 'aurelia-auth';
import {inject} from 'aurelia-framework';
import {notificationType} from 'notifications';
import {errorParser} from 'errorparser';

@inject(AuthService, errorParser)
export class Signin {

    constructor(auth, errorParser){
        this.auth = auth;
        this.errorParser = errorParser;
    };

    isSigningIn = false;

    shownotification = false;
    notificationmessage = "";
    notificationtype = notificationType.Info.value;

    username='';
    password='';

    login() {
        var self = this;

        self.shownotification = false;
        self.isSigningIn = true;

        return this.auth.login(this.username, this.password)
            .then(message => {

            })
            .catch(message => {
                self.isSigningIn = false;

                var parsedError = self.errorParser.parseResponse(message);
                self.notificationmessage =  parsedError.Message;

                if (parsedError.StackTrace) {
                    self.notificationmessage += "<br><br>Detailed Error:<br>" + parsedError.StackTrace;
                }

                self.shownotification = true;
                self.notificationtype = notificationType.Error.value;
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
    }

    attached() {
        // called when View is attached, you are safe to do DOM operations here
    }
}