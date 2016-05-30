define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18N = require "i18next"
    jgu = require "JqGridUtil"
    security = require "security/security"
    IdentitySettingsRemarksDialog = require "admin/identification-document-settings/remarks-dialog/remarks-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @selectedRowId = ko.observable()
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.identificationDocumentSettings
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.create, security.categories.identificationDocumentSettings
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.identificationDocumentSettings
                  
        openViewTab: () ->
            nav.open
                path: "admin/identification-document-settings/edit"
                title: i18N.t "View"
                data:  {
                    id: @rowId() if @rowId()?
                    submitted: yes
                }

        openAddTab: () ->
            nav.open
                path: "admin/identification-document-settings/add"
                title: i18N.t "New"

        openEditTab: () ->
            nav.open
                path: "admin/identification-document-settings/edit"
                title: i18N.t "Edit"
                data: id: @rowId() if @rowId()?       
                
        attached: (view) ->
            self = this
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $grid.on "gridLoad selectionChange", (e, row) =>
                @rowId row.id
                @rowChange row
                
            $(view).on "click", ".identity-settings-remark", ->
                id = $(@).parents("tr").first().attr "id"
                remark = $(@).attr "title"
                (new IdentitySettingsRemarksDialog id, remark).show ->
                    $grid.trigger "reload"
