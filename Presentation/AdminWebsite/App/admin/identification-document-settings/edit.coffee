define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    security = require "security/security"
    model = require "admin/identification-document-settings/model/model"
    config = require "config"

    class ViewModel
        constructor: ->
            @SavePath = config.adminApi("IdentificationDocumentSettings/UpdateSetting")
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
        
        activate: (data) ->
            @clearMessage()
            if (data.submitted)
                @submitted yes
            else
                @submitted no
                
            @Model.load(data.id)
            
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
                        @showMessage(i18N.t('app:admin.identificationDocumentSettings.updatedSuccessfully'))
                        @submitted true
                        $('#identification-settings-grid').trigger 'reload'
                    else
                        @showError(response.data)
            else
                @Model.errors.showAllMessages()
                
    new ViewModel()