define (reguire) ->
    dialog = require "plugins/dialog"
    
    class ConfirmDialog
        constructor: (onConfirmAction, text, onConfirmText, onFailedAction) ->
            @onConfirmAction = onConfirmAction
            @onFailedAction = onFailedAction
            @question = ko.observable text
            @onConfirmText = ko.observable onConfirmText
            @submitted = ko.observable no
            @message = ko.observable()
            @messageClass = ko.observable()

        showError: (msg) ->
             @message msg
             @messageClass 'alert alert-danger'
             
        showMessage: (msg) ->
            @message msg
            @messageClass 'alert alert-success'

        show : ->
            dialog.show @

        noAction: ->
            dialog.close @
            
        yesAction: =>
            successFunc = () =>
                @submitted yes
                @showMessage @onConfirmText()
                
            failFunc = (msg) =>
                @submitted yes
                @showError msg
            
            @onConfirmAction successFunc, failFunc