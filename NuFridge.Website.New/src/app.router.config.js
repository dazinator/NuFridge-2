import {AuthorizeStep} from 'paulvanbladel/aurelia-auth';
import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';

@inject(Router)
export default class{

    constructor(router) {
        this.router = router;
    }
    configure() {
        var appRouterConfig = function(config) {
            config.title = 'NuFridge';
            config.addPipelineStep('authorize', AuthorizeStep);

            config.map([
                { route: '', title: 'Dashboard', moduleId: 'home', nav: true, auth: true },
                { route: 'feeds', name: 'feeds', title: 'Feeds', moduleId: 'feeds', nav: true, auth: true },
                {
                    route: 'feeds/view/:id',
                    title: 'View Feed',
                    moduleId: 'feedview',
                    name: 'feedview',
                    nav: false,
                    auth: true,
                    breadcrumb: [
                        {
                            routename: 'feeds',
                            title: 'Feeds'
                        }
                    ]
                },
                {
                    route: 'feeds/view/:id/package/:packageid/:packageversion',
                    title: 'View Package',
                    moduleId: 'package',
                    name: 'package',
                    nav: false,
                    auth: true,
                    breadcrumb: [
                        {
                            routename: 'feeds',
                            title: 'Feeds'
                        },
                        {
                            routename: 'feedview',
                            title: 'Feed',
                            parameters: ['id']
                        }
                    ]
                },
                {
                    route: 'feeds/view/:id/import/:jobid',
                    title: 'Importing Packages',
                    moduleId: 'importpackages',
                    name: 'importpackages',
                    nav: false,
                    auth: true,
                    breadcrumb: [
                        {
                            routename: 'feeds',
                            title: 'Feeds'
                        },
                        {
                            routename: 'feedview',
                            title: 'Feed',
                            parameters: ['id']
                        }
                    ]
                },
                {
                    route: 'feeds/view/:id/import',
                    title: 'Import Packages',
                    moduleId: 'importpackages',
                    nav: false,
                    auth: true,
                    breadcrumb: [
                        {
                            routename: 'feeds',
                            title: 'Feeds'
                        },
                        {
                            routename: 'feedview',
                            title: 'Feed',
                            parameters: ['id']
                        }
                    ]
                },
                { route: 'profile', title: 'Profile', moduleId: 'profile', nav: false, auth: true },
                { route: 'feedgroup/view/:id', title: 'View Feed Group', moduleId: 'feedgroup', name: 'feedgroup', auth: true, breadcrumb: [
                {
                    routename: 'feeds',
                    title: 'Feed Groups'
                }] },
                { route: 'feedgroup/create', title: 'Create Feed Group', moduleId: 'feedgroup', auth: true, breadcrumb: [
                    {
                        routename: 'feeds',
                        title: 'Feed Groups'
                    }
                ] },
                { route: 'feeds/create/:id', title: 'Create Feed', moduleId: 'feedcreate', nav: false, auth: true, breadcrumb: [
                {
                    routename: 'feeds',
                    title: 'Feed Groups'
                },
                {
                    routename: 'feedgroup',
                    title: 'Feed Group',
                    parameters: ['id']
                }]},
                { route: 'signin', title: 'Sign in', moduleId: 'signin', nav: false },
                { route: 'signout', title: 'Sign out', moduleId: 'signout', nav: false, auth: true },
                { route: 'setup', title: 'Setup', nav: false, moduleId: 'setup', auth: false },
                { route: 'profile', title: 'Profile', nav: false, moduleId: 'profile', auth: true, name: 'profile' },
                { route: 'users/view/:id', title: 'User', nav: false, moduleId: 'profile', auth: true, name: 'userview', breadcrumb: [
                {
                    routename: 'users',
                    title: 'Users'
                }] },
                { route: 'jobs', title: 'Jobs', nav: true, moduleId: 'jobs', auth: true },
                { route: 'users', title: 'Users', nav: true, moduleId: 'users', auth: true, name: 'users' }
            ]);
        };

        this.router.configure(appRouterConfig);
    }

}