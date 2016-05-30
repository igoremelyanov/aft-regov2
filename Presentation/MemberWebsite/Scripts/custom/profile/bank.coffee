class @Bank extends FormBase
    constructor: (@id) ->
        super
        
        @name = ko.observable('').extend
            validatable: yes
        @bank = ko.observable('').extend
            validatable: yes
        @account_number = ko.observable('').extend
            validatable: yes
        @branch = ko.observable('').extend
            validatable: yes
        @province = ko.observable('').extend
            validatable: yes
        @city = ko.observable('').extend
            validatable: yes
        @hasErrors = ko.observable(false)
        
        @submitBankDetail = ->
            $.postJson '/api/Bank',
                name: @name
                bank: @bank
                account_number: @account_number
                branch: @branch
                province: @province
                city: @city
             .success (response) =>
                    if response.hasError
                        $.each response.errors, (propName)=>
                            observable = @[propName]
                            #observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                            observable.error = 'test'
                            observable.__valid__ no    
                    else
                        popupAlert('', '')