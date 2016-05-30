define (require) ->
    require "controls/grid"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            @playerId = ko.observable()
            @config = require "config"
        
        activate: (data) ->
            @playerId data.playerId
            
        attached: (view) ->
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
