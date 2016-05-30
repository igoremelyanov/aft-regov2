define 	["nav", 
        'durandal/app', 
        "i18next", 
        "security/security", 
        "shell", 
        "controls/grid", 
        "JqGridUtil", 
        "CommonNaming",
        "fraud/verification/activate-dialog",
        "fraud/verification/deactivate-dialog",
        "vmGrid"],
(nav, app, i18n, security, shell, common, jgu, CommonNaming, activateDialog, deactivateDialog, vmGrid) ->
    class ViewModel extends vmGrid
        constructor: ->
            super
            @naming = new CommonNaming("verification-manager")
            @gridId = "#verification-manager-list"
            @rowId = ko.observable()
            @shell = shell
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.create, security.categories.autoVerificationConfiguration
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.autoVerificationConfiguration
            @isDeleteAllowed = ko.observable security.isOperationAllowed security.permissions.delete, security.categories.autoVerificationConfiguration
            
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.autoVerificationConfiguration
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.autoVerificationConfiguration
            
            @canActivate = ko.observable no
            @canDeactivate = ko.observable no
            @compositionComplete = =>
                    $ =>
                        $(@gridId).on "gridLoad selectionChange", (e, row) =>
                            @rowId row.id
                            @canActivate row.data.Status is "Inactive"
                            @canDeactivate row.data.Status is "Active"
        @onBrandChanged = =>
                $(@gridId).trigger "reload"
        
        $(document).on "brand_changed", @onBrandChanged
            
        @detached = =>
            $(document).off "brand_changed", @onBrandChanged
            
        add: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "New Auto Verification Configuration",
                data: {
                    editMode: true
                }
            }

        openViewTab: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "View Auto Verification Configuration",
                data: {
                    id: @rowId(),
                    editMode: false
                }
            }
                
        openEditTab: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "Edit Auto Verification Configuration",
                data: {
                    id: @rowId(),
                    editMode: true
                }
            }
            
        openActivateDialog: ->
            activateDialog.show @, @rowId()
        
        openDeactivateDialog: ->
            deactivateDialog.show @, @rowId()
