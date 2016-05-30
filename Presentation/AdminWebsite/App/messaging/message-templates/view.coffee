define (require) ->
    i18n = require "i18next"
    baseViewModel = require "base/base-view-model"
    viewTemplateModel = require "messaging/message-templates/models/view-template-model"
    aceEditor = require "ace-editor"

    class ViewModel extends baseViewModel
        constructor: ->
            super

        activate: (data) =>
            super
            $.get "/MessageTemplate/View?id=" + data.id
                .done (response) =>
                    @Model = new viewTemplateModel()
                    @Model.licenseeName response.licenseeName
                    @Model.brandName response.brandName
                    @Model.languageName response.languageName
                    @Model.messageType(i18n.t "messageTemplates.messageTypes." + response.messageType)
                    @Model.messageDeliveryMethod(i18n.t "messageTemplates.deliveryMethods." + response.messageDeliveryMethod)
                    @Model.templateName response.templateName
                    @Model.subject response.subject
                    @Model.messageContent response.messageContent
                    @subjectEditorId = "view-template-subject-" + data.id
                    @messageEditorId = "view-template-message-" + data.id
                    @isEmail = response.messageDeliveryMethod is "Email"

        compositionComplete: ->
            new aceEditor @subjectEditorId, @Model.subject, false, true
            new aceEditor @messageEditorId, @Model.messageContent, false, false