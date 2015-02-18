import {Router} from 'aurelia-router';

export class Feeds{
static inject() { return [Router]; }

  constructor(router){
    this.heading = 'Feeds';
    this.feeds = []; 
    this.routerInstance = router;
  }

editFeed(feed)
{
this.routerInstance.navigate("feeds/" + feed.Id + "/detail");
}

  activate(){
var self = this;


	return $.ajax({
    	type: "GET", url: "/api/feeds",
    	success: function (data) {
        	self.feeds = data;
    	}
	});
  }
}