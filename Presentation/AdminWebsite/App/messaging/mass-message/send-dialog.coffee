define (reguire) ->
    dialog = require "plugins/dialog"
    
    class SendDialog
        constructor: (closeParent) ->
            @closeParent = closeParent

        show : ->
            dialog.show @
            
        done: ->
            @closeParent()
            dialog.close @