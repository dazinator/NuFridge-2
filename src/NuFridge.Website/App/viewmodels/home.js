define(function() {
    var ctor = function () {
        this.displayName = 'Welcome to NuFridge!';
    };

    ctor.prototype.activate = function () {

        $(".nav-wrapper ul li").each(function() {
            $(this).removeAttr("disabled");
        });
    }

    ctor.prototype.compositionComplete = function () {

    }

    return ctor;
});