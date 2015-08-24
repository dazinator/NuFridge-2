import {AuthorizeStep} from 'aurelia-auth';
import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';

@inject(Router)
export default class{

	constructor(router){
	    this.router = router;
}
    configure() {
        var appRouterConfig = function(config) {
            config.title = 'NuFridge';
            config.addPipelineStep('authorize', AuthorizeStep);

            config.map([
                { route: '', title: 'Dashboard', moduleId: 'home', nav: true, auth: true },
                { route: 'feeds', title: 'Feeds', moduleId: 'feeds', nav: true, auth: true },
                { route: 'feeds/view/:id', title: 'View Feed', moduleId: 'feedview', nav: false, auth: true },
                { route: 'profile', title: 'Profile', moduleId: 'profile', nav: false, auth: true },
                { route: 'feedgroup/view/:id', title: 'View Feed Group', moduleId: 'feedgroup', auth: true },
                { route: 'feedgroup/create', title: 'Create Feed Group', moduleId: 'feedgroup', auth: true },
                { route: 'feeds/create/:id', title: 'Create Feed', moduleId: 'feedcreate', nav: false, auth: true },
                { route: 'signin', title: 'Sign in', moduleId: 'signin', nav: false },
                { route: 'signout', title: 'Sign out', nav: true, moduleId: 'signout', nav: false, auth: true }
                //{ route: 'settings/database', title: 'Database Settings;', nav: false, moduleId: 'database', auth: true }
            ]);
        };

        this.router.configure(appRouterConfig);
    }

}