﻿define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    picker = require "datePicker"
    config = require "config"
    moment = require "moment"

    class ViewModel
        constructor: ->
            @initializeViewModel()
        
        initializeViewModel: () ->
            @SavePath = config.adminApi("PlayerInfo/UploadId")
            @playerId = ko.observable()
            @message = ko.observable()
            @messageClass = ko.observable()
            @submitted = ko.observable()
            
            @licensee = ko.observable()
            .extend
                required: true
            
            @brand = ko.observable()
            .extend
                required: true
            
            @documentType = ko.observable()
            .extend
                required: true
            
            @username = ko.observable()
            
            @documentTypes = ko.observableArray()
            
            @isIdOrCredit = ko.computed =>
                @documentType() == "Id" || @documentType() == "CreditCard"
                
            @firstFileUploaderLabel = ko.computed =>
                if @isIdOrCredit
                    return i18N.t "playerManager.identificationDocuments.labels.idFrontUpload"
                    
                return "Upload Document"
            
            @cardNumber = ko.observable()
            .extend
                required: {
                    onlyIf: @isIdOrCredit
                },
                minLength: 1,
                maxLength: 20,
                pattern: {
                    message: i18N.t("app:common.validationMessages.onlyNumeric").replace("__fieldName__", i18N.t("app:playerManager.identificationDocuments.labels.cardNumber")),
                    params: '^[0-9]+$'
                }

            @cardExpirationDate = ko.observable()
            @cardExpirationDate.extend
                required: {
                    onlyIf: @isIdOrCredit
                },
                validation: {
                    validator: (val, isIdOrCredit) =>
                        if @isIdOrCredit()
                            return moment().diff(val) < 0
                        yes
                    message: 'Expiration Date is not valid.',
                    params: @isIdOrCredit
                }

            @uploadId1FieldId = ko.observable("deposit-confirm-upload-id-1")
            @uploadId2FieldId = ko.observable("deposit-confirm-upload-id-2")
            defaultImagePreviewSrc = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHRleHQtYW5jaG9yPSJtaWRkbGUiIHg9IjE1MCIgeT0iMTAwIiBzdHlsZT0iZmlsbDojYWFhO2ZvbnQtd2VpZ2h0OmJvbGQ7Zm9udC1zaXplOjE5cHg7Zm9udC1mYW1pbHk6QXJpYWwsSGVsdmV0aWNhLHNhbnMtc2VyaWY7ZG9taW5hbnQtYmFzZWxpbmU6Y2VudHJhbCI+MzAweDIwMDwvdGV4dD48L3N2Zz4=";
            @uploadId1Src = ko.observable(defaultImagePreviewSrc)
            @uploadId2Src = ko.observable(defaultImagePreviewSrc)
           
            @maxSize = 4194304
           
            @idFrontImage = ko.observable()
            .extend
                required: true
                validation: {
                    validator: (val) =>
                        element = $('input#' + @uploadId1FieldId())[0]
                        file = if element then element.files[0] else undefined
                        file && (file.size <= @maxSize)
                    message: 'Maximum file size is 4Mb.'
                }
                
            @idBackImage = ko.observable()
            .extend
                validation: [{
                    validator: (val) =>
                        element = $('input#' + @uploadId2FieldId())[0]
                        file = if element then element.files[0] else undefined
                        
                        if file == undefined
                            return yes
                        
                        file.size <= @maxSize
                    message: 'Maximum file size is 4Mb.'
                }, {
                    validator: (val, isIdOrCredit) =>
                        if isIdOrCredit()
                            element = $('input#' + @uploadId2FieldId())[0]
                            file = if element then element.files[0] else undefined
                        
                            if file == undefined
                                return no
                                
                            yes
                            
                        yes
                    message: 'Both sides of the documents should be uploaded.',
                    params: @isIdOrCredit
                }]
           
            @remarks = ko.observable()
            .extend
                maxLength: 200
           
            @errors = ko.validation.group @
           
        compositionComplete: (data) ->
            $('input#' + @uploadId1FieldId()).ace_file_input(@makeFileInputSettings(@idFrontImage));
            $('input#' + @uploadId2FieldId()).ace_file_input(@makeFileInputSettings(@idBackImage));
            
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
                        resetOb("")
                        alert(i18N.t("app:payment.deposit.pleaseSelectImage"))
                        -1
                else
                    type = $.trim(file.type)
                    if ((type.length > 0 && !(/^image\/(jpe?g|png|gif)$/i).test(type)) || (type.length == 0 && !(/\.(jpe?g|png|gif)$/i).test(file.name)))
                        resetOb("")
                        alert(i18N.t("app:payment.deposit.pleaseSelectImage"))
                        -1
                true

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
        
        clear : () ->
            @clearMessage()
            @licensee('')
            @licensee.isModified(no)
            @brand('')
            @brand.isModified(no)
            @documentType('')
            @documentType.isModified(no)
            @cardNumber('')
            @cardNumber.isModified(no)
            @cardExpirationDate('')
            @cardExpirationDate.isModified(no)
            @idFrontImage('')
            @idFrontImage.isModified(no)
            @idBackImage('')
            @idBackImage.isModified(no)
            @remarks('')
            @remarks.isModified(no)
            
        activate: (data) ->
            @playerId data.playerId
            @clear()
            @submitted off
            @load()
        
        load: ()->
            $.get config.adminApi("PlayerInfo/GetIdentificationDocumentEditData?id=" + @playerId())
                .done (data) =>
                    @licensee data.licenseeName
                    @brand data.brandName
                    @username data.username
                    @documentTypes data.documentTypes
        
        forceValidation :=>
            @idBackImage @idBackImage()
            @idFrontImage @idFrontImage()
        
        save: ->
            @clearMessage()
            @forceValidation()
            
            if @isValid()
                model = {
                 documentType: @documentType(),
                 cardNumber: @cardNumber(),
                 cardExpirationDate: @cardExpirationDate(),
                 remarks: @remarks()
                }

                fd = new FormData()
                fd.append('uploadId1', $('input#' + @uploadId1FieldId())[0].files[0])
                fd.append('uploadId2', $('input#' + @uploadId2FieldId())[0].files[0])
                fd.append('data', JSON.stringify(model))
                fd.append('playerId', @playerId())

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
                                        $('#id-documents-grid').trigger 'reload'
                            
                                        if response.data.frontIdFilename
                                            @uploadId1Src('image/Show?fileId=' + response.data.frontIdFilename + '&playerId=' + @playerId());

                                        if response.data.backIdFilename
                                            @uploadId2Src('image/Show?fileId=' + response.data.backIdFilename + '&playerId=' + @playerId());
                            
                                        @showMessage("Documents have been uploaded successfully.")
                                        @submitted true

                                    $('input#' + @uploadId1FieldId()).data().ace_file_input.disable()
                                    $('input#' + @uploadId2FieldId()).data().ace_file_input.disable()
                            return req;
            else
                @errors.showAllMessages()
                
    new ViewModel()