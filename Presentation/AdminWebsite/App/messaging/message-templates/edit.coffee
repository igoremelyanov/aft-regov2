define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    editTemplateModel = require "messaging/message-templates/models/edit-template-model"
    aceEditor = require "ace-editor"

    class EditViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/MessageTemplate/Edit"
            @Model = new editTemplateModel()
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:messageTemplates.validation." + error

        onsave: ->
            $(document).trigger "message_template_changed"
            nav.close()
            nav.open
                path: "messaging/message-templates/view"
                title: i18n.t "app:common.view"
                data: 
                    id: @Model.id()
                
        activate: (data) =>
            super
            $.get "/MessageTemplate/Edit?id=" + data.id
                .done (response) =>
                    @Model.id data.id
                    @Model.licenseeName response.licenseeName
                    @Model.brandName response.brandName
                    @Model.languageName response.languageName
                    @Model.messageType response.messageType
                    @Model.messageDeliveryMethod response.messageDeliveryMethod
                    @Model.templateName response.templateName
                    @Model.subject response.subject
                    @Model.messageContent response.messageContent
                    @subjectEditorId = "edit-template-subject-" + data.id
                    @messageEditorId = "edit-template-message-" + data.id

        compositionComplete: ->
            new aceEditor @subjectEditorId, @Model.subject, true, true
            new aceEditor @messageEditorId, @Model.messageContent, true, false