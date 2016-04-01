import {customElement, bindable, inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';

@inject(Router)
@customElement('breadcrumb')
export class Breadcrumb {
    @bindable currentpage;
    @bindable previouspage;

    constructor(router){
        this.router = router;
    }

    bind(context){
        this.$parent = context;
    }

    getParams(item) {
        var self = this;

        var params = item.parameters;

        if (!params)
            return null;

        var obj = {};

        var arrayLength = params.length;
        for (var i = 0; i < arrayLength; i++) {
            obj[params[i]] = self.router.currentInstruction.params[params[i]];
        }

        return obj;
    }

    attached() {

    }
}