(function() {
  define(function(require) {
    var gameManagementEnabled;
    gameManagementEnabled = false;
    return {
      gameManagementEnabled: gameManagementEnabled,
      adminApiClientId: "local",
      adminApi: function(path) {
        if (path == null) {
          path = "";
        }
        return adminApiUrl + path;
      }
    };
  });

}).call(this);
