System.register([], function (_export) {
  "use strict";

  var _prototypeProperties, _classCallCheck, Welcome, UpperValueConverter;
  return {
    setters: [],
    execute: function () {
      _prototypeProperties = function (child, staticProps, instanceProps) { if (staticProps) Object.defineProperties(child, staticProps); if (instanceProps) Object.defineProperties(child.prototype, instanceProps); };

      _classCallCheck = function (instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } };

      Welcome = _export("Welcome", (function () {
        function Welcome() {
          _classCallCheck(this, Welcome);

          this.heading = "Feeds";
          this.firstName = "John";
          this.lastName = "Doe";
        }

        _prototypeProperties(Welcome, null, {
          fullName: {
            get: function () {
              return "" + this.firstName + " " + this.lastName;
            },
            configurable: true
          },
          welcome: {
            value: function welcome() {
              alert("Welcome, " + this.fullName + "!");
            },
            writable: true,
            configurable: true
          }
        });

        return Welcome;
      })());
      UpperValueConverter = _export("UpperValueConverter", (function () {
        function UpperValueConverter() {
          _classCallCheck(this, UpperValueConverter);
        }

        _prototypeProperties(UpperValueConverter, null, {
          toView: {
            value: function toView(value) {
              return value && value.toUpperCase();
            },
            writable: true,
            configurable: true
          }
        });

        return UpperValueConverter;
      })());
    }
  };
});
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImZlZWRzLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs2Q0FBYSxPQUFPLEVBZ0JQLG1CQUFtQjs7Ozs7Ozs7QUFoQm5CLGFBQU87QUFDUCxpQkFEQSxPQUFPO2dDQUFQLE9BQU87O0FBRWhCLGNBQUksQ0FBQyxPQUFPLEdBQUcsT0FBTyxDQUFDO0FBQ3ZCLGNBQUksQ0FBQyxTQUFTLEdBQUcsTUFBTSxDQUFDO0FBQ3hCLGNBQUksQ0FBQyxRQUFRLEdBQUcsS0FBSyxDQUFDO1NBQ3ZCOzs2QkFMVSxPQUFPO0FBT2Qsa0JBQVE7aUJBQUEsWUFBRTtBQUNaLDBCQUFVLElBQUksQ0FBQyxTQUFTLFNBQUksSUFBSSxDQUFDLFFBQVEsQ0FBRzthQUM3Qzs7O0FBRUQsaUJBQU87bUJBQUEsbUJBQUU7QUFDUCxtQkFBSyxlQUFhLElBQUksQ0FBQyxRQUFRLE9BQUksQ0FBQzthQUNyQzs7Ozs7O2VBYlUsT0FBTzs7QUFnQlAseUJBQW1CO2lCQUFuQixtQkFBbUI7Z0NBQW5CLG1CQUFtQjs7OzZCQUFuQixtQkFBbUI7QUFDOUIsZ0JBQU07bUJBQUEsZ0JBQUMsS0FBSyxFQUFDO0FBQ1gscUJBQU8sS0FBSyxJQUFJLEtBQUssQ0FBQyxXQUFXLEVBQUUsQ0FBQzthQUNyQzs7Ozs7O2VBSFUsbUJBQW1CIiwiZmlsZSI6ImZlZWRzLmpzIiwic291cmNlUm9vdCI6Ii9zcmMvIn0=