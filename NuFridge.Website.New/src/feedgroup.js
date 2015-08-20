import {computedFrom} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/custom.css!';

export class Feedgroup {
  hello = 'Welcome to Aurelia!';

  constructor(){

  }

  activate() {
    // called when the VM is activated
  }

  attached() {
    $('.field .ui.dropdown')
      .dropdown({
        allowAdditions: false
      })
    ;
  }
}
