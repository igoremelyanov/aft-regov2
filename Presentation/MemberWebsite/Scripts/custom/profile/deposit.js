(function() {
  var Deposit,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  Deposit = (function(_super) {
    var model;

    __extends(Deposit, _super);

    function Deposit() {
      this.amount = ko.observable();
      this.bonuses = ko.observableArray();
      this.amount.subscribe((function(_this) {
        return function(value) {
          if (value) {
            return _this.checkForBonuses(value);
          }
        };
      })(this));
      this.checkForBonuses = (function(_this) {
        return function(value) {
          var data;
          data = {
            amount: value
          };
          return $.post('/api/qualifiedbonuses', data).done(function(response) {
            _this.bonuses([]);
            _this.bonuses(response);
            return console.log(response);
          });
        };
      })(this);
      this.selectBonus = (function(_this) {
        return function(data, event) {
          if (!$(event.currentTarget).hasClass('disable')) {
            if ($(event.currentTarget).hasClass('selected')) {
              return $(event.currentTarget).removeClass('selected');
            } else {
              $('#bonusList .col-sm-3').removeClass('selected');
              return $(event.currentTarget).addClass('selected');
            }
          }
        };
      })(this);
    }

    model = new Deposit();

    ko.applyBindings(model, $("#profile-wrapper")[0]);

    return Deposit;

  })(FormBase);

}).call(this);

//# sourceMappingURL=deposit.js.map
