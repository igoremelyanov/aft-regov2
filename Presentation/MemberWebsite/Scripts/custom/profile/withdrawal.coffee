class @Withdrawal extends FormBase
    constructor: (@id) ->
        super
        
        @amount = ko.observable('').extend
            validatable: yes
        @sms = ko.observable('')
        @email = ko.observable('')
        @hasErrors = ko.observable(false)
        
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
                