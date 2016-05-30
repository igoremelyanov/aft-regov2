define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    config = require "config"
    
    class DialogViewModel
        constructor: (data) ->
            @data = data
            @applySanctions = ko.observable '0'
            @newSanction = ko.observable '0'
        update: ->
            @data.action = @applySanctions()
            @data.sanction = @newSanction()
            $.post '/signup/update', data: @data
                .done (response) =>
                    @callback()
                    dialog.close @
        cancel: ->
            dialog.close @
        show:(callback) ->
            @callback = callback
            dialog.show @