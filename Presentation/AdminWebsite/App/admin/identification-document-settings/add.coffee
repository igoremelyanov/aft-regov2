define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    security = require "security/security"
    model = require "admin/identification-document-settings/model/model"
    config = require "config"

    class ViewModel
        constructor: ->
            @SavePath = config.adminApi("IdentificationDocumentSettings/CreateSetting")
            @message = ko.observable()
            @messageClass = ko.observable()
            @submitted = ko.observable()
            @Model = model
           
        showError: (msg) ->
             @message msg
             @messageClass 'alert alert-danger'
             
        showMessage: (msg) ->
            @message msg
            @messageClass 'alert alert-success'
            
        clearMessage: () ->
            @message ''
            @messageClass ''
        
        cancel: ->
           nav.close()
           @Model.clear()
        
        activate: () ->
            @clearMessage()
            @submitted off
            
        save: ->
            @clearMessage()
            
            if @Model.isValid()
                $.ajax
                    type: "POST"
                    url: @SavePath
                    data: ko.toJSON(@Model.getModelToSave())
                    contentType: "application/json"
                .done (response) =>
                    if (response.result == "success")
                        $('#identification-settings-grid').trigger 'reload'
                        @showMessage(i18N.t('app:admin.identificationDocumentSettings.createdSuccessfully'))
                        @submitted true
                    else
                        @showError(response.data)
            else
                @Model.errors.showAllMessages()
                
    new ViewModel()