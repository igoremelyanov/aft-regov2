# CoffeeScript
define (require) ->
    i18n = require "i18next"    
    picker = require "datePicker"
    config = require "config"
    common = require "payments/bank-accounts/common"
    regex = require "regular-expression"
    baseModel = require "base/base-model"
    mapping = require "komapping"
    moment = require "moment"
    
    class AddBankAccountModel extends baseModel
        constructor: ()->
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
                    $.get config.adminApi "BrandCurrency/GetBrandCurrenciesWithNames?brandId=" + brandId
                    .done (response) ->
                        self.currencies response.currencyCodes
                    $.get "BankAccounts/GetBanks?brandId=" + brandId
                    .done (response) ->
                        self.banks response.banks
                       
            @currencies = @makeSelect()
            
            @currencyCode = @makeField().extend
                required: true
            
            @bankAccountId = @makeField().extend
                required: true
                maxLength: common.bankIdMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
                
            @bankId = ko.observable().extend
                required: true
            
            @banks = @makeSelect()
            
            @bankAccountNumber = @makeField().extend
                required: true
                maxLength: common.bankAccountNumberMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace         

            @bankAccountName = @makeField().extend
                required: true
                maxLength: common.bankAccountNameMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace 

            @bankAccountProvince = @makeField().extend
                required: true
                maxLength: common.bankAccountProvinceMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace 

            @bankAccountBranch = @makeField().extend
                required: true
                maxLength: common.bankAccountBranchMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace 


            
            $.get "BankAccounts/GetBankAccountTypes"
                .done (data) =>
                    @bankAccountAccountTypes data.bankAccountTypes
            
            @bankAccountAccountTypeId = ko.observable().extend
                required: true
            
            @bankAccountAccountTypes = @makeSelect()
            
            @supplierName = @makeField().extend
                required: true
                maxLength: common.supplierNameMaxLength

            @contactNumber = @makeField().extend
                required: true
                maxLength: common.contactNumberMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.Numeric"
                    params: regex.numeric                                                                         

            @usbCode = @makeField().extend
                required: true
                maxLength: common.usbCodeMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.Alphanumeric"
                    params: regex.alphaNumeric
                    
            @purchasedDate = @makeField().extend
                required: true

            @utilizationDate = @makeField().extend
                required: true
                
            @expirationDate = @makeField().extend
                required: true                                                                        
                                                                        
            @maxSize = 4194304
            
            @uploadId1FieldId = ko.observable("bank-account-upload-id-1")
            @uploadId1Src = ko.observable()
            @file1Src = ko.observable()
            @idFrontImage =  @makeField().extend
                required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId1FieldId())[0]
                        file = if element then element.files[0] else undefined
                        file && (file.size <= @maxSize)
                    message: 'Maximum file size is 4Mb.'
                }
            
            @uploadId2FieldId = ko.observable("bank-account-upload-id-2")
            @uploadId2Src = ko.observable()
            @file2Src = ko.observable()
            @idBackImage =  @makeField().extend
                required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId2FieldId())[0]
                        file = if element then element.files[0] else undefined
                        file && (file.size <= @maxSize)
                    message: 'Maximum file size is 4Mb.'
                }
            
            @uploadId3FieldId = ko.observable("bank-account-upload-id-3")
            @uploadId3Src = ko.observable()
            @file3Src = ko.observable()
            @atmCardImage =  @makeField().extend
                required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId3FieldId())[0]
                        file = if element then element.files[0] else undefined
                        file && (file.size <= @maxSize)
                    message: 'Maximum file size is 4Mb.'
                }
            
            @remarks = @makeField().extend
                required: true
                maxLength: common.remarksMaxLength
                
                
        makeFileInputSettings: (resetOb) ->
            style: 'well',
            btn_choose: "Drop image or click to choose",
            no_file: 'No File ...',
            droppable: true,
            thumbnail: 'fit',
            
            before_change: (files, dropped) -> 
                file = files[0]

                if (typeof file == "string")
                    if (!(/\.(jpe?g|png|gif)$/i).test(file))
                        #resetOb("")
                        -1
                else
                    if(file)
                        type = $.trim(file.type)
                        if ((type.length > 0 && !(/^image\/(jpe?g|png|gif)$/i).test(type)) || (type.length == 0 && !(/\.(jpe?g|png|gif)$/i).test(file.name)))
                            #resetOb("")
                            -1
                    true
                
            before_remove: () ->
                #resetOb(null)
                resetOb('')
                true
                
        mapto: -> 
            ignoreFields = ["licensees", "licenseeId", "brands", "currencies"]
            super ignoreFields
                    
          
            
            