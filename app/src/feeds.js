export class Feeds{


  constructor(){
    this.heading = 'Feeds';
    this.feeds = [];
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