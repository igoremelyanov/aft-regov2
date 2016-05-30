define (reguire) ->
    dialog = require "plugins/dialog"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    activateTemplateModel = require "messaging/message-templates/models/activate-template-model"
    
    class ActivateDialog extends baseModel
        constructor: (id, remarks) ->
            super
            @SavePath = "/MessageTemplate/Activate"
            @Model = new activateTemplateModel()
            @Model.id id
            @Model.remarks remarks
            @Model.remarks.isModified false

        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:messageTemplates.validation." + error

        onsave: ->
            $(document).trigger "message_template_changed"
            dialog.close @

        cancel: ->
            dialog.close @

        clear: ->
            @Model.remarks ""
            @Model.remarks.isModified false

        show: ->
            dialog.show @