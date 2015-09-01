import {computedFrom} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';

export class Feedgroup {

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
