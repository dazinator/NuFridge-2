import {customElement, bindable} from 'aurelia-framework';
import dotdotdot from 'dotdotdot';

@customElement('ellipsistext')
export class EllipsisText {

    @bindable contenttext;
    @bindable classname;

    constructor(){

    }

    bind(context){
        this.$parent = context;
    }

    attached() {
        var self = this;

        $("." + self.classname.replace( /(:|\.|\[|\])/g, "\\$1" )).dotdotdot({
            ellipsis	: '... ',
            wrap		: 'word',
            fallbackToLetter: true,
            after		: null,
            watch		: true,
            height		: 75,
            callback	: function(isTruncated, orgContent) {
                if (isTruncated) {
                
                }
            }
        });
    }
}