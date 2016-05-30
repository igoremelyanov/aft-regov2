class OnlineDeposit extends FormBase
    constructor: ->
        super
        
        @amount = ko.observable(0).extend
            validatable: yes
        @code = ko.observable('').extend
            validatable: yes
        @hasErrors = ko.observable no
        @isEnabled = ko.observable no
        
        @step2 = ->
            amount = @amount()
            
            if amount is ''
                @isEnabled(no)
                @['amount'].__valid__ no
                return no
            
            if isNaN amount
                @['amount'].__valid__ no
                @isEnabled(no)
            else
                checkAmount = parseInt amount
                
                if isNaN checkAmount
                    @['amount'].__valid__ no
                    @isEnabled(no)
            
                if checkAmount < 200 or checkAmount > 50000
                    @['amount'].__valid__ no
                    @isEnabled(no)
                else 
                    @isEnabled(yes)
        
        @submitCode = ->
            $.postJson '/api/Code',
                amount: @amount
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
            alert "submit deposit"
            $.postJson '/api/OnlineDeposit',
                amount: @amount
                .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        #observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.error = 'test'
                        observable.__valid__ no
                else
                    popupAlert('', '')
        
    selectBonus: (item, event) ->
        target = $(event.target)
        
        if target.closest('.col-sm-6').hasClass 'disable'
            return no
        
        if target.closest('.col-sm-6').hasClass 'selected'
            target.closest('.col-sm-6').removeClass 'selected'
        else
            target.closest('.col-sm-6').addClass 'selected'
                    
    setAmount: (amount) =>
        addAmount = amount.replace ',', ''        
        @amount(parseInt(addAmount))
        @step2()

    model = new OnlineDeposit
    model.errors = ko.validation.group(model);
    ko.applyBindings model, document.getElementById "cashier-wrapper"