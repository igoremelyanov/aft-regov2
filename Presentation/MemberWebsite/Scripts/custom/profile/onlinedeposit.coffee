class OnlineDeposit extends FormBase
    constructor: ->
        super
        
        @amount = ko.observable('test').extend
            validatable: yes
        @code = ko.observable('')
            validatable: yes
        @hasErrors = ko.observable(false)
        
        @setAmount = ->
        
        @submitWithdrawalDetail = ->
        
            $.postJson '/api/Withdrawal',
                amount: @amount
                sms: @sms
                email: @email
             .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        #observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.error = 'test'
                        observable.__valid__ no    
                else
                    popupAlert('', '')
        
        @submitOnlineDeposit = ->
            alert "dede"

viewModel = new OnlineDeposit()
viewModel.load()
ko.applyBindings viewModel, document.getElementById "cashier-wrapper"


alert "eddede"