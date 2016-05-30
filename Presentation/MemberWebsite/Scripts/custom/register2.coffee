class RegisterStep2
    constructor: ->
        @amount = ko.observable($("#hidden-amount").val()).extend
            required: yes
            validatable: yes
        
        @bonuscode = ko.observable().extend
            validatable: yes
        @selectedBonusId = ko.observable()
        @isEnabled = ko.observable no
        @submitDisabled = ko.observable no
        @buttonTitle = ko.computed () =>
            if @selectedBonusId()
                return "DEPOSIT AND GET BONUS"
            else
                return "DEPOSIT"

        @selectBonus = (id, requiredAmount) =>
            if requiredAmount > @amount()
                return no

            if @selectedBonusId() == id
                @selectedBonusId('')
            else
                @selectedBonusId id

        @onSubmit = (formElement) =>
            if isNaN(@amount())
                field = @['amount']
                if(field)
                    field.error = "You can enter only numbers"
                    field.__valid__(false)
                    return no
            data = {amount: @amount()}
            @validate '/api/validateonlinedepositamount', data, () =>
                onlineDeposit = @createOnlineDepositObject @selectedBonusId()
                onlineDeposit.submitOnlineDeposit()

        @createOnlineDepositObject = (bonusId) =>
            onlineDeposit = new OnlineDepositModel "/home/registerstep4",'', () => 
                popupAlert("Error occured.", "Contact an administrator.")
            onlineDeposit.onlineAmount @amount()
            onlineDeposit.onlineDepositBonusId bonusId

        @validate = (url, data, onSuccess) =>
            $.postJson url, data
                .done (response) =>
                    if response.hasError
                        @onErrorHandler response
                    else
                    onSuccess(response)
                .fail (failResponse) => 
                    @onErrorHandler failResponse

    validateBonusCode: ()=>
            $.postJson "/api/ValidateFirstDepositBonus", {bonusCode: @bonuscode(), depositAmount: @amount()}
                .done (response) =>
                    if not response.isValid
                        field = @['bonuscode']
                        if(field)
                            field.error = response.errors[0]
                            field.__valid__(false)
                    else
                        redirect '/home/registerstep2?bonusCode=' + response.bonus.code + "&amount=" + @amount()
                          
    onErrorHandler: (failResponse) =>
        field = @['amount']
        message = failResponse.errors['amount']
            if(field)
            field.error = message
                field.__valid__(false)
                
    setAmount: (amount) =>
        @amount(amount)
    
    submitBonus: () =>

    model = new RegisterStep2()
    ko.applyBindings model, $("#register2-wrapper")[0]
