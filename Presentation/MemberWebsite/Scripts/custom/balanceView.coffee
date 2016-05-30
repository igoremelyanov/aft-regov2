class BalanceViewModel
    currentAttempt = 0
    settings =
        balanceApiUrl: '/api/getbalance'
        balancePlaceholderSelector: '.js-balance-live-view'
        currencyCodePlaceholderSelector: '.js-balance-currency-code'
        currencySymbolPlaceholderSelector: '.js-balance-currency-symbol'
        spinnerHtml: '<i class="fa fa-spinner fa-spin"></i>'
        stubKey: 'app:balanceInformation.balanceNotAvailableStub'
        defaultBalanceType: 'playable'
        balanceTypeDataAttribute: 'balance-type'
        initialDelay: 1000        
        maxAttempts: 10

    constructor: ->                
        @currentBalance = ko.observable {}
        @shouldShowSpinner = ko.observable true
    
    showBalance: =>
        @shouldShowSpinner false
        $(settings.balancePlaceholderSelector).each (index, element) =>
            el = $(element)
            balanceType = el.data(settings.balanceTypeDataAttribute) ? settings.defaultBalanceType
            el.text @currentBalance[balanceType]
            
    showCurrencyCode: =>
        $(settings.currencyCodePlaceholderSelector).each (index, element) =>
            el = $(element)
            el.text @currentBalance.currencyCode
            
    showCurrencySymbol: =>
        $(settings.currencySymbolPlaceholderSelector).each (index, element) =>
            el = $(element)
            el.text @currentBalance.currencySymbol            

    showSpinner: =>
        @shouldShowSpinner true
        $(settings.balancePlaceholderSelector).html settings.spinnerHtml      

    showStub: =>
        @shouldShowSpinner false
        $(settings.balancePlaceholderSelector).html i18n.t(settings.stubKey)    

    getBalance: (successHandler, failHandler) =>
        $.getJson settings.balanceApiUrl
        .done (response) =>
            if response.success
                successHandler response.balance
            else failHandler()
        .fail (jqXHR) =>
            failHandler()

    updateBalance: =>
        if @isAnyPlaceholderPresent() 
            @showSpinner()
            @getBalance \
                (balance) => 
                    @currentBalance = balance
                    @showBalance()
                    @showCurrencyCode()
                    @showCurrencySymbol()
                ,() =>
                    @showSpinner()
                    currentAttempt++;
                    if currentAttempt < settings.maxAttempts
                        delay = settings.initialDelay * currentAttempt
                        setTimeout @updateBalance, delay 
                    else
                        @showStub()         

    isAnyPlaceholderPresent: =>
        $(settings.balancePlaceholderSelector).length > 0      

balanceView = new BalanceViewModel()
balanceView.showSpinner()
balanceView.updateBalance()
