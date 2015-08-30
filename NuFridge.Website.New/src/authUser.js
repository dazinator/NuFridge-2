export class authUser{

	constructor() {

	}

    hasClaim() {
        var self = this;
        if (self.data && self.data.Claims) {
            for (var i = 0; i < arguments.length; i++) {
                if (self.data.Claims.indexOf(arguments[i].value) > -1)
                    return true;
            }
        }
        return false;
    }

    set(profile) {
        var self = this;
        self.data = profile;
    }
}