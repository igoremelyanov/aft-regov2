define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    config = require "config"
    
    class IdentitySettingsRemarksDialog
        constructor: (id, remarks) ->
            @remarks = ko.observable remarks
            .extend
                required: true
                
            @logId = ko.observable id
         
        cancel: ->
            dialog.close @
            
        show: (callback) ->
            @callback = callback
            dialog.show @