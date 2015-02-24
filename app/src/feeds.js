import {Router} from 'aurelia-router';

export class Feeds{
static inject() { return [Router]; }

  constructor(router){
    this.heading = 'Feeds';
    this.feeds = []; 
    this.routerInstance = router;
  }
  
getCookie(c_name)
    {
     var i,x,y,ARRcookies=document.cookie.split(";");
     for (i=0;i<ARRcookies.length;i++)
     {
      x=ARRcookies[i].substr(0,ARRcookies[i].indexOf("="));
      y=ARRcookies[i].substr(ARRcookies[i].indexOf("=")+1);
      x=x.replace(/^\s+|\s+$/g,"");
      if (x==c_name)
      {
       return unescape(y);
      }
     }
    }

editFeed(feed)
{
this.routerInstance.navigate("feeds/" + feed.Id + "/detail");
}

  activate(){
var self = this;


	return $.ajax({
    	type: "GET", url: "/api/feeds",
		beforeSend: function (request)
            {
                request.setRequestHeader("Authorization", self.getCookie("accesstoken"));
            },
    	success: function (data) {
        	self.feeds = data;
    	}
	});
  }
}