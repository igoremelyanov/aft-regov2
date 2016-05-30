# CoffeeScript
define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    picker = require "dateTimePicker"
    baseModel = require "base/base-view-model"
    editBankAccountModel = require "payments/bank-accounts/models/edit-bank-account-model"
    
    class EditViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/BankAccounts/SaveChanges"
            @submitted off
            @message = ko.observable()
            @messageClass = ko.observable()
            @tmp = ko.observable();
            
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:banks.validation." + error
                    
        save: () ->
            @clearMessage()
            
            if @Model.validate()
                bankAccount = {
                    id: @Model.id(),
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
                    expirationDate: @Model.expirationDate(),
                    uploadId1Src:  @Model.uploadId1Src(),
                    uploadId2Src:  @Model.uploadId2Src(),
                    uploadId3Src:  @Model.uploadId3Src()
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
                                                message: i18n.t "app:bankAccounts.updated"
                                        $('#bank-accounts-list').trigger 'reloadGrid'
                                    
                                    
                                    if response.data.idFrontImage
                                        @Model.uploadId1Src('image/Show?fileId=' + response.data.idFrontImage + '&playerId=' + @Model.id());
                                    if response.data.idBackImage
                                        @Model.uploadId2Src('image/Show?fileId=' + response.data.idBackImage + '&playerId=' + @Model.id());
                                    if response.data.atmCardImage
                                        @Model.uploadId3Src('image/Show?fileId=' + response.data.atmCardImage + '&playerId=' + @Model.id());
                                    
                                                                        
                                    @showMessage("Updated successfully.")
                                    @submitted true


                            return req
            else
                @showError(i18n.t "app:bankAccounts.notValid")

        
        activate: (data) =>
            super
            $.get "/BankAccounts/Edit?id=" + data.id
                .done (response) =>
                    @Model = new editBankAccountModel(response)
                    
                    if response.idFrontImage
                        @Model.uploadId1Src('image/Show?fileId=' + response.idFrontImage + '&playerId=' + @Model.id());
                    
                    if response.idBackImage                        
                        @Model.uploadId2Src('image/Show?fileId=' + response.idBackImage + '&playerId=' + @Model.id());
                        
                        
                    if response.atmCardImage                        
                        @Model.uploadId3Src('image/Show?fileId=' + response.atmCardImage + '&playerId=' + @Model.id());

                    
        compositionComplete: (data) ->
            $('input#' + @Model.uploadId1FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.idFrontImage))
            $('input#' + @Model.uploadId2FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.idBackImage))
            $('input#' + @Model.uploadId3FieldId()).ace_file_input(@Model.makeFileInputSettings(@Model.atmCardImage))


        showError: (msg) ->
            @message msg
            @messageClass 'alert alert-danger'
             
        showMessage: (msg) ->   
            @message msg
            @messageClass 'alert alert-success'
            
        clearMessage: () ->
            @message ''
            @messageClass ''
        
        cancel: ->
           nav.close()          