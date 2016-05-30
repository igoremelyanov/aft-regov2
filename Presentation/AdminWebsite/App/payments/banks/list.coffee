define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    shell = require "shell"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super

            @shell = shell
            @rowId = ko.observable()
            @gridId = "#banks-list"
            @hasAddPermission = ko.observable security.isOperationAllowed security.permissions.create, security.categories.banks
            @hasEditPermission = ko.observable security.isOperationAllowed security.permissions.update, security.categories.banks
            @hasViewPermission = ko.observable security.isOperationAllowed security.permissions.view, security.categories.banks

            @compositionComplete = =>
                $ =>
                    $(@gridId).on "gridLoad selectionChange", (e, row) =>
                        @rowId row.id

            @onBankChange = =>
                $(@gridId).trigger "reload"

            $(document).on "bank_changed", @onBankChange
            
            @detached = =>
                $(document).off "bank_changed", @onBankChange

        openAddTab: ->
            nav.open
                path: "payments/banks/add"
                title: i18n.t "app:common.new"

        openViewTab: ->
            id = @rowId()
            if id?
                nav.open
                    path: "payments/banks/view"
                    title: i18n.t "app:common.view"
                    data: 
                        id: id

        openEditTab: ->
            id = @rowId()
            if id?
                nav.open
                    path: "payments/banks/edit"
                    title: i18n.t "app:common.edit"
                    data: 
                        id: id