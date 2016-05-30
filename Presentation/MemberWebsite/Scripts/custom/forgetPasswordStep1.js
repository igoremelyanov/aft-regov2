(function() {
  var ForgetPasswordModel,
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  ForgetPasswordModel = (function(superClass) {
    var model;

    extend(ForgetPasswordModel, superClass);

    function ForgetPasswordModel() {
      ForgetPasswordModel.__super__.constructor.apply(this, arguments);
      this.message = ko.observable();
      this.hasErrors = ko.observable(false);
      this.id = ko.observable('').extend({
        validatable: true
      });
      this.submitForgetPassword = function() {
        return $.postJson('/api/ValidateResetPasswordRequest', {
          Id: this.id()
        }).success((function(_this) {
          return function(response) {
            if (response.hasError) {
              return $.each(response.errors, function(propName) {
                var observable;
                observable = _this[propName];
                observable.error = i18n.t('resetPassword.' + response.errors[propName]);
                return observable.__valid__(false);
              });
            } else {
              $.ajax({
                url: '/api/ResetPassword',
                type: 'post',
                data: $('#forgetPasswordFormId').serialize()
              });
              $('#alert-modal').one('hidden.bs.modal', function() {
                return window.location.href = "/";
              });
              return popupAlert(i18n.t('resetPassword.checkYourEmail'), i18n.t('resetPassword.confirmStep1'));
            }
          };
        })(this));
      };
    }

    model = new ForgetPasswordModel;

    model.errors = ko.validation.group(model);

    ko.applyBindings(model, $("#forget-step1-wrapper")[0]);

    $("#forget-step1-wrapper").i18n();

    return ForgetPasswordModel;

  })(FormBase);

}).call(this);

//# sourceMappingURL=forgetPasswordStep1.js.map
