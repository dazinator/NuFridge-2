define(function() {
    var ctor = function () {
        this.displayName = 'Welcome!';
    };

    ctor.prototype.activate = function () {

    }
    ctor.prototype.compositionComplete = function () {
        $("#progressBar").attr("aria-busy", false);
    }

    return ctor;
});