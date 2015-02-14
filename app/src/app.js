import {Router} from 'aurelia-router';
import bootstrap from 'bootstrap';

export class App {
  static inject() { return [Router]; }
  constructor(router) {
    this.router = router;
    this.router.configure(config => {
      config.title = 'NuFridge';
      config.map([
        { route: ['','dashboard'],  moduleId: 'dashboard',      nav: true, title:'Dashboard' },
        { route: 'feeds',        moduleId: 'feeds',       nav: true, title: 'Feeds' },
	    { route: 'settings', moduleId: 'settings', nav: true, title: 'Settings'},
		{ route: 'install', moduleId: 'install', nav: false, title: 'Install NuFridge' }
      ]);
    });
  }
}