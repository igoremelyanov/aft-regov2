# CoffeeScript
define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseViewModel = require "base/base-view-model"
    viewBankAccountModel = require "payments/bank-accounts/models/view-bank-account-model"
    
    class ViewModel extends baseViewModel
        constructor: ->
            super    
    
        activate: (data) =>
            super
            $.get "/BankAccounts/View?id=" + data.id
                .done (response) =>
                    @Model = new viewBankAccountModel(response)
                    @Model.message data.message if data.message