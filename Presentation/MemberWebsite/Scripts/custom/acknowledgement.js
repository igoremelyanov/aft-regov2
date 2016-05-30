(function() {
  var Acknowledgement, model,
    bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  Acknowledgement = (function() {
    function Acknowledgement() {
      this.getMainText = bind(this.getMainText, this);
      this.self = this;
      this.getText = ko.observable('u<br/>hu');
    }

    Acknowledgement.prototype.getMainText = function(date) {
      return i18n.t('acknowledgment.main', {
        date: date
      });
    };

    return Acknowledgement;

  })();

  model = new Acknowledgement();

  ko.applyBindings(model, document.getElementById("acknowledgement-wrapper"));

}).call(this);

//# sourceMappingURL=acknowledgement.js.map
