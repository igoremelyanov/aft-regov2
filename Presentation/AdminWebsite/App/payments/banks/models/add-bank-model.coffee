define (require) ->
    i18n = require "i18next"    
    config = require "config"
    common = require "payments/banks/common"
    regex = require "regular-expression"
    baseModel = require "base/base-model"

    class AddBankModel extends baseModel
        constructor: ->
            super

            $.get "/Licensee/Licensees?useFilter=true"
                .done (data) =>
                    @licensees data.licensees

            @licensees = @makeSelect()

            @licenseeId = ko.observable().extend
                required: true

            @licenseeId.subscribe (licenseeId) =>
                self = @
                if licenseeId?
                    $.get config.adminApi "Brand/Brands?useFilter=true&licensees=" + licenseeId
                    .done (response) ->
                        self.brands response.brands

            @brands = @makeSelect()

            @brandId = @makeField().extend
                required: true

            @brandId.subscribe (brandId) =>
                self = @
                if brandId?
                    $.get config.adminApi "Brand/GetCountries?brandId=" + brandId
                    .done (response) ->
                        self.countries response.countries

            @countries = @makeSelect()
            
            @countryCode = @makeField().extend
                required: true

            @bankId = @makeField().extend
                required: true
                maxLength: common.bankIdMaxLength

            @bankName = @makeField().extend
                required: true
                maxLength: common.bankNameMaxLength
                pattern:
                    message: i18n.t "banks.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace

            @remarks = @makeField().extend
                required: true
                maxLength: common.remarksMaxLength

        mapto: -> 
            ignoreFields = ["licensees", "licenseeId", "brands", "countries"]
            super ignoreFields