# CoffeeScript
define (require) ->
    baseModel = require "base/base-model"

    class ViewBankAccountModel extends baseModel
        constructor: (bankAccount)->
            super
            
            @message = @makeField()
            @id = @makeField(bankAccount.id)
            @licenseeName = @makeField(bankAccount.licenseeName)
            @brandName = @makeField(bankAccount.brandName)
            @currencyCode = @makeField(bankAccount.currencyCode)
            @bankAccountId = @makeField(bankAccount.bankAccountId)
            @bankName = @makeField(bankAccount.bankName)
            @bankAccountNumber = @makeField(bankAccount.bankAccountNumber)
            @bankAccountAccountName = @makeField(bankAccount.bankAccountAccountName)
            @bankAccountProvince = @makeField(bankAccount.bankAccountProvince)
            @bankAccountBranch = @makeField(bankAccount.bankAccountBranch)
            
            @bankAccountAccountType = @makeField(bankAccount.bankAccountAccountType)
            @bankAccountAccountTypeName = @makeField(bankAccount.bankAccountAccountTypeName)
            
            @supplierName = @makeField(bankAccount.supplierName)
            @contactNumber = @makeField(bankAccount.contactNumber)
            @usbCode = @makeField(bankAccount.usbCode)
            
            @purchasedDate = @makeField(bankAccount.purchasedDate)
            @utilizationDate = @makeField(bankAccount.utilizationDate)
            @expirationDate = @makeField(bankAccount.expirationDate)
            
            @uploadId1Src = @makeField(bankAccount.uploadId1Src)
            @uploadId2Src = @makeField(bankAccount.uploadId2Src)
            @uploadId3Src = @makeField(bankAccount.uploadId3Src)
            
            @remarks = @makeField(bankAccount.remarks)