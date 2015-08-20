export class App {
  configureRouter(config, router){
    config.title = 'NuFridge';
    config.map([
      { route: '', title: 'Dashboard', moduleId: 'home', nav: true },
      { route: 'feeds', title: 'Feeds', moduleId: 'feeds', nav: true},
      { route: 'feeds/view/:id', title: 'View Feed', moduleId: 'feedview', nav: false},
      { route: 'profile', title: 'Profile', moduleId: 'profile', nav: false },
      { route: 'feedgroup/view/:id', title: 'View Feed Group', moduleId: 'feedgroup' },
      { route: 'feedgroup/create', title: 'Create Feed Group', moduleId: 'feedgroup' },
      { route: 'settings', title: 'Settings', moduleId: 'settings', nav: true},
      { route: 'feeds/create', title: 'Create Feed', moduleId: 'addfeed', nav: false },
      { route: 'signin', title: 'Sign in', moduleId: 'registersignin', nav: false },
      { route: 'signout', title: 'Sign out', nav: true, moduleId:'signout', nav: false}
    ]);

    this.router = router;
  }
}
