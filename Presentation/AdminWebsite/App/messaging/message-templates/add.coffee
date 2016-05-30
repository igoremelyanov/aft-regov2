define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    addTemplateModel = require "messaging/message-templates/models/add-template-model"
    aceEditor = require "ace-editor"

    class AddViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/MessageTemplate/Add"
            @Model = new addTemplateModel()
            
        compositionComplete: ->
            new aceEditor "add-template-editor-subject", @Model.subject, true, true
            new aceEditor "add-template-editor-message", @Model.messageContent, true, false
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:messageTemplates.validation." + error

        onsave: (data) ->
            $(document).trigger "message_template_changed"
            nav.close()
            nav.open
                path: "messaging/message-templates/view"
                title: i18n.t "app:common.view"
                data: 
                    id: data.data