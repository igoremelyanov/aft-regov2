# CoffeeScript
define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    picker = require "dateTimePicker"
    baseModel = require "base/base-view-model"
    addBankAccountModel = require "payments/bank-accounts/models/add-bank-account-model"

    class AddViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/BankAccounts/Add"
            @message = ko.observable()
            @messageClass = ko.observable()
            @Model = new addBankAccountModel()
            
        compositionComplete: () ->
            $('input#' + @Model.uploadId1FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.idFrontImage()))
            $('input#' + @Model.uploadId2FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.idBackImage()))
            $('input#' + @Model.uploadId3FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.atmCardImage()))
        
        handleSaveFailure: (response) ->
            alert("failure")
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @showError(@Model[field.name], i18n.t "app:banks.validation." + error)

        save: ->
            @clearMessage()
            
            if @Model.validate()
                bankAccount = {
                    bank: @Model.bankId(),
                    brandId: @Model.brandId(),
                    licenseeId: @Model.licenseeId(),
                    currency: @Model.currencyCode(),
                    accountId: @Model.bankAccountId(),
                    accountName: @Model.bankAccountName(),
                    accountNumber: @Model.bankAccountNumber(),
                    accountType: @Model.bankAccountAccountTypeId(),
                    province: @Model.bankAccountProvince(),
                    branch: @Model.bankAccountBranch(),
                    remarks: @Model.remarks(),
                    supplierName: @Model.supplierName(),
                    contactNumber: @Model.contactNumber(),
                    uSBCode: @Model.usbCode(),
                    purchasedDate: @Model.purchasedDate(),
                    utilizationDate: @Model.utilizationDate(),
                    expirationDate: @Model.expirationDate()
                }            
            
                fd = new FormData()
                fd.append('uploadId1', $('input#' + @Model.uploadId1FieldId())[0].files[0])
                fd.append('uploadId2', $('input#' + @Model.uploadId2FieldId())[0].files[0])
                fd.append('uploadId3', $('input#' + @Model.uploadId3FieldId())[0].files[0])
                fd.append('bankAccount', JSON.stringify(bankAccount))
                
                $.ajax
                        type: "POST"
                        url: @SavePath
                        data: fd
                        processData: false,
                        contentType: false,
                        xhr: () =>
                            req = new XMLHttpRequest();
                            req.onreadystatechange = (e) =>
                                if (4 == req.readyState)
                                    response = JSON.parse(req.responseText)
                                    if response.result == "failed"
                                        @showError response.data
                                    else
                                        nav.close()
                                        nav.open
                                            path: "payments/bank-accounts/view"
                                            title: i18n.t "app:banks.viewAccount"
                                            key: response.data.id
                                            data: 
                                                id: response.data.id
                                                message: i18n.t "app:bankAccounts.created"
                                        $("#bank-accounts-list").trigger "reloadGrid"
                                    

                            return req
                
            else
                @showError("notValid")     
            
           
        showError: (msg) ->
            @message i18n.t "app:bankAccounts." + msg
            @messageClass 'alert alert-danger'
             
        showMessage: (msg) ->   
            @message msg
            @messageClass 'alert alert-success'
            
        clearMessage: () ->
            @message ''
            @messageClass ''
        
        cancel: ->
           nav.close() 
   