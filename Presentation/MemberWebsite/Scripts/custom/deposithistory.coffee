class DepositHistory extends FormBase
    constructor: ->
        super

        @depositId = ko.observable()
        @depositId.subscribe (val)=>
            $.get "/api/GetBankAccountForOfflineDeposit?offlineDepositId=" + val, (data) =>
                @bankAccountIdSettings data

        @bankAccountIdSettings = ko.observable()
        @playerHasValidIdDocuments = ko.observable()

        @startdate = ko.observable('')
        @enddate = ko.observable('')
        @type = ko.observable('')

        @transferMethod = ko.observable('')
        @transferBank = ko.observable('')
        @playerAccountName = ko.observable('')
        @playerAccountName.subscribe (val)=>
            $.blockUI()
            $.get '/api/IsDepositorsFullNameValid?name=' + val, (data)=>
                @isFullNameEqualsPlayersFullName data
                $.unblockUI()
        
        @isFullNameEqualsPlayersFullName = ko.observable no
           
        #@isIdLoaded = ko.computed ()=>
        #    return (@idBack() && @idFront())
            
        @isReceiptRequiredPaymentLevelCriteria = ko.computed ()=>
            bankSettings = @bankAccountIdSettings()
            transferMethod = @transferMethod()
            transferBank = @transferBank()
            
            if bankSettings == undefined
                return no
            
            # transferMethod: InternetBanking, CounterDeposit, ATM
            # transferBank: SameBank, DifferentBank
            if (transferMethod == 'InternetBanking' && transferBank == 'SameBank')
                return bankSettings.internetSameBank
            else if (transferMethod == 'CounterDeposit' && transferBank == 'SameBank')
                return bankSettings.counterDepositSameBank
            else if (transferMethod == 'ATM' && transferBank == 'SameBank')
                return bankSettings.atmSameBank
            else if (transferMethod == 'InternetBanking' && transferBank == 'DifferentBank')
                return bankSettings.internetDifferentBank
            else if (transferMethod == 'CounterDeposit' && transferBank == 'DifferentBank')
                return bankSettings.counterDepositDifferentBank
            else if (transferMethod == 'ATM' && transferBank == 'DifferentBank')
                return bankSettings.atmDifferentBank
            
        @isReceiptRequired = ko.computed ()=>
            if @playerAccountName() == '' || @playerAccountName() == undefined
                return no
        
            if @isReceiptRequiredPaymentLevelCriteria() == yes
                return yes;
            
            if @playerHasValidIdDocuments() == no
                return yes
            
            if @isFullNameEqualsPlayersFullName() == no
                return yes
            
            return no
            
        @isIdRequired = ko.computed ()=>
            if @playerAccountName() == '' || @playerAccountName() == undefined
                return no
        
            if @playerHasValidIdDocuments() == no
                return yes
            
            if @isFullNameEqualsPlayersFullName() == no
                return yes
            
            return no
                
        @depositReceipt = ko.observable('').extend
          validation: {
                    validator: (val) =>
                        element = $('input#depositReceipt')[0]
                        file = if element then element.files[0] else undefined
                        if file
                            return file.size <= @maxSize
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }
                
        @idFront = ko.observable('').extend
          validation: [{
                    validator: (val) =>
                        element = $('input#idFront')[0]
                        file = if element then element.files[0] else undefined
                        if file
                            return file.size <= @maxSize 
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }]
                
        @idBack = ko.observable('').extend
          validation: [{
                    validator: (val) =>
                        element = $('input#idBack')[0]
                        file = if element then element.files[0] else undefined
                        if file
                            return file.size <= @maxSize
                        else
                            true
                    message: 'Maximum file size is 4Mb.'
                }]
                
        @amount = ko.observable()
        @remark = ko.observable('')
        @maxSize = 4194304
        
        $.blockUI()
        $.get "/api/ArePlayersIdDocumentsValid", (data) =>
            @playerHasValidIdDocuments data
            $.unblockUI()
           
        @submitFilter = ->
          $.postJson '/api/FilterDepositHistory',
            start: @startdate
            end: @enddate
            type: @type
          .success (response) =>
            if response.hasError
              $.each response.errors, (propName)=>
                observable = @[propName]
                observable.error = 'error message'
                observable.__valid__ no
            else
              popupAlert('', '')
            
            return no
        
        @selectDepositForConfirm = (id) ->
            @depositId id
        
        @submitDepositConfirmation= ->
            serializedForm = $('#confirm-deposit-history-id').serializeArray()
        
            depositConfirm = {
                Id: @depositId(),
                PlayerAccountName: @playerAccountName(),
                PlayerAccountNumber: null,
                #ReferenceNumber: @bankReferenceNumber(),
                TransferType: @transferBank(), #same different
                OfflineDepositType: @transferMethod(), # counter, atm, internet
                Amount: @amount(),
                Remark: @remark(),
                IdFrontImage: @idFront(),
                IdBackImage: @idBack(),
                ReceiptImage: @depositReceipt()
            }
        
            $.ajax
                url: '/api/ValidateConfirmDepositRequest'
                type: 'post',
                data: depositConfirm
            .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        localizedError = i18n.t "apiResponseCodes." + response.errors[propName]
                        if observable
                            observable.error = localizedError
                            observable.__valid__ no
                            observable.isModified yes
                        else
                            popupAlert('Error', localizedError)
                else
                    xhr = new XMLHttpRequest()
                
                    xhr.onreadystatechange = (e) =>
                        if (4 == xhr.readyState)
                            response = JSON.parse(xhr.responseText)
                            if response.result == "failed"
                                popupAlert('Error', 'Error occured. Contact an administrator.')
                            else
                                redirect("/Home/DepositHistoryConfirmation?depositId=" + @depositId())
                
                    xhr.open('post', '/api/ConfirmOfflineDeposit', true)
                
                    fd = new FormData()
                
                    fd.append('uploadId1', $('#idFront')[0].files[0])
                    fd.append('uploadId2', $('#idBack')[0].files[0])
                    #fd.append('receiptUpLoad', $('#idReceipt')[0].files[0])
                    
                    fd.append('depositConfirm', JSON.stringify(depositConfirm))

                    xhr.send(fd)
                
        @triggerInputFile = (target) ->
          target = $('#' + target)
          if target.parent().hasClass('upload')
            target.click()
                
        @deleteFile = (target) ->
            $('#' + target)
              .val ''
              .parent().parent()
                .find('img').attr('src','/Content/images/icon_id.png')
                .end().end().end()
              .parent()
                .removeClass 'uploaded'
                .addClass 'upload'
                .find('button').hide().removeClass 'show'
              
            @[target](null)
            return no
            
    ko.bindingHandlers.fileUpload =
      init: (element, valueAccessor) ->
        $(element)
          .change ->
            valueAccessor()(element.files[0].name)
            $(element).parent()
              .removeClass 'upload'
              .addClass 'uploaded'
              .find('button').show().end()
              .find('input').eq(0).blur()
    
    $('#deposit-confirmation').one 'hidden.bs.modal', ()=>
       window.location.href = window.location.href # hook, the way to clear viewmodel
    
    model = new DepositHistory
    model.errors = ko.validation.group(model)
    ko.applyBindings model, document.getElementById "cashier-wrapper"