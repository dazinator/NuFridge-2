define(function() {
    var ctor = function () {
        this.displayName = 'Welcome to NuFridge!';
        this.description = 'NuFridge is a package management server for NuGet which supports multiple feeds.';
    };

    //Note: This module exports a function. That means that you, the developer, can create multiple instances.
    //This pattern is also recognized by Durandal so that it can create instances on demand.
    //If you wish to create a singleton, you should export an object instead of a function.
    //See the "flickr" module for an example of object export.

    ctor.prototype.compositionComplete = function () {

        //$('.slider').slider({ full_width: true });
        //$('.tab-demo').show().tabs();
        //$('.parallax').parallax();
        //$('.modal-trigger').leanModal();
        //$('.scrollspy').scrollSpy();
        //$('.datepicker').pickadate({ selectYears: 20 });
        //$('select').not('.disabled').material_select();
    }

    return ctor;
});