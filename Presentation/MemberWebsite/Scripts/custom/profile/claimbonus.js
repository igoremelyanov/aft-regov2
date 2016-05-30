(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  this.ClaimBonus = (function(superClass) {
    extend(ClaimBonus, superClass);

    function ClaimBonus(id) {
      this.id = id;
      ClaimBonus.__super__.constructor.apply(this, arguments);
      this.code = ko.observable('').extend({
        validatable: true
      });
      this.hasErrors = ko.observable(false);
      this.submitBonusCode = function() {
        return $.postJson('/api/ClaimBonusCode', {
          code: this.code
        }.success((function(_this) {
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
        })(this)));
      };
    }

    return ClaimBonus;

  })(FormBase);

}).call(this);

//# sourceMappingURL=claimbonus.js.map
