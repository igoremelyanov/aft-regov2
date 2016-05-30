define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    
    class ViewModel extends require "vmGrid"
        constructor: ->  
            super
            
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.update, security.categories.paymentLevelSettings
            
            @noRecordsFound = ko.observable off
            
            @rowId = ko.observable()
            
            @selectedRowId = ko.observable()
            
            @brandNameSearchPattern = ko.observable()
            @Search = ko.observable()

            [@brands, @licensees, @paymentGateways, @statuses, @paymentTypes, @paymentMethods,@currencies,@paymentLevels] = ko.observableArrays()


            @filtersCriteria = ko.computed =>
                licensee = @licensees
                brand = @brands
                criteria = {}
                criteria['Brand.LicenseeName'] = licensee if licensee?
                criteria['Brand.Name'] = brand if brand?
                criteria

        reloadGrid: ->
            $("#payment-level-settings-grid").trigger "reload"
        
        rowChange: (row) ->
            @noRecordsFound ($("#payment-level-settings-grid")[0].gridParam "reccount") is 0
            
        activate: ->
            super
            $.get 'Licensee/Licensees?useFilter=true'
                .done (response) => @licensees.push item.name for item in response.licensees
               
            $.get '/PaymentSettings/BrandsList'
                .done (response) => @brands.push item for item in response
    
            $.get "/PaymentSettings/PaymentTypesList"
                .done (response) => @paymentTypes.push item for item in response

            $.get "/PaymentSettings/PaymenMethodsList"
                .done (response) => @paymentMethods.push item for item in response
                
            $.get "/PaymentSettings/CurrencyList"
                .done (response) => @currencies.push item for item in response
                
            $.get "/PaymentLevel/GetPaymentLevels"
                .done (response) => @paymentLevels.push item.name for item in response.data.paymentLevels

        openEditTab: ->
            grid =$("#payment-level-settings-grid")[0];
            selectedData = grid.gridParam("selarrrow")
            playerData =[]
            if selectedData.length>0
                ko.utils.arrayForEach selectedData, (playerid) =>
                        row = grid.getRowData(playerid)
                        row.id = playerid
                        playerData.push row
                
                nav.open
                    path: "player-manager/payment-level-settings/setPaymentLevel"
                    title: i18n.t "app:playerManager.paymentLevelSettings.setPaymentLevel"
                    data: {
                        playerData: playerData
                        brandId :playerData[0].BrandId
                        currency : playerData[0].Currency
                    }
    