export class authUser{

    Username = "";
    Claims = new Array();

	constructor() {

	}

    hasClaim() {
        var self = this;
        for (var i = 0; i < arguments.length; i++) {
            if (self.Claims.indexOf(arguments[i].value) > -1)
                return true;
        }
        return false;
    }

    set(profile) {
        var self = this;
        self.Username = profile.Username;
        self.Claims = profile.Claims;
    }
}