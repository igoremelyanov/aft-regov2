(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  this.Bank = (function(superClass) {
    extend(Bank, superClass);

    function Bank(id) {
      this.id = id;
      Bank.__super__.constructor.apply(this, arguments);
      this.name = ko.observable('').extend({
        validatable: true
      });
      this.bank = ko.observable('').extend({
        validatable: true
      });
      this.account_number = ko.observable('').extend({
        validatable: true
      });
      this.branch = ko.observable('').extend({
        validatable: true
      });
      this.province = ko.observable('').extend({
        validatable: true
      });
      this.city = ko.observable('').extend({
        validatable: true
      });
      this.hasErrors = ko.observable(false);
      this.submitBankDetail = function() {
        return $.postJson('/api/Bank', {
          name: this.name,
          bank: this.bank,
          account_number: this.account_number,
          branch: this.branch,
          province: this.province,
          city: this.city
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
    }

    return Bank;

  })(FormBase);

}).call(this);

//# sourceMappingURL=bank.js.map
