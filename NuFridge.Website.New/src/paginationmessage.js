export class PaginationMessage{
    constructor(pagenumber, pagesize, resolve) {
        this.pagenumber = pagenumber;
        this.pagesize = pagesize;
        this.resolve = resolve ? resolve : function() {};
    } 
}