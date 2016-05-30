(function() {
  var Withdrawal, viewModel,
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  Withdrawal = (function(superClass) {
    extend(Withdrawal, superClass);

    function Withdrawal() {
      Withdrawal.__super__.constructor.apply(this, arguments);
      $("#page").i18n();
      this.amount = ko.observable().extend({
        validatable: true
      });
      this.notifySms = ko.observable(false);
      this.notifyEmail = ko.observable(false);
      this.isSuccessMessageVisible = ko.observable(false);
      this.submitForm = (function(_this) {
        return function() {
          var data, notificationType;
          notificationType = 0;
          if (_this.notifySms()) {
            notificationType = 1;
          }
          if (_this.notifyEmail()) {
            notificationType = 2;
          }
          if (_this.notifySms() && _this.notifyEmail()) {
            notificationType = 3;
          }
          data = {
            amount: _this.amount(),
            notificationType: notificationType
          };
          $.postJson('/api/ValidateWithdrawalRequest', data).success(function(response) {
            if (response.hasError) {
              return $.each(response.errors, function(propName) {
                var error, observable;
                observable = _this[propName];
                error = JSON.parse(response.errors[propName]);
                observable.error = i18n.t(error.text, error.variables);
                return observable.__valid__(false);
              });
            } else {
              return $.post('/api/OfflineWithdrawal', data).success(function(response) {
                return redirect('/home/withdrawal?isSuccess=true');
              });
            }
          });
          return false;
        };
      })(this);
    }

    return Withdrawal;

  })(FormBase);

  viewModel = new Withdrawal();

  ko.applyBindings(viewModel, document.getElementById("withdrawal-wrapper"));

}).call(this);

//# sourceMappingURL=withdrawal.js.map
