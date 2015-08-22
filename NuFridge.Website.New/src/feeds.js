import {inject} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/custom.css!';
import {Router} from 'aurelia-router';
import {AuthService} from 'aurelia-auth';

@inject(Router, AuthService)
export class Feeds {
    hello = 'Welcome to Aurelia!';
    feedGroups = new Array();
    addFeedGroup(e){
        this.router.navigate("feedgroup/create");
    }

    constructor(router, auth) {
        this.router = router;
        this.auth = auth;
    }

    feedClick(feed) {
        this.router.navigate("feeds/view/" + feed.Id);
    }

    activate() {
        var self = this;

        $.ajax({
            url: "/api/feeds",
            cache: false,
            headers: {Authorization: 'Token ' + self.auth.auth.getToken()},
            dataType: 'json'
        }).then(function(response) {
            self.feedGroups = response.Results;
        }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
            if (xmlHttpRequest.status === 401) {

            }
        });
    }
}