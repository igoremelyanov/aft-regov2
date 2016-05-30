# CoffeeScript
define (require) ->
    i18n = require "i18next"    
    picker = require "datePicker"
    config = require "config"
    common = require "payments/bank-accounts/common"
    regex = require "regular-expression"
    baseModel = require "base/base-model"
    moment = require "moment"
    
    class EditBankAccountModel extends baseModel
        constructor: (bankAccount)->
            super
            
            @submitted = ko.observable()

            @id = @makeField(bankAccount.id)
            
            @isActive = @makeField(bankAccount.isActive)
            
            @licenseeId = @makeField().extend
                required: true
            
            @licenseeName = @makeField(bankAccount.licenseeName)
            
            @brandId = @makeField().extend
                required: true
            
            @brandName = @makeField(bankAccount.brandName)    
                
            @bankId = ko.observable(bankAccount.bankId).extend
                required: true
            
            @bankAccountAccountTypeId = ko.observable(bankAccount.bankAccountAccountTypeId).extend
                required: true
            
            @bankAccountAccountTypes = @makeSelect()  
            
            
            @bankName = @makeField(bankAccount.bankName)  
            
            @banks = @makeSelect()
            
            @currencyCode = @makeField(bankAccount.currencyCode).extend
                required: true
                
            @remarks = @makeField(bankAccount.remarks).extend
                required: true
                maxLength: common.remarksMaxLength
                
            @bankAccountId = @makeField(bankAccount.bankAccountId).extend
                required: true
                maxLength: common.bankIdMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
       
            @bankAccountNumber = @makeField(bankAccount.bankAccountNumber).extend
                required: true
                maxLength: common.bankAccountNumberMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
                
            @bankAccountName = @makeField(bankAccount.bankAccountName).extend
                required: true  
                maxLength: common.bankAccountNameMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace              

            @bankAccountProvince = @makeField(bankAccount.bankAccountProvince).extend
                required: true
                maxLength: common.bankAccountProvinceMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
       
            @bankAccountBranch = @makeField(bankAccount.bankAccountBranch).extend
                required: true
                maxLength: common.bankAccountBranchMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.AlphanumericDashUnderscoreSpace"
                    params: regex.alphaNumericDashUnderscoreSpace
                
            
            @supplierName = @makeField(bankAccount.supplierName).extend
                required: true
                maxLength: common.supplierNameMaxLength
       
            @contactNumber = @makeField(bankAccount.contactNumber).extend
                required: true
                maxLength: common.contactNumberMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.Numeric"
                    params: regex.numeric
                
            @usbCode = @makeField(bankAccount.usbCode).extend
                required: true  
                maxLength: common.usbCodeMaxLength
                pattern:
                    message: i18n.t "bankAccounts.validation.Alphanumeric"
                    params: regex.alphaNumeric
                
            @purchasedDate = @makeField(bankAccount.purchasedDate).extend
                required: true

            @cardPurchasedDate = @makeField()
            @cardPurchasedDate.extend {
                validation: {
                    validator: (val) =>
                        moment().diff(@cardPurchasedDate()) < 0
                        yes
                    message: 'Expiration Date is not valid.'
                }
            }

            @utilizationDate = @makeField(bankAccount.utilizationDate).extend
                required: true
                
            @expirationDate = @makeField(bankAccount.expirationDate).extend
                required: true
                
            @uploadId1FieldId = ko.observable("bank-account-upload-id-1")
            @uploadId2FieldId = ko.observable("bank-account-upload-id-2")
            @uploadId3FieldId = ko.observable("bank-account-upload-id-3")
            defaultImagePreviewSrc = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHRleHQtYW5jaG9yPSJtaWRkbGUiIHg9IjE1MCIgeT0iMTAwIiBzdHlsZT0iZmlsbDojYWFhO2ZvbnQtd2VpZ2h0OmJvbGQ7Zm9udC1zaXplOjE5cHg7Zm9udC1mYW1pbHk6QXJpYWwsSGVsdmV0aWNhLHNhbnMtc2VyaWY7ZG9taW5hbnQtYmFzZWxpbmU6Y2VudHJhbCI+MzAweDIwMDwvdGV4dD48L3N2Zz4=";
            
            @uploadId1Src = ko.observable()
            @uploadId2Src = ko.observable()
            @uploadId3Src = ko.observable()
            
            @maxSize = 4194304
            
            @idFrontImage =  @makeField().extend
                #required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId1FieldId())[0]
                        if element
                            file = if element then element.files[0] else undefined
                            if file
                                file && (file.size <= @maxSize)
                            else
                                true
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }
                
            @idBackImage =  @makeField().extend
                #required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId2FieldId())[0]
                        if element
                            file = if element then element.files[0] else undefined
                            if file
                                file && (file.size <= @maxSize)
                            else
                                true
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }
            
            @atmCardImage =  @makeField().extend
                #required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId3FieldId())[0]
                        if element
                            file = if element then element.files[0] else undefined
                            if file
                                file && (file.size <= @maxSize)
                            else
                                true
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }

            @file1Src = ko.computed =>
                @idFrontImage() || @uploadId1Src()
            @file1Src 
                .extend
                    required: true
                    validation: 
                        validator: (val) =>
                            val
                        message: 'ID Front required'
                        params: on            
           
            @file2Src = ko.computed =>
                @idBackImage() || @uploadId2Src()
            @file2Src 
                .extend
                    required: true
                    validation: 
                        validator: (val) =>
                            val
                        message: 'ID Back required'
                        params: on              
            
            @file3Src = ko.computed =>
                @atmCardImage() || @uploadId3Src()
            @file3Src 
                .extend
                    required: true
                    validation: 
                        validator: (val) =>
                            val
                        message: 'ATM card required'
                        params: on              
            
            @licensees = @makeSelect()

            @brands = @makeSelect()

            @currencies = @makeSelect()
            
            @arr = ko.observableArray()

            $.ajax "BankAccounts/GetBankAccountTypes"
                .done (data) =>
                    @bankAccountAccountTypes data.bankAccountTypes
                    @bankAccountAccountTypeId bankAccount.bankAccountAccountTypeId
            
            
            $.ajax "/Licensee/Licensees?useFilter=false"
                .done (response) =>
                    @licensees response.licensees
                    @licenseeId bankAccount.licenseeId
                    $.ajax
                        url: config.adminApi "Brand/Brands?useFilter=false&licensees=" + bankAccount.licenseeId
                        context: @
                        self = @
                    .done (response) =>
                        @brands response.brands
                        @brandId bankAccount.brandId
                        $.get "BankAccounts/GetBanks?brandId=" + bankAccount.brandId
                        .done (response) ->
                            self.banks response.banks
                            self.bankId bankAccount.bankId
                        $.ajax
                            url: config.adminApi "BrandCurrency/GetBrandCurrenciesWithNames?brandId=" + bankAccount.brandId
                            context: @
                        .done (response) ->
                            @currencies response.currencyCodes
                            @currencyCode bankAccount.currencyCode
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
                                    url: config.adminApi "BrandCurrency/GetBrandCurrenciesWithNames?brandId=" + brandId
                                    context: @
                                .done (response) ->
                                    @currencies response.currencyCodes
                                $.get "BankAccounts/GetBanks?brandId=" + brandId
                                .done (response) ->
                                    self.banks response.banks
                                   
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
                    type = $.trim(file.type)
                    if ((type.length > 0 && !(/^image\/(jpe?g|png|gif)$/i).test(type)) || (type.length == 0 && !(/\.(jpe?g|png|gif)$/i).test(file.name)))
                        #resetOb("")
                        -1
                true
                
            after_change: (files, dropped) ->
                alert(files)
                alert(dropped)
                true
            
            before_remove: () ->
                #@idBackImage = null 
                #@uploadId2Src = null
                resetOb('')
                true

            
        mapto: -> 
            ignoreFields = ["licensees", "licenseeId", "brands", "currencies", "bankAccountAccountTypes"]
            super ignoreFields
