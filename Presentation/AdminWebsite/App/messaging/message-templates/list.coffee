define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    shell = require "shell"
    activateDialog = require "messaging/message-templates/activate-dialog"
    activateDialogX = require "controls/status-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super

            @shell = shell
            @rowId = ko.observable()
            @gridId = "#message-template-grid"            
            @canActivate = ko.observable false
            @remarks = ko.observable()            
            @hasAddPermission = ko.observable security.isOperationAllowed security.permissions.create, security.categories.messageTemplateManager            
            @hasEditPermission = ko.observable security.isOperationAllowed security.permissions.update, security.categories.messageTemplateManager            
            @hasActivatePermission = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.messageTemplateManager
            @hasViewPermission = ko.observable security.isOperationAllowed security.permissions.view, security.categories.messageTemplateManager

            @compositionComplete = =>
                $ =>
                    $(@gridId).on "gridLoad selectionChange", (e, row) =>
                        @rowId row.id
                        @canActivate row.data.Status == "Inactive"

            @onMessageTemplateChange = =>
                $(@gridId).trigger "reload"

            $(document).on "message_template_changed", @onMessageTemplateChange
            
            @detached = =>
                $(document).off "message_template_changed", @onMessageTemplateChange

        statusFormatter: -> i18n.t "common.statuses.#{@Status}"
        
        messageTypeFormatter: -> i18n.t "messageTemplates.messageTypes.#{@MessageType}"
        
        messageDeliveryMethodFormatter: -> i18n.t "messageTemplates.deliveryMethods.#{@MessageDeliveryMethod}"

        openAddTab: ->
            nav.open
                path: "messaging/message-templates/add"
                title: i18n.t "app:common.new"
                
        openViewTab: ->
            id = @rowId()
            if id?
                nav.open
                    path: "messaging/message-templates/view"
                    title: i18n.t "app:common.view"
                    data: 
                        id: id
                
        openEditTab: ->
            id = @rowId()
            if id?            
                nav.open
                    path: "messaging/message-templates/edit"
                    title: i18n.t "app:common.edit"
                    data: 
                        id: id
        
        openActivateDialog: ->
            id = @rowId()            
            if id and @canActivate()
                dialog = new activateDialog(id)
                dialog.show()
                null