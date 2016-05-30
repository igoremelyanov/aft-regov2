(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, autoVerificationStatusDialog, i18n, jgu, nav, security;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
    jgu = require("JqGridUtil");
    security = require("security/security");
    autoVerificationStatusDialog = require("withdrawal/auto-verification-status-dialog");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
      }

      ViewModel.prototype.playerInfo = function(data, event) {
        var id;
        id = event.target.id;
        return nav.open({
          path: "player-manager/info",
          title: i18n.t("app:playerManager.list.playerInfo"),
          data: {
            playerId: id
          }
        });
      };

      ViewModel.prototype.verify = function() {
        return nav.open({
          path: "withdrawal/withdrawal-verify",
          title: "Verify",
          data: {
            id: this.rowId(),
            event: "verify"
          }
        });
      };

      ViewModel.prototype.cancel = function() {
        return nav.open({
          path: "withdrawal/withdrawal-verify",
          title: "Cancel",
          data: {
            id: this.rowId(),
            event: "cancel"
          }
        });
      };

      ViewModel.prototype.unverify = function() {
        return nav.open({
          path: "withdrawal/withdrawal-verify",
          title: "Unverify",
          data: {
            id: this.rowId(),
            event: "unverify"
          }
        });
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=on-hold-queue.js.map
