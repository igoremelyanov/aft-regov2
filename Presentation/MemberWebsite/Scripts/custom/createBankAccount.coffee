class CreateBankAccount extends FormBase
    constructor: ->
        super
        $("#page").i18n();
        @branch = ko.observable().extend
            validatable: yes
        @bankId = ko.observable().extend
            validatable: yes
        @accountName = ko.observable().extend
            validatable: yes
        @province = ko.observable().extend
            validatable: yes
        @accountNumber = ko.observable().extend
            validatable: yes
        @city = ko.observable().extend
            validatable: yes
        
        @banks = ko.observableArray()
        
        $.get "/api/getbanks"
            .success (response) =>
                @banks response
        
        @submitForm = ()=>
            data = 
                branch : @branch()
                bank: @bankId()
                accountName: @accountName()
                province: @province()
                accountNumber: @accountNumber()
                city: @city()
            $.postJson '/api/ValidatePlayerBankAccount', data
                .success (response) =>
                     if response.hasError
                        $.each response.errors, (propName) =>
                            observable = @[propName]
                            observable.error = i18n.t JSON.parse(response.errors[propName]).text
                            observable.__valid__ no
                     else
                        $.post '/api/CreatePlayerBankAccount', data
                            .success (response) =>
                                redirect '/home/withdrawal'
            no
            
viewModel = new CreateBankAccount()
ko.applyBindings viewModel, document.getElementById "create-bank-account-wrapper"