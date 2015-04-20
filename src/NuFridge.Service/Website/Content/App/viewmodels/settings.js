﻿define(function() {
    var ctor = function () {
        this.displayName = 'Settings';
    };

    ctor.prototype.compositionComplete = function () {
        $('#settingsTabs').tabs();
        $('.datepicker').pickadate({
            selectMonths: true, // Creates a dropdown to control month
            selectYears: 15 // Creates a dropdown of 15 years to control year
        });

    }

    return ctor;
});