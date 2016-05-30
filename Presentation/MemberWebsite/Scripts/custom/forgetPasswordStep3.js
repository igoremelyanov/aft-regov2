(function() {
  var ResetPasswordLastModel, viewModel,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  ResetPasswordLastModel = (function(_super) {
    __extends(ResetPasswordLastModel, _super);

    function ResetPasswordLastModel() {
      ResetPasswordLastModel.__super__.constructor.apply(this, arguments);
      $("#page").i18n();
      this.playerId = ko.observable();
      this.newPassword = ko.observable();
      this.confirmPassword = ko.observable().extend({
        validatable: true
      });
      this.submitResetPassword = (function(_this) {
        return function() {
          $.postJson('/api/ValidateConfirmResetPasswordRequest', {
            NewPassword: _this.newPassword(),
            ConfirmPassword: _this.confirmPassword(),
            PlayerId: _this.playerId()
          }).success(function(response) {
            if (response.hasError) {
              return $.each(response.errors, function(propName) {
                var observable;
                observable = _this[propName];
                observable.error = i18n.t('resetPassword.' + response.errors[propName]);
                return observable.__valid__(false);
              });
            } else {
              return $.ajax({
                url: '/api/ConfirmResetPasswordRequest',
                type: 'post',
                data: $('#confirm-reset-password-form').serialize()
              }).success(function(reponse) {
                localStorage.setItem("reset", "success");
                return redirect('/');
              });
            }
          });
          return false;
        };
      })(this);
      (function(_this) {
        return (function() {});
      })(this)();
      this.playerId($("[name=playerId]").val());
    }

    return ResetPasswordLastModel;

  })(FormBase);

  viewModel = new ResetPasswordLastModel();

  ko.applyBindings(viewModel, document.getElementById("forget-step3-wrapper"));

  $("#forget-step3-wrapper").i18n();

}).call(this);

//# sourceMappingURL=forgetPasswordStep3.js.map
