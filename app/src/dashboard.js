
export class Dashboard{

  constructor(cookies){

    this.username = '';
    this.password = '';
	
	     var i,x,y,ARRcookies=document.cookie.split(";");
     for (i=0;i<ARRcookies.length;i++)
     {
      x=ARRcookies[i].substr(0,ARRcookies[i].indexOf("="));
      y=ARRcookies[i].substr(ARRcookies[i].indexOf("=")+1);
      x=x.replace(/^\s+|\s+$/g,"");
      if (x=="accesstoken")
      {
       this.accesstoken = unescape(y);
      }
     }
	 
	 
	
	if (this.accesstoken)
	{
		this.loggedIn = true;
		    this.heading = 'Dashboard';
	}
	else
	{
		this.loggedIn = false;
		    this.heading = 'Login';
	}
  }



   createCookie(name, value, days) {
    var expires;
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }
    else {
        expires = "";
    }
    document.cookie = name + "=" + value + expires + "; path=/";
   }
   
  loginClick()  {
	  var self = this;
	  $.ajax({
    	type: "POST", url: "/token", data: "grant_type=password&username=" + self.username + "&password=" + self.password,
    	success: function (response) {
        	var token = response.access_token;
			self.createCookie("accesstoken", "Bearer " + token, 7);
			self.loggedIn = true;
			self.heading = "Dashboard";
    	},
		error: function(response) {
			alert(response.responseJSON.error_description);
			self.loggedIn = false;
		}
	});
  }

  welcome(){
    alert(`Welcome, ${this.fullName}!`);
  }
}

export class UpperValueConverter {
  toView(value){
    return value && value.toUpperCase();
  }
}
