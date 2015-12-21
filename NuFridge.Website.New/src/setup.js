import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import {HttpClient} from 'aurelia-http-client';

@inject(Router, HttpClient)
export class Setup {

    isSetup = false;
    isCreatingUser = false;

    request = {
        Username: "",
        EmailAddress: "",
        Password: "",
        ConfirmPassword: ""
    };

    constructor(router, http) {
        this.router = router;
        this.http = http;
    }

    activate() {
        var self = this;

        this.http.get("api/setup").then(message => {
            self.isSetup = JSON.parse(message.response);

            if (self.isSetup === true) {
                self.router.navigate("signin");
            }
        });
    }

    setupClick() {
        var self = this;

        $('form.segment.form').form("validate form");

        if ($('form.segment.form').form("is valid") === false) {
            return false;
        }

        self.isCreatingUser = true;

        this.http.post("api/setup", self.request).then(message => {
            if (message.statusCode === 201) {
                self.router.navigate("signin");
            } else {
                self.isCreatingUser = false;
            }
        });
    }

    attached()
    {
        $('form.segment.form')
            .form({
                inline: true,
                on: 'blur',
                fields: {
                    username: {
                        identifier: 'username',
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Please enter a username'
                            },
                            {
                                type: 'minLength[3]',
                                prompt: 'Your username must be at least 3 characters long'
                            }
                        ]
                    },
                    email: {
                        identifier: 'email',
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Please enter an email address'
                            },
                            {
                                type: 'email',
                                prompt: 'Please enter a valid email address'
                            }
                        ]
                    },
                    password: {
                        identifier  : 'password',
                        rules: [
                            {
                                type: 'minLength[8]',
                                prompt: 'Your password must be at least 8 characters long'
                            }
                        ]
                    },
                    confirmPassword: {
                        identifier  : 'confirmPassword',
                        rules: [
                          {
                              type   : 'match[password]',
                              prompt : 'The passwords do not match'
                          }
                        ]
                    }
                }
            });
    }
}