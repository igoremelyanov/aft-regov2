define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    statusDialog = require "payments/settings/status-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->  
            super     
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.create, security.categories.paymentSettings
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.paymentSettings
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.paymentSettings
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.paymentSettings
            
            @noRecordsFound = ko.observable off
            
            @rowId = ko.observable()
            
            @selectedRowId = ko.observable()
            
            @brandNameSearchPattern = ko.observable()
            @Search = ko.observable()
            
            @canActivate = ko.observable no
            @canDeactivate = ko.observable no   
            
            [@brands, @currencies, @licensees, @paymentTypes, @paymentMethods, @statuses] = ko.observableArrays()
            @vipLevels =  ko.observable {}
            
        reloadGrid: ->
            $("#payment-settings-grid").trigger "reload"
        
        rowChange: (row) ->
            @canActivate row.data.Enabled is "Inactive" 
            @canDeactivate row.data.Enabled is "Active"
            @noRecordsFound ($("#payment-settings-grid")[0].gridParam "reccount") is 0
            
        activate: ->           
            super
            $.get '/PaymentSettings/LicenseesList'
                .done (response) => @licensees.push item for item in response
               
            $.get '/PaymentSettings/BrandsList'
                .done (response) => @brands.push item for item in response
                
            $.get "/PaymentSettings/PaymentTypesList"
                .done (response) => @paymentTypes.push item for item in response
                
            $.get "/PaymentSettings/VipLevelList"
            .done (response) =>
                @vipLevels()[item.Id] = item.Name for item in response
                
            $.get "/PaymentSettings/PaymenMethodsList"
            .done (response) => @paymentMethods.push item for item in response
                
            $.get "/PaymentSettings/CurrencyList"
                .done (response) => @currencies.push item for item in response
                
            $.get "/PaymentSettings/StatusesList"
                .done (response) => @statuses.push item for item in response
            
                        
        openAddTab: ->
            nav.open
                path: "payments/settings/details"
                title: i18n.t "app:payment.settings.newSettings"

        openEditTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/settings/details"
                    title: i18n.t "app:payment.settings.editSettings"
                    data: {
                        id: @rowId()
                        editMode: true
                    }
    
        openViewTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/settings/details"
                    title: i18n.t "app:payment.settings.viewSettings"
                    data: {
                        id: @rowId()
                        editMode: false
                    }
         
        openActivateDialog: ->
            statusDialog.show @canDeactivate(), @rowId()
            
        openDeactivateDialog: ->
            statusDialog.show @canDeactivate(), @rowId()     
 
