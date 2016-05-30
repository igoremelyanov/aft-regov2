define (require) ->
    require "controls/grid"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            @playerId = ko.observable()
            @config = require "config"
            [@paymentMethods] = ko.observableArrays()
        
        activate: (data) ->
            @playerId data.playerId
            
            $.get "/PaymentSettings/PaymenMethodsList"
            .done (response) => @paymentMethods.push item for item in response
            
        attached: (view) ->
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
