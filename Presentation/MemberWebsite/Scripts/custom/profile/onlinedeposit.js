(function() {
  var OnlineDeposit, viewModel,
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  OnlineDeposit = (function(superClass) {
    extend(OnlineDeposit, superClass);

    function OnlineDeposit() {
      OnlineDeposit.__super__.constructor.apply(this, arguments);
      this.amount = ko.observable('test').extend({
        validatable: true
      });
      this.code = ko.observable('')({
        validatable: true
      });
      this.hasErrors = ko.observable(false);
      this.setAmount = function() {};
      this.submitWithdrawalDetail = function() {
        return $.postJson('/api/Withdrawal', {
          amount: this.amount,
          sms: this.sms,
          email: this.email
        }).success((function(_this) {
          return function(response) {
            if (response.hasError) {
              return $.each(response.errors, function(propName) {
                var observable;
                observable = _this[propName];
                observable.error = 'test';
                return observable.__valid__(false);
              });
            } else {
              return popupAlert('', '');
            }
          };
        })(this));
      };
      this.submitOnlineDeposit = function() {
        return alert("dede");
      };
    }

    return OnlineDeposit;

  })(FormBase);

  viewModel = new OnlineDeposit();

  viewModel.load();

  ko.applyBindings(viewModel, document.getElementById("cashier-wrapper"));

  alert("eddede");

}).call(this);

//# sourceMappingURL=onlinedeposit.js.map
