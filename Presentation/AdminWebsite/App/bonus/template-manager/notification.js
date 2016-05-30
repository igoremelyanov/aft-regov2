﻿// Generated by IcedCoffeeScript 108.0.9
(function() {
  define(['i18next', './changeTracker'], function(i18N, ChangeTracker) {
    var TemplateNotification;
    return TemplateNotification = (function() {
      function TemplateNotification(triggers) {
        var trigger, triggersArray;
        triggersArray = (function() {
          var _i, _len, _results;
          _results = [];
          for (_i = 0, _len = triggers.length; _i < _len; _i++) {
            trigger = triggers[_i];
            _results.push({
              Id: trigger,
              Name: i18N.t("messageTemplates.messageTypes." + trigger)
            });
          }
          return _results;
        })();
        this.triggers = ko.observableArray(triggersArray);
        this.EmailTriggers = ko.observableArray();
        this.SmsTriggers = ko.observableArray();
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateNotification;

    })();
  });

}).call(this);