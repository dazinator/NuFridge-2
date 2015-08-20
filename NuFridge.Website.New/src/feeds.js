import {inject} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/custom.css!';
import {Router} from 'aurelia-router';


export class Feeds {
  hello = 'Welcome to Aurelia!';

  static inject() {
    return [Router];
  }

  addFeedGroup(e){
    this.router.navigate("feedgroup/create");
  }

  constructor(router) {
    this.router = router;
  }

  feedClick(e) {
    this.router.navigate("feeds/view/1");
  }

  activate() {
    // called when the VM is activated
  }

  attached() {
    // called when View is attached, you are safe to do DOM operations here
  }
}
