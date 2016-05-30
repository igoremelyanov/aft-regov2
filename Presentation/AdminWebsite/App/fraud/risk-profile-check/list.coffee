define (require) ->
    require "controls/grid"
    nav = require "nav"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @playerId = ko.observable()
            @selectedRowId = ko.observable()

        activate: (data) ->
            @playerId ''

        attached: (view) ->
            $grid = findGrid view
            
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $grid.on "gridLoad selectionChange", (e, row) =>
                @selectedRowId row.id
                #@rowChange row

        create: ->
            nav.open {
                path: 'fraud/risk-profile-check/edit',
                title: "New Risk Profile Check Configuration",
                data: {
                    editMode: true
                }
            }

        edit: ->
            nav.open {
                path: 'fraud/risk-profile-check/edit',
                title: "Edit Risk Profile Check Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: true
                }
            }
            
        view: ->
            nav.open {
                path: 'fraud/risk-profile-check/edit',
                title: "View Risk Profile Check Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: false
                }
            }