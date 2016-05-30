class OnlineDeposit extends FormBase
    constructor: ->
        super
        @skipBonusModalId = "skipbonus-alert-modal"
        @depositRequestInProgress = ko.observable(no)
        @amount = ko.observable("0").extend
            required: yes
            validatable: yes
            mustNotContainLetters: yes
            notNegativeAmount: yes
            isValidAmount: yes
            validation:[{
                validator: (val, min) =>
                        unformattedValue = val.replace /,/g, ''
                        return unformattedValue >= min()
                    message: 'Amount must be greater or equals to a minimum deposit of ${0}'
                    params: () =>
                        return parseFloat($('#deposit-amount-min').val())
            },{
                validator: (val, max) =>
                            if (max() == 0)
                                return yes;
                            
                            unformattedValue = val.replace /,/g, ''
                            return unformattedValue <= max()
                    message: 'Amount must be less or equals to a maximum deposit of ${0}'
                    params: () =>
                        return parseFloat($('#deposit-amount-max').val())
            }]
            
        @amount.subscribe () =>
            @claimedBonus undefined
            
        $.ajax "/api/GetVisibleDepositQualifiedBonuses",
            success: (response) =>
                @bonuses response
         
        @parseAmountString = (amountStr) =>
            purgedString = amountStr.replace /,/g, ''
            return parseFloat(purgedString)
                    
        @amountEntered = ko.computed () =>
            value = @parseAmountString(@amount())
            value > 0
                    
        @bonuses = ko.observableArray()
        @claimedBonus = ko.observable()
        @code = ko.observable('').extend
            validatable: yes
        @hasErrors = ko.observable no
        
        @depositCaption = ko.computed ()=>
            if @claimedBonus()
                return i18n.t "balanceInformation.depositAndGetBonus"
            
            return "Deposit"
        @submitRequest = =>
            bonusId = if @claimedBonus() == undefined then null else @claimedBonus().id
            if(@bonuses().length > 0 && bonusId == null)
                $("#" + @skipBonusModalId).modal()
            else 
                @goAheadWithDeposititng()
                           
        @createOnlineDepositObject = (bonusId) =>
            onlineDeposit = new OnlineDepositModel "/home/OnlineDepositConfirmation",'', () => 
                popupAlert("Error occured.", "Contact an administrator.")
            onlineDeposit.onlineAmount @amount()
            onlineDeposit.onlineDepositBonusId bonusId

        @submitCode = =>
            @clearError @code
            
            bonusByCode = _.find @bonuses(), (item) =>
                item.code == @code()
                
            if bonusByCode != undefined
                @claimedBonus bonusByCode
                @code ''
            else
                $.postJson "/api/ValidateFirstDepositBonus", { depositAmount: @amount(), bonusCode: @code() }
                .done (response) => 
                    if response.isValid
                        elem = _.find @bonuses(), (item) =>
                            item.id == response.bonus.id
                        if elem == undefined
                            @bonuses.unshift response.bonus
                            @claimedBonus response.bonus
                            @code ''
                    else
                        @setError @code, response.errors[0]
                .fail (failResponse) => 
                    popupAlert("Error occured.", "Contact an administrator.")
                
        @validate = (url, data, onSuccess) =>
            $.postJson url, data
                .done (response) =>
                    if response.hasError
                        @onErrorHandler response
                    else
                        onSuccess(response)
                .fail (failResponse) => 
                    @onErrorHandler failResponse
        
        @onErrorHandler = (failResponse) =>
            field = @['amount']
            message = failResponse.errors['amount']
            if(field)
                field.error = message
                field.__valid__(false)
                        
    setAmount: (amount) =>
        addAmount = @parseAmountString(amount)
        @amount addAmount.toString()
        
    setError: (observable, errorMessage)=> 
        observable.error = errorMessage
        observable.__valid__ no
        
    clearError: (observable)=>
        observable.error = ''
        observable.__valid__ yes

    selectBonus: (bonusItem) =>
        if (@amount() < bonusItem.requiredAmount)
            return
    
        if (@claimedBonus() != bonusItem)
            @claimedBonus bonusItem
        else
            @claimedBonus undefined

    skipDepositAndGetBonus: ->
        $("#" + @skipBonusModalId).modal('hide')
        
    goAheadWithDeposititng: ->
        data = {amount: @amount()}
        @validate '/api/validateonlinedepositamount', data, () =>
            bonusId = if @claimedBonus() == undefined then null else @claimedBonus().id 
            onlineDeposit = @createOnlineDepositObject bonusId
            onlineDeposit.submitOnlineDeposit()
        
    model = new OnlineDeposit
    model.errors = ko.validation.group(model);
    ko.applyBindings model, document.getElementById "cashier-wrapper"