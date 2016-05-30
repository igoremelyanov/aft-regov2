define (require) ->
    require "controls/grid"
    nav = require "nav"
    security = require "security/security"

    class DuplicateMechanismConfigsViewModel
        constructor: ->
            @selectedRowId = ko.observable()
            
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.duplicateConfiguration
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.create, security.categories.duplicateConfiguration

        attached: (view) ->
            $grid = findGrid view
            
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $grid.on "gridLoad selectionChange", (e, row) =>
                @selectedRowId row.id

        create: ->
            nav.open {
                path: 'fraud/duplicate-mechanism/edit',
                title: "New Duplicate Mechanism Configuration",
                data: {
                    editMode: true
                }
            }

        edit: ->
            nav.open {
                path: 'fraud/duplicate-mechanism/edit',
                title: "Edit Duplicate Mechanism Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: true
                }
            }
            
        view: ->
            nav.open {
                path: 'fraud/duplicate-mechanism/edit',
                title: "View Duplicate Mechanism Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: false
                }
            }