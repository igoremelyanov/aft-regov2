ko.validation.init({
    registerExtenders: true
}, true);

class BalanceInformationModel
    constructor: ->
        NotificationMethods = Email: "Email", SMS: "SMS"
        WithdrawalNotificationMethods = DoNotNotify: "Do not notify", Email: "Email", SMS: "SMS"

        @shownTab = ko.observable()

        # balance informaion
        @currency = ko.observable "N/A"
        @balanceWallet = ko.observable()
            
        #withdrawalRequest
        @playerBankAccountId = ko.observable ""
        @withdrawalAmount = ko.observable("").extend
            formatDecimal: 2
            validatable: true
            required: yes
            min:
                message: "Entered amount must be greater than 0."
                params: 0.01
            max:
                message: "Entered amount is bigger than allowed."
                params: 2147483647
        @withdrawalNotificationMethods = ko.observableArray (WithdrawalNotificationMethods[x] for x of WithdrawalNotificationMethods)
        @withdrawalNotificationMethod = ko.observable WithdrawalNotificationMethods.DoNotNotify
        @withdrawalRequestInProgress = ko.observable no
        @withdrawalRequestSuccess = ko.observable location.hash is "#withdrawalDetails/success"
        @withdrawalRequestErrors = ko.observableArray []
        
        # fund-in request
        @playerName = ko.observable "N/A"
        @transferFundType = ko.observable()
        @fundInWallet = ko.observable()
        @fundInBonusCode = ko.observable()
        @walletBalance = ko.observable("")
        
        @fundInWallet.subscribe () =>
            $.get "GetProductBalance", walletId:  @fundInWallet()
                .done (response) =>
                    @walletBalance response.main.toFixed(2)
                                    
        @fundInAmount = ko.observable("").extend
            formatDecimal: 2
            validatable: true
            required: yes
            min:
                message: "Entered amount must be greater than 0."
                params: 0.01
            max:
                message: "Entered amount is bigger than allowed."
                params: 2147483647
        @fundInRequestInProgress = ko.observable no
        @fundInSuccess = ko.observable ~location.hash.indexOf "#fundIn/success"
        @fundInTransferId = ko.observable location.hash.substr "#fundIn/success/".length
        @fundInErrors = ko.observableArray []
        
        $.extend @, new window.OfflineDepositModel()
        $.extend @, new window.OnlineDepositModel()
        $(window).on "hashchange", =>
            @toggleTab()
        
        setTimeout -> $("[type=number]").val ""
        
    IsJsonString: (str) ->
        try
            JSON.parse(str)
        catch e
            return false
        true
        
    loadProfile: =>
        $.getJson '/api/profile'
        .done (response) =>
            if response.success
                @currency response.profile.currencyCode
                @playerName "#{response.profile.firstName} #{response.profile.lastName}"

    submitWithdrawalRequest: =>
        @withdrawalRequestSuccess off
        @withdrawalRequestErrors []
        unless @withdrawalAmount.isValid()
            @withdrawalRequestErrors.push "Withdrawal failed. " + @withdrawalAmount.error
            return
        @withdrawalRequestInProgress yes
        
        accountId = $("#playerBankAccountId").text()
        accountTime = $("#playerBankAccountTime").text()
        bankTime = $("#playerBankTime").text()
        
        $.postJson '/api/offlineWithdrawal',
            PlayerBankAccountId: accountId
            Amount: @withdrawalAmount()
            NotificationType: @withdrawalNotificationMethod()
            BankTime: bankTime
            BankAccountTime: accountTime
        .done (response) =>
                location.href = "#withdrawalDetails/success"
                location.reload()
        .fail (jqXHR) =>
            response = JSON.parse jqXHR.responseText
            if IsJsonString(response.message)
                error = JSON.parse(response.message);
                @withdrawalRequestErrors.push 'Withdrawal failed. ' + i18n.t(error.text, error.variables)
            else if response.unexpected
                @withdrawalRequestErrors.push 'Withdrawal failed. '+'Unexpected error occurred.'
            else
                @withdrawalRequestErrors.push error for error in response.errors
                if response.errors.length is 0 and response.message
                    @withdrawalRequestErrors.push 'Withdrawal failed. ' + i18n.t(response.message)
        .always =>
            @withdrawalRequestInProgress no

    submitFundIn: =>
        @fundInSuccess off
        @fundInErrors []
        unless @fundInAmount.isValid()
            @fundInErrors.push "Withdrawal failed. " + @fundInAmount.error
            return
        @fundInRequestInProgress yes
        $.postJson '/api/fundIn',
            TransferFundType: @transferFundType()
            WalletId: @fundInWallet()
            Amount: @fundInAmount()
            BonusCode: @fundInBonusCode()
        .done (response) =>
            location.href = "#fundIn/success/" + response.transferId
            location.reload()
        .fail (jqXHR) =>
            response = JSON.parse jqXHR.responseText
            if response.unexpected
                @fundInErrors.push 'Unexpected error occurred.'
            else
                @fundInErrors.push error for error in response.errors
                if response.errors.length is 0 and response.message
                    @fundInErrors.push 'Transfer failed. ' + i18n.t(response.message)
        .always =>
            @fundInRequestInProgress no

    toggleTab: =>
        target = location.hash.substr(1) or "balance"
        target = target.substr 0, target.indexOf "/" if ~target.indexOf "/"
        @shownTab target

model = new BalanceInformationModel()
model.loadProfile()
model.toggleTab()

model.errors = ko.validation.group(model);
ko.applyBindings model, document.getElementById "balance-information-wrapper"