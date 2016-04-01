import {customElement, bindable, inject} from 'aurelia-framework';
import {EventAggregator} from 'aurelia-event-aggregator';
import {PaginationMessage} from './paginationmessage';

@customElement('pagination')
@inject(EventAggregator)
export class Pagination {

    @bindable pagesize = 10;
    @bindable totalresults = 0;

    pagenumber = 1;
    totalpages = new Array();

    constructor(eventAggregator){
        this.eventAggregator = eventAggregator;
    }

    totalresultsChanged() {
        var self = this;
        self.totalpages =  new Array(Math.ceil(self.totalresults / self.pagesize));
    }

    bind(context){
        this.$parent = context;
        this.loadInitialData();
    }

    loadInitialData() {
        var self = this;

        self.eventAggregator.publish(new PaginationMessage(self.pagenumber, self.pagesize));
    }

    previousPageClick() {
        var self = this;

        if (self.pagenumber <= 1) {
            return;
        }

        new Promise((resolve) => {
            self.eventAggregator.publish(new PaginationMessage(self.pagenumber - 1, self.pagesize, resolve));
        }).then(function() {
            self.pagenumber--;
        });
    }

    goToPageClick(page) {
        var self = this;

        new Promise((resolve) => {
            self.eventAggregator.publish(new PaginationMessage(page, self.pagesize, resolve));
        }).then(function() {
            self.pagenumber = page;
        });
    }

    nextPageClick() {
        var self = this;

        if (self.pagenumber >= self.totalpages.length) {
            return;
        }

        new Promise((resolve) => {
            self.eventAggregator.publish(new PaginationMessage(self.pagenumber + 1, self.pagesize, resolve));
        }).then(function() {
            self.pagenumber++;
        });
    }
}