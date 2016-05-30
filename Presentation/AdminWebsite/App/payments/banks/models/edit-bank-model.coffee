define (require) ->
    i18n = require "i18next"    
    config = require "config"
    common = require "payments/banks/common"
    regex = require "regular-expression"
    baseModel = require "base/base-model"

    class AddBankModel extends baseModel
        constructor: (bank)->
            super

            @id = @makeField(bank.id)
            
            @licenseeId = @makeField().extend
                required: true
            
            @brandId = @makeField().extend
                required: true
                
            @bankId = @makeField(bank.bankId)

            @bankName = @makeField(bank.bankName).extend
                required: true
                maxLength: common.bankNameMaxLength
                pattern:
                    message: i18n.t "banks.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
            
            @countryCode = @makeField().extend
                required: true

            @remarks = @makeField(bank.remarks).extend
                required: true
                maxLength: common.remarksMaxLength

            @licensees = @makeSelect()

            @brands = @makeSelect()

            @countries = @makeSelect()

            $.ajax "/Licensee/Licensees?useFilter=false"
                .done (response) =>
                    @licensees response.licensees
                    @licenseeId bank.licenseeId
                    $.ajax
                        url: config.adminApi "Brand/Brands?useFilter=false&licensees=" + bank.licenseeId
                        context: @
                    .done (response) =>
                        @brands response.brands
                        @brandId bank.brandId
                        $.ajax
                            url: config.adminApi "Brand/GetCountries?brandId=" + bank.brandId
                            context: @
                        .done (response) ->
                            @countries response.countries
                            @countryCode bank.countryCode
                            @licenseeId.subscribe (licenseeId) =>
                                @licenseeId licenseeId
                                $.ajax
                                    url: config.adminApi "Brand/Brands?useFilter=false&licensees=" + licenseeId
                                    context: @
                                .done (response) ->
                                    @brands response.brands
                            @brandId.subscribe (brandId) =>
                                @brandId brandId
                                $.ajax
                                    url: config.adminApi "Brand/GetCountries?brandId=" + brandId
                                    context: @
                                .done (response) ->
                                    @countries response.countries
                                    
        mapto: -> 
            ignoreFields = ["licensees", "licenseeId", "brands", "countries", "bankId"]
            super ignoreFields