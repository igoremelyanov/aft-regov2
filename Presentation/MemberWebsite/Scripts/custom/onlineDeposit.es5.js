'use strict';

(function () {
  var OnlineDeposit,
      bind = function bind(fn, me) {
    return function () {
      return fn.apply(me, arguments);
    };
  },
      extend = function extend(child, parent) {
    for (var key in parent) {
      if (hasProp.call(parent, key)) child[key] = parent[key];
    }function ctor() {
      this.constructor = child;
    }ctor.prototype = parent.prototype;child.prototype = new ctor();child.__super__ = parent.prototype;return child;
  },
      hasProp = ({}).hasOwnProperty;

  OnlineDeposit = (function (superClass) {
    var model;

    extend(OnlineDeposit, superClass);

    function OnlineDeposit() {
      this.setAmount = bind(this.setAmount, this);
      OnlineDeposit.__super__.constructor.apply(this, arguments);
      this.amount = ko.observable(0).extend({
        validatable: true
      });
      this.code = ko.observable('').extend({
        validatable: true
      });
      this.hasErrors = ko.observable(false);
      this.isEnabled = ko.observable(false);
      this.step2 = function () {
        var amount, checkAmount;
        amount = this.amount();
        if (amount === '') {
          this.isEnabled(false);
          this['amount'].__valid__(false);
          return false;
        }
        if (isNaN(amount)) {
          this['amount'].__valid__(false);
          return this.isEnabled(false);
        } else {
          checkAmount = parseInt(amount);
          if (isNaN(checkAmount)) {
            this['amount'].__valid__(false);
            this.isEnabled(false);
          }
          if (checkAmount < 200 || checkAmount > 50000) {
            this['amount'].__valid__(false);
            return this.isEnabled(false);
          } else {
            return this.isEnabled(true);
          }
        }
      };
      this.submitCode = function () {
        return $.postJson('/api/Code', ({
          amount: this.amount
        }).success((function (_this) {
          return function (response) {};
        })(this)), response.hasError ? $.each(response.errors, (function (_this) {
          return function (propName) {
            var observable;
            observable = _this[propName];
            observable.error = 'test';
            return observable.__valid__(false);
          };
        })(this)) : popupAlert('', ''));
      };
      this.submitOnlineDeposit = function () {
        alert("submit deposit");
        return $.postJson('/api/OnlineDeposit', ({
          amount: this.amount
        }).success((function (_this) {
          return function (response) {};
        })(this)), response.hasError ? $.each(response.errors, (function (_this) {
          return function (propName) {
            var observable;
            observable = _this[propName];
            observable.error = 'test';
            return observable.__valid__(false);
          };
        })(this)) : popupAlert('', ''));
      };
    }

    OnlineDeposit.prototype.setAmount = function (amount) {
      var addAmount;
      addAmount = amount.replace(',', '');
      this.amount(parseInt(addAmount) + this.amount());
      return this.step2();
    };

    model = new OnlineDeposit();

    model.errors = ko.validation.group(model);

    ko.applyBindings(model, document.getElementById("cashier-wrapper"));

    return OnlineDeposit;
  })(FormBase);
}).call(undefined);

//# sourceMappingURL=onlineDeposit.js.map

