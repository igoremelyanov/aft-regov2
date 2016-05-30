define (require) ->
    i18n = require "i18next"
    baseViewModel = require "base/base-view-model"
    viewBankModel = require "payments/banks/models/view-bank-model"

    class ViewModel extends baseViewModel
        constructor: ->
            super

        activate: (data) =>
            super
            $.get "/Banks/View?id=" + data.id
                .done (response) =>
                    @Model = new viewBankModel()
                    @Model.licenseeName response.data.licenseeName
                    @Model.brandName response.data.brandName
                    @Model.bankId response.data.bankId
                    @Model.name response.data.bankName
                    @Model.country response.data.countryName
                    @Model.remarks response.data.remarks
                    @Model.message data.message if data.message