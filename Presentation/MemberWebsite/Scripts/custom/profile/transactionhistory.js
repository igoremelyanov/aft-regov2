(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  this.TransactionHistory = (function(superClass) {
    extend(TransactionHistory, superClass);

    function TransactionHistory(id) {
      this.id = id;
      TransactionHistory.__super__.constructor.apply(this, arguments);
      this.startdate = ko.observable('').extend({
        validatable: true
      });
      this.enddate = ko.observable('').extend({
        validatable: true
      });
      this.hasErrors = ko.observable(false);
      this.submitFilter = function() {
        return $.postJson('/api/TransactionFilter', {
          game: this.amount,
          from: this.from,
          to: this.to
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

    return TransactionHistory;

  })(FormBase);

}).call(this);

//# sourceMappingURL=transactionhistory.js.map
