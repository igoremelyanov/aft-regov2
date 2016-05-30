(function() {
  ko.observables = function(self, variablesWithIniailValues) {
    var i, j, results;
    if (variablesWithIniailValues != null) {
      return $.extend(this, ko.mapping.fromJS(variablesWithIniailValues));
    } else {
      results = [];
      for (i = j = 0; j < 100; i = ++j) {
        results.push(ko.observable());
      }
      return results;
    }
  };

  ko.observableArrays = function() {
    var i, j, results;
    results = [];
    for (i = j = 0; j < 100; i = ++j) {
      results.push(ko.observableArray());
    }
    return results;
  };

}).call(this);
