define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    shell = require "shell"
    statusDialog = require "brand/brand-manager/status-dialog"
    config = require "config"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @config = config
            @shell = shell
            @gridId = "#brand-grid"
            @rowId = ko.observable()
            
            @canActivate = ko.observable()
            @canDeactivate = ko.observable()
            @canEdit = ko.observable(false)
            
            @hasAddPermission = ko.observable security.isOperationAllowed security.permissions.create, security.categories.brandManager
            @hasEditPermission = ko.observable security.isOperationAllowed security.permissions.update, security.categories.brandManager
            @hasViewPermission = ko.observable security.isOperationAllowed security.permissions.view, security.categories.brandManager
            @hasActivatePermission = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.brandManager
            @hasDeactivatePermission = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.brandManager

            @compositionComplete = =>
                $ =>
                    $(@gridId).on "gridLoad selectionChange", (e, row) =>
                        @rowId row.id
                        @canActivate row.data.Status is "Inactive"
                        @canDeactivate row.data.Status is "Active"
                        @canEdit @canActivate()

            @onBrandChanged = =>
                $(@gridId).trigger "reload"

            $(document).on "brand_changed", @onBrandChanged
            
            @detached = =>
                $(document).off "brand_changed", @onBrandChanged

        openAddTab: ->
            nav.open
                path: 'brand/brand-manager/add-brand'
                title: i18n.t "app:brand.newBrand"
                
        openEditTab: ->
            id = @rowId()
            nav.open
                path: 'brand/brand-manager/edit-brand'
                title: i18n.t "app:brand.edit"
                data: { id: id } if id?
                
        openViewTab: ->
            id = @rowId()
            nav.open
                path: 'brand/brand-manager/view-brand'
                title: i18n.t "app:brand.view"
                data: { id: id } if id?

        showActivateDialog: ->
            statusDialog.show @rowId()

        showDeactivateDialog: ->
            statusDialog.show @rowId(), true