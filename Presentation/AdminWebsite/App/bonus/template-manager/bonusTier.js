﻿(function() {
  var slice = [].slice;

  define(function() {
    var BonusTier;
    return BonusTier = (function() {
      function BonusTier() {
        var args;
        args = 1 <= arguments.length ? slice.call(arguments, 0) : [];
        this.From = ko.observable(0);
        this.Reward = ko.observable(0);
        this.MaxAmount = ko.observable(0);
        this.To = ko.observable(null);
        this.NotificationPercentThreshold = ko.observable(0);
        if (args.length === 1) {
          this.From(args[0].From);
          this.Reward(args[0].Reward);
          this.MaxAmount(args[0].MaxAmount);
          this.To(args[0].To);
          this.NotificationPercentThreshold(args[0].NotificationPercentThreshold);
        }
        this.vFrom = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.From() === 0) {
                return '';
              } else {
                return _this.From();
              }
            };
          })(this),
          write: this.From
        });
        this.vTo = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.To() === null) {
                return '';
              } else {
                return _this.To();
              }
            };
          })(this),
          write: this.To
        });
        this.vReward = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.Reward() === 0) {
                return '';
              } else {
                return _this.Reward();
              }
            };
          })(this),
          write: this.Reward
        });
        this.vMaxAmount = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.MaxAmount() === 0) {
                return '';
              } else {
                return _this.MaxAmount();
              }
            };
          })(this),
          write: this.MaxAmount
        });
        this.vNotificationPercentThreshold = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.NotificationPercentThreshold() === 0) {
                return '';
              } else {
                return _this.NotificationPercentThreshold();
              }
            };
          })(this),
          write: this.NotificationPercentThreshold
        });
      }

      return BonusTier;

    })();
  });

}).call(this);
