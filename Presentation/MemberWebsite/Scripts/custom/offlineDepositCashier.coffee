class OfflineDeposit extends FormBase
    constructor: ->
        super
        @skipBonusModalId = "skipbonus-alert-modal"
        @amount = ko.observable("0").extend
            required: yes
            validatable: yes
            mustNotContainLetters: yes
            notNegativeAmount: yes
            isValidAmount: yes
            validation:[{
                validator: (val, min) ->
                    unformattedValue = val.replace /,/g, ''
                    return unformattedValue >= min()
                message: 'Amount must be greater or equals to a minimum deposit of ${0}'
                params: () =>
                    return parseFloat($('#deposit-amount-min').val())
            },{
                validator: (val, max) ->
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
        
        @depositCaption = ko.computed ()=>
            if @claimedBonus()
                return i18n.t "balanceInformation.depositAndGetBonus"
            
            return "Deposit"
                
        @code = ko.observable('').extend
            validatable: yes
        @bank = ko.observable('').extend
            validatable: yes
        @remark = ko.observable('').extend
            validatable: yes
        @hasErrors = ko.observable no
        @smsEnabled = ko.observable no
        @emailEnabled = ko.observable no
        
        @notificationMethod = ko.computed ()=>
            if (@smsEnabled() && @emailEnabled())
                return 3
            else if (@emailEnabled())
                return 1
            else if (@smsEnabled())
                return 2
            else return 0
        
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
                
        @submitDeposit = ->
            bonusId = if @claimedBonus() == undefined then null else @claimedBonus().id
            if(@bonuses().length > 0 && bonusId == null)
                $("#" + @skipBonusModalId).modal()
            else 
                @goAheadWithDeposititng()
                     
        
    selectBonus: (bonusItem) =>
        if (@amount() < bonusItem.requiredAmount)
            return
    
        if (@claimedBonus() != bonusItem)
            @claimedBonus bonusItem
        else
            @claimedBonus undefined
                    
    setAmount: (amount) =>
        addAmount = @parseAmountString(amount)
        @amount addAmount.toString()
        
    setError: (observable, errorMessage)=> 
        observable.error = errorMessage
        observable.__valid__ no
        
    clearError: (observable)=>
        observable.error = ''
        observable.__valid__ yes
        
    skipDepositAndGetBonus: ->
        $("#" + @skipBonusModalId).modal('hide')
        
    goAheadWithDeposititng: ->
        bonusId = if @claimedBonus() == undefined then null else @claimedBonus().id
        $.postJson '/api/offlineDeposit',
                BankAccountId: @bank()
                Amount: @amount()
                NotificationMethod: @notificationMethod()
                PlayerRemarks: @remark()
                BonusId: bonusId
            .done (response) =>
                query = "?depositId=" + response.id
                if response.bonusRedemptionId != undefined
                    query = query + "&redemptionId=" + response.bonusRedemptionId
                
                redirect "Home/OfflineDepositConfirmation" + query
            .fail (jqXHR) =>
                popupAlert("Error occured.", "Contact an administrator.")
        
    model = new OfflineDeposit
    model.errors = ko.validation.group(model);
    ko.applyBindings model, document.getElementById "cashier-wrapper"