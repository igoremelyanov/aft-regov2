define ['nav', "i18next"], (nav, i18n) ->
    class CreateOfflineDepositRequest
        constructor: ->
            [@message, @messageClass, @playerId, @username, @selectedBankAccount, @selectedBonus] = ko.observables()
            @disable = ko.observable no
            @banks = ko.observableArray()

            @amount = ko.observable().extend
                formatDecimal: 2
                validatable: yes
                required: yes
                min:
                    message: "Entered amount must be greater than 0."
                    params: 0.01
                max:
                    message: "Entered amount is bigger than allowed."
                    params: 2147483647

            noneBonus = 
                id: null
                name: i18n.t("app:common.none")
                code: null
            @bonuses = ko.observableArray [ noneBonus ]
            @selectedBonus noneBonus
            @bonusFormatter = (bonus) ->  
                if bonus.code? then "#{bonus.code}: #{bonus.name}" else "#{bonus.name}"

            @errors = ko.validation.group @
            @IsJsonString = (str) ->
                try
                    JSON.parse str
                    return yes
                catch e
                    return no
            
        close: -> nav.close()
        activate: (data) ->
            $.get '/offlineDeposit/GetInfoForCreate', playerId: data.playerId
                .done (response) =>
                    banks = response.data.banks
                    @banks banks
                    if banks.length is 0
                        @disable yes
                        @message i18n.t "app:payment.paymentLevelDisableOfflineDeposit"
                        @messageClass "alert-danger"
                    @username response.data.username
                    @playerId data.playerId
                    response.data.bonuses.forEach (bonus) => @bonuses.push bonus

        sendRequest: ->
            if @isValid()
                @message null
                @messageClass null
                $.post '/offlineDeposit/Create',
                    playerId: @playerId()
                    bankAccountId: @selectedBankAccount().id
                    bonusId: @selectedBonus().id
                    amount: @amount()
                .done (response) => 
                    if response.result is "failed"
                        if @IsJsonString response.data
                            error = JSON.parse(response.data)
                            @message(i18n.t("app:payment.deposit.depositFailed") + i18n.t(error.text, error.variables))
                        else
                            @message(i18n.t("app:payment.deposit.depositFailed") + i18n.t(response.data))
                        @messageClass "alert-danger"
                    else
                        $('#offline-deposit-confirm-grid').trigger 'reload'
                        @close()
                        nav.open
                            path: 'player-manager/offline-deposit/view-request'
                            title: i18n.t "app:payment.offlineDepositRequest.view"
                            data:
                                hash: '#offline-deposit-view',
                                requestId: response.data,
                                message: i18n.t "app:payment.deposit.successfullyCreated"

            else
                @errors.showAllMessages()