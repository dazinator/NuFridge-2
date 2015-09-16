export class authUser{

	constructor() {

	}

    data = null;

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

    SetDisplayName(displayName) {
        var self = this;
        self.data.DisplayName = displayName;
    }

    set(profile) {
        var self = this;
        self.data = profile;
    }
}