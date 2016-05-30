define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    shell = require "shell"
    statusDialog = require "payments/player-bank-accounts/status-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @shell = shell
            @statusDialog = statusDialog
            @rowId = ko.observable()
            @gridId = "player-bank-account-verify-grid"            
            
            @hasViewPermission = ko.observable security.isOperationAllowed security.permissions.view, security.categories.playerBankAccount            
            @hasVerifyPermission = ko.observable security.isOperationAllowed security.permissions.verify, security.categories.playerBankAccount            
            @hasRejectPermission = ko.observable security.isOperationAllowed security.permissions.reject, security.categories.playerBankAccount

            @compositionComplete = =>
                $ =>
                    $(@gridId).on "gridLoad selectionChange", (e, row) =>
                        @rowId row.id

            @detached = =>
                @setEventHandlers off
                        
            @setEventHandlers on
            
        reloadGrid: =>
            $("#" + @gridId).trigger "reload"
                        
        setEventHandlers: (turnOn) =>
            events = ["player_bank_account_created", "player_bank_account_updated"]
            if turnOn
                $(document).on event, @reloadGrid for event in events
            else
                $(document).off event, @reloadGrid for event in events
                
        openViewTab: ->
            nav.open
                path: "payments/player-bank-accounts/edit"
                title: i18n.t "app:banks.viewAccount"
                data: 
                    id: @rowId()
                    isView: true
                    naming:
                        gridBodyId: @gridId
        
        openVerifyDialog: ->
            @statusDialog.show @, true
            
        openRejectDialog: ->
            @statusDialog.show @, false