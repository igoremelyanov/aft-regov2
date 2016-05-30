define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @hasSendPermission = ko.observable security.isOperationAllowed security.permissions.send, security.categories.massMessageTool

        openNewTab: ->
            nav.open
                path: "messaging/mass-message/wizard"
                title: i18n.t "app:messaging.massMessage.new"