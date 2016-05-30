define (require) ->
    require "controls/grid"
    nav = require "nav"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @selectedRowId = ko.observable()

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
                path: 'fraud/signup-fraud-types/edit',
                title: "New Sign Up Fraud Type",
                data: {
                    editMode: true
                }
            }

        edit: ->
            nav.open {
                path: 'fraud/signup-fraud-types/edit',
                title: "Edit Sign Up Fraud Type",
                data: {
                    id: @selectedRowId(),
                    editMode: true
                }
            }
            
        view: ->
            nav.open {
                path: 'fraud/signup-fraud-types/edit',
                title: "View Sign Up Fraud Type",
                data: {
                    id: @selectedRowId(),
                    editMode: false
                }
            }