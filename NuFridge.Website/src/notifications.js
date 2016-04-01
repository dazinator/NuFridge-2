import {containerless, bindingMode, customElement, bindable, inject} from 'aurelia-framework';
import {Enum} from 'enum';

@containerless
export class notifications{
    
    @bindable({defaultBindingMode: bindingMode.twoWay, defaultValue: false}) notificationvisible = false;
    @bindable({defaultBindingMode: bindingMode.twoWay, defaultValue: ""}) notificationtext = "";
    @bindable({defaultBindingMode: bindingMode.twoWay, defaultValue: ""}) notificationtype = notificationType.Info.value;

    constructor() {

    }
}

export const notificationType = new Enum({
    Info: {value: ''},
    Warning: {value: 'warning'},
    Error: {value: 'error'}
});