define ["nav", "i18next", "komapping"], (nav, i18n, mapping) ->
    class ViewOfflineDepositRequest
        constructor: ->
            [@message, @username, @amount, @bankName, @selectedBankAccount, @bankAccountId,
            @bankProvince, @bankBranch, @bankAccountName, @bankAccountNumber, 
            @playerRemark, @unverifyReason, @status, @transactionNumber, @bonusName] = ko.observables()
            
        close: -> nav.close()
        activate: (data) ->
            @message data.message
            $.get "/offlineDeposit/GetForView", id: data.requestId
                .done (response) =>
                    mapping.fromJS response.data, {}, @
                    if @bonusName() is null
                        @bonusName i18n.t "common.none"