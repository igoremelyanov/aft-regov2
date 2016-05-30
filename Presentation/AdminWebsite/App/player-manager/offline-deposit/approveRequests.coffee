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
            
            @isApproveBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.approve, security.categories.depositApproval
   
            @isRejectBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.reject, security.categories.depositApproval

            @compositionComplete = =>
                $ =>
                    $("#deposit-approve-grid").on "gridLoad selectionChange", (e, row) =>
                        @selectedRowId row.id
                        @selectedRowDepositType row.data.DepositType
                    $("#deposit-approve-username-search-form").submit =>
                        @usernameSearchPattern $('#deposit-approve-username-search').val()
                        $("#deposit-approve-grid").trigger "reload"
                        off                        
                
        approveDepositRequest: ->            
            nav.open
                path: @getPath()
                title: i18n.t "app:common.approve"
                data:  
                    hash: '#offline-deposit-approve'
                    requestId: @selectedRowId()
                    action: 'approve'
                
        rejectDepositRequest: ->
            nav.open
                path: @getPath()
                title: i18n.t "app:common.reject"
                data:
                    hash: '#offline-deposit-reject'
                    requestId: @selectedRowId()
                    action: 'reject'
                        
        activate: ->
            $.get "/PaymentSettings/PaymenMethodsList"
            .done (response) => @paymentMethods.push item for item in response
           
        getPath: ->            
            if @selectedRowDepositType() == 'Online'
                url = 'player-manager/offline-deposit/verifyOnlineDeposit'
            else
                url = 'player-manager/offline-deposit/approve'

        
        