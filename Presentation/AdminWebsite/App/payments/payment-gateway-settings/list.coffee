define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    statusDialog = require "payments/payment-gateway-settings/status-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->  
            super     
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.create, security.categories.paymentGatewaySettings
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.paymentGatewaySettings
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.paymentGatewaySettings
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.paymentGatewaySettings
            
            @noRecordsFound = ko.observable off
            
            @rowId = ko.observable()
            
            @selectedRowId = ko.observable()
            
            @brandNameSearchPattern = ko.observable()
            @Search = ko.observable()
            
            @canActivate = ko.observable no
            @canDeactivate = ko.observable no
            @canEdit = ko.observable no

            [@brands, @licensees, @paymentGateways, @statuses] = ko.observableArrays()
            @vipLevels =  ko.observable {}
            
        reloadGrid: ->
            $("#payment-gateway-settings-grid").trigger "reload"
        
        rowChange: (row) ->
            @canActivate row.data.Status is "Inactive" 
            @canDeactivate row.data.Status is "Active"
            @canEdit row.data.Status is "Inactive" 
            @noRecordsFound ($("#payment-gateway-settings-grid")[0].gridParam "reccount") is 0
            
        activate: ->           
            super
            $.get '/PaymentSettings/LicenseesList'
                .done (response) => @licensees.push item for item in response
               
            $.get '/PaymentSettings/BrandsList'
                .done (response) => @brands.push item for item in response
                                                
            $.get "/PaymentSettings/StatusesList"
                .done (response) => @statuses.push item for item in response
                
            $.get '/PaymentGatewaySettings/GetPaymentGateways'
                .done (response) => @paymentGateways.push item.name for item in response.data.paymentGateways
                        
        openAddTab: ->
            nav.open
                path: "payments/payment-gateway-settings/details"
                title: i18n.t "app:payment.newPaymentGatewaySettings"
                data:{
                    pageMode:'Add'
                }
        openEditTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/payment-gateway-settings/details"
                    title: i18n.t "app:payment.editPaymentGatewaySettings"
                    data: {
                        id: @rowId()
                        editMode: true,
                        pageMode:'Edit'
                    }
    
        openViewTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/payment-gateway-settings/details"
                    title: i18n.t "app:payment.viewPaymentGatewaySettings"
                    data: {
                        id: @rowId()
                        editMode: false
                        pageMode:'View'
                    }
         
        openActivateDialog: ->
            statusDialog.show @canDeactivate(), @rowId()
            
        openDeactivateDialog: ->
            statusDialog.show @canDeactivate(), @rowId()     
 
