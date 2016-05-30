define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    security = require "security/security"
    
    class ViewModel 
        constructor: ->       
            @i18n = i18n
            @selectedRowId = ko.observable()
            @usernameSearchPattern = ko.observable()
            @filterVisible = ko.observable off           
            
            @isConfirmBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.confirm, security.categories.offlineDepositConfirmation
   
            @isViewBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.view, security.categories.offlineDepositRequests            

            @transferTypeFormatter = (transferType) => 
                i18n.t "app:payment.transferTypes." + transferType
                
            @depositMethodFormatter = (depositMethod) => 
                i18n.t "app:payment.depositMethods." + depositMethod

            @compositionComplete = =>
                $ =>
                    $("#offline-deposit-confirm-grid").on "gridLoad selectionChange", (e, row) =>
                        @selectedRowId row.id
                    $("#offline-deposit-confirm-username-search-form").submit =>
                        @usernameSearchPattern $('#offline-deposit-confirm-username-search').val()
                        $("#offline-deposit-confirm-grid").trigger "reload"
                        off                        
                        

                
        confirmDepositRequest: ->            
            nav.open
                path: 'player-manager/offline-deposit/confirm'
                title: i18n.t "app:payment.offlineDepositRequest.tabTitle.confirm"
                data:  
                    hash: '#offline-deposit-confirm'
                    requestId: @selectedRowId()
                
        viewDepositRequest: ->
            nav.open
                path: 'player-manager/offline-deposit/view-request'
                title: i18n.t "app:payment.offlineDepositRequest.view"
                data:
                    hash: '#offline-deposit-view',
                    requestId: @selectedRowId()

        
        