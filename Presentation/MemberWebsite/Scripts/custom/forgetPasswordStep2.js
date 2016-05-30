(function() {
  var SecurityAnswerModel,
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  SecurityAnswerModel = (function(superClass) {
    var viewModel;

    extend(SecurityAnswerModel, superClass);

    function SecurityAnswerModel() {
      SecurityAnswerModel.__super__.constructor.apply(this, arguments);
      this.playerId = ko.observable();
      this.token = ko.observable();
      this.question = ko.observable("(loading...)");
      this.answer = ko.observable().extend({
        validatable: true
      });
      this.securityQuestion = (function(_this) {
        return function() {
          return $.getJson('/api/securityQuestions').done(function(response) {
            return console.log(response.securityQuestions);
          });
        };
      })(this);
      this.submitSecurityAnswerCheck = (function(_this) {
        return function() {
          $.postJson('/api/ValidateSecurityAnswerRequest', {
            PlayerId: _this.playerId(),
            Answer: _this.answer()
          }).success(function(response) {
            if (response.hasError) {
              return $.each(response.errors, function(propName) {
                var observable;
                observable = _this[propName];
                observable.error = i18n.t('resetPassword.' + response.errors[propName]);
                return observable.__valid__(false);
              });
            } else {
              $("#security-answer-form").submit();
              return redirect("/Home/ForgetPasswordStep3?token=" + _this.token());
            }
          });
          return false;
        };
      })(this);
      (function(_this) {
        return (function() {
          _this.playerId($("[name=playerId]").val());
          _this.token($("[name=token]").val());
          return $.get("/api/getSecurityQuestion", {
            PlayerId: _this.playerId()
          }, function(response) {
            return _this.question(response.securityQuestion);
          });
        });
      })(this)();
    }

    viewModel = new SecurityAnswerModel();

    ko.applyBindings(viewModel, document.getElementById("forget-step2-wrapper"));

    $("#forget-step2-wrapper").i18n();

    return SecurityAnswerModel;

  })(FormBase);

}).call(this);

//# sourceMappingURL=forgetPasswordStep2.js.map
