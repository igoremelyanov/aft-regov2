define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    security = require "security/security"
    
    class ViewModel 
        constructor: ->        
            @selectedRowId = ko.observable()
            @selectedRowDepositType = ko.observable()
            @usernameSearchPattern = ko.observable()
            @filterVisible = ko.observable off           
            [@paymentMethods] = ko.observableArrays()
            
            @isVerifyBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.verify, security.categories.depositVerification
   
            @isUnverifyBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.unverify, security.categories.depositVerification
            
            @isRejectBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.reject, security.categories.depositVerification

            @compositionComplete = =>
                $ =>
                    $("#deposit-verify-grid").on "gridLoad selectionChange", (e, row) =>
                        @selectedRowId row.id
                        @selectedRowDepositType row.data.DepositType
                    $("#deposit-verify-username-search-form").submit =>
                        @usernameSearchPattern $('#deposit-verify-username-search').val()
                        $("#deposit-verify-grid").trigger "reload"
                        off
                
        verifyDepositRequest: ->            
            nav.open
                path: @getPath()
                title: i18n.t "app:common.verify"
                data:  
                    hash: '#offline-deposit-confirm'
                    requestId: @selectedRowId()
                    action: 'verify'
                
        unverifyDepositRequest: ->
            nav.open
                path: @getPath()
                title: i18n.t "app:common.unverify"
                data:
                    hash: '#offline-deposit-confirm',
                    requestId: @selectedRowId()
                    action: 'unverify'                    
    
        rejectDepositRequest: ->
            nav.open
                path:  @getPath()
                title: i18n.t "app:common.reject"
                data:
                    hash: '#online-deposit-reject',
                    requestId: @selectedRowId()
                    action: 'reject'  
        
        getPath: ->            
            if @selectedRowDepositType() == 'Online'
                url = 'player-manager/offline-deposit/verifyOnlineDeposit'
            else
                url = 'player-manager/offline-deposit/verify'
        
        activate: ->
            $.get "/PaymentSettings/PaymenMethodsList"
            .done (response) => @paymentMethods.push item for item in response
        