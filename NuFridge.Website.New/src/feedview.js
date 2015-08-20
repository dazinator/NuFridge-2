import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/timeline.css!';
import '/styles/custom.css!';

export class Feeds {
  hello = 'Welcome to Aurelia!';

  constructor(){

  }

  activate() {
    // called when the VM is activated
  }

  attached() {
    $(".feedMenu .item").tab();
  }
}
