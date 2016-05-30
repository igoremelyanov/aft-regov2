class Withdrawal extends FormBase
    constructor: ->
        super
        $("#page").i18n();
        @amount = ko.observable().extend
            validatable: yes
        @notifySms = ko.observable false
        @notifyEmail = ko.observable false
        @isSuccessMessageVisible = ko.observable no
        @submitForm = ()=>
            notificationType = 0
            if @notifySms()
                notificationType = 1
            if @notifyEmail()
                notificationType = 2
            if @notifySms() and @notifyEmail()
                notificationType = 3
            data = 
                amount : @amount()
                notificationType: notificationType
            $.postJson '/api/ValidateWithdrawalRequest', data
                .success (response) =>
                     if response.hasError
                        $.each response.errors, (propName) =>
                            observable = @[propName]
                            error = JSON.parse(response.errors[propName])
                            observable.error = i18n.t error.text, error.variables
                            observable.__valid__ no
                     else
                        $.post '/api/OfflineWithdrawal', data
                            .success (response) =>
                                redirect '/home/withdrawal?isSuccess=true'
            no
            
viewModel = new Withdrawal()
ko.applyBindings viewModel, document.getElementById "withdrawal-wrapper"