# CoffeeScript
define ['i18next', './changeTracker'], (i18N, ChangeTracker) ->
    class TemplateNotification
        constructor: (triggers) ->
            triggersArray = ({Id: trigger, Name: i18N.t "messageTemplates.messageTypes." + trigger} for trigger in triggers)
            @triggers = ko.observableArray(triggersArray)
            @EmailTriggers = ko.observableArray()
            @SmsTriggers = ko.observableArray()
            new ChangeTracker @
            ko.validation.group @