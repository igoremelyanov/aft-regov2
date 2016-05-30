(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  this.ResponsibleGambling = (function(superClass) {
    extend(ResponsibleGambling, superClass);

    function ResponsibleGambling(id) {
      this.id = id;
      this.submitResponsible = bind(this.submitResponsible, this);
      this.closeModal = bind(this.closeModal, this);
      this.logout = bind(this.logout, this);
      this.save = bind(this.save, this);
      ResponsibleGambling.__super__.constructor.apply(this, arguments);
      this.message = ko.observable('');
      this.submitted = ko.observable();
      this.isTimeOutEnabled = ko.observable(false);
      this.isTimeOutEnabled.subscribe((function(_this) {
        return function(value) {
          if (value) {
            return _this.isSelfExclusionEnabled(false);
          }
        };
      })(this));
      this.timeOut = ko.observable();
      this.timeOuts = ko.observable([
        {
          id: 0,
          name: '24 hrs'
        }, {
          id: 1,
          name: '1 week'
        }, {
          id: 2,
          name: '1 month'
        }, {
          id: 3,
          name: '6 weeks'
        }
      ]);
      this.isSelfExclusionEnabled = ko.observable(false);
      this.isSelfExclusionEnabled.subscribe((function(_this) {
        return function(value) {
          if (value) {
            return _this.isTimeOutEnabled(false);
          }
        };
      })(this));
      this.selfExclusion = ko.observable();
      this.selfExclusions = ko.observable([
        {
          id: 0,
          name: '6 months'
        }, {
          id: 1,
          name: '1 year'
        }, {
          id: 2,
          name: '5 years'
        }, {
          id: 3,
          name: 'permanent'
        }
      ]);
    }

    ResponsibleGambling.prototype.save = function() {
      var question;
      question = this.isSelfExclusionEnabled() ? "Are you sure you want to SelfExclude?" : "Are you sure you want to Time-Out?";
      this.message(question);
      return $('#responsiblegaming-alert').modal();
    };

    ResponsibleGambling.prototype.logout = function() {
      return $.postJson('/api/Logout').done((function(_this) {
        return function(response) {
          return redirect('/Home/Acknowledgement?id=' + _this.id());
        };
      })(this));
    };

    ResponsibleGambling.prototype.closeModal = function() {
      return $('#responsiblegaming-alert button.close').trigger('click');
    };

    ResponsibleGambling.prototype.submitResponsible = function() {
      if (this.isSelfExclusionEnabled()) {
        return this.submit("/api/SelfExclude", {
          PlayerId: this.id(),
          Option: this.selfExclusion()
        }, (function(_this) {
          return function() {
            _this.editing(false);
            return _this.logout();
          };
        })(this));
      } else if (this.isTimeOutEnabled()) {
        return this.submit("/api/TimeOut", {
          PlayerId: this.id(),
          Option: this.timeOut()
        }, (function(_this) {
          return function() {
            _this.editing(false);
            return _this.logout();
          };
        })(this));
      }
    };

    return ResponsibleGambling;

  })(FormBase);

}).call(this);

//# sourceMappingURL=responsible-gambling.js.map
