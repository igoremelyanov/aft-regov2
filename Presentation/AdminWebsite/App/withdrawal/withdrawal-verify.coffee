define (require) ->
    i18n = require "i18next"
    nav = require "nav"
    withdrawalLogDialogViewModel = require "withdrawal/withdrawal-log-dialog"
            
    class ViewModel
        constructor: ->
            @id = ko.observable()
            
            #base info
            @licenseeName = ko.observable()
            @brandName = ko.observable()
            @username = ko.observable()
            @referenceCode = ko.observable()
            @status = ko.observable()
            @internalAccount = ko.observable()
            @currency = ko.observable()
            @paymentMethod = ko.observable()
            @amount = ko.observable()
            @submittedBy = ko.observable()
            @dateSubmitted = ko.observable()
            
            #bank information
            @bankName = ko.observable()
            @bankAccountName = ko.observable()
            @bankAccountNumber = ko.observable() 
            @branch = ko.observable()
            @swiftCode = ko.observable()
            @address = ko.observable()
            @city = ko.observable()
            @province = ko.observable()
            @remarks = ko.observable()

            #Which menu did we use to open this dialog : verification queue, on hold queue, acceptence queue, release queue
            @queueWeMadeTheCallFrom = ko.observable()
            
            #=============================
            @message = ko.observable()
            @messageCss = ko.observable()
            @isSubmitted = ko.observable(false)
            @event = ko.observable()
            @gridId = ko.observable()

            #============Process info========
            @hasAutoVerification = ko.observable(false)
            @autoVerificationStatus = ko.observable()
            @autoVerificationDate = ko.observable()
            
            @hasRiskLevel = ko.observable(false)
            @riskLevelStatus = ko.observable()
            @riskLevelDate = ko.observable()
            
            @hasVerificationQueue = ko.observable(false)
            @verificationQueueResult = ko.observable()
            @verificationHandledBy = ko.observable()
            @verificationDateHandled = ko.observable()
            
            @hasOnHoldQueue = ko.observable(false)
            @OnHoldResult = ko.observable()
            @OnHoldHandledBy = ko.observable()
            @OnHoldDateHandled = ko.observable()
            
            @hasDocuments = ko.observable(false)
            @documentsStatus = ko.observable()
            @documentsDate = ko.observable()
            
            @hasInvestigation = ko.observable(false)
            @investigationStatus = ko.observable()
            @investigationDate = ko.observable()
            @submitLabel = ko.observable("Submit")
            
            #=========== Remarks ============
            
            @adminRemarks = ko.observableArray()
            @playerRemarks = ko.observableArray()
            @officer = ko.observable()
            
        activate: (data) ->
            @id(data.id)
            @event(data.event)
            @gridId(data.gridId)
            @loadInfo()
            @queueWeMadeTheCallFrom(data.queueName)
        loadInfo: ->
            $.ajax "OfflineWithdraw/WithdrawalInfo?id=" + @id()
                .done (data) =>
                    @licenseeName(data.baseInfo.licensee)
                    @brandName(data.baseInfo.brand)
                    @username(data.baseInfo.username)
                    @referenceCode(data.baseInfo.referenceCode)
                    @status(data.baseInfo.status)
                    @internalAccount(data.baseInfo.internalAccount)
                    @currency(data.baseInfo.currency)
                    @paymentMethod(data.baseInfo.paymentMethod)
                    @amount(data.baseInfo.amount)
                    @submittedBy(data.baseInfo.submitted)
                    @dateSubmitted(data.baseInfo.dateSubmitted)
                    
                    @bankName(data.bankInformation.bankName)
                    @bankAccountName(data.bankInformation.bankAccountName)
                    @bankAccountNumber(data.bankInformation.bankAccountNumber)
                    @branch(data.bankInformation.branch)
                    @swiftCode(data.bankInformation.swiftCode)
                    @address(data.bankInformation.address)
                    @city(data.bankInformation.city)
                    @province(data.bankInformation.province)
                    
                    #======== process info ======
                    @hasAutoVerification(data.processInformation.autoVerification.hasAutoVerification)
                    @autoVerificationStatus(data.processInformation.autoVerification.status)
                    @autoVerificationDate(data.processInformation.autoVerification.date)
            
                    @hasRiskLevel(data.processInformation.riskLevel.hasRiskLevel)
                    @riskLevelStatus(data.processInformation.riskLevel.status)
                    @riskLevelDate(data.processInformation.riskLevel.date)
            
                    @hasVerificationQueue data.processInformation.verificationQueue.hasResult
                    @verificationQueueResult data.processInformation.verificationQueue.result
                    @verificationHandledBy data.processInformation.verificationQueue.handledBy
                    @verificationDateHandled data.processInformation.verificationQueue.dateHandled
                    
                    @hasOnHoldQueue data.processInformation.onHoldQueue.hasResult
                    @OnHoldResult data.processInformation.onHoldQueue.result
                    @OnHoldHandledBy data.processInformation.onHoldQueue.handledBy
                    @OnHoldDateHandled data.processInformation.onHoldQueue.dateHandled
                    
                    #=========== Remarks ==========
                    @adminRemarks(data.remarks.adminRemarks)
                    @playerRemarks(data.remarks.playerRemarks)
                    @officer(data.officer)
                    
        reloadGrid: ->
            $(@gridId()).trigger "reload"
        submit: ->
            self = @
            url = ""
            status = ""
            if @event() == "cancel"
                url = "offlinewithdraw/CancelRequest"
                status = "Canceled"
            if @event() == "unverify"
                url = "offlinewithdraw/unverify"
                status = "Unverified"
            if @event() == "verify"
                url = "offlinewithdraw/verify"
                status = "Verified"
            if @event() == "accept"
                url ="offlinewithdraw/accept"
                status = "Accepted"
            if @event() == "revert"
                url ="offlinewithdraw/revert"
                status = "Reverted"
            if @event() == "release"
                url ="offlinewithdraw/Approve"
                status = "Released"
            if @event() == "documents"
                url ="offlinewithdraw/documents"
                status = "Documents"
            if @event() == "investigate"
                url ="offlinewithdraw/investigate"
                status = "Investigate"
            submittedEvent = @event()
            
            $.post url,
                requestId: @id(), remarks: @remarks()
            .done (response) -> 
                self.adminRemarks.splice(0, 0, {user:self.officer, remark: self.remarks})
                self.message i18n.t response.data
                if response.result == "success"
                    $(document).trigger self.getEventByName(submittedEvent)
                    self.messageCss "alert alert-success left"
                    self.isSubmitted(true)
                    self.status(status)
                else
                    self.messageCss "alert alert-danger left"
                if self.event() is "release"
                    self.reloadAfterReleaseProcessed()
                else
                    self.reloadGrid()
        reloadAfterReleaseProcessed: ->
            self = @
            timeoutId = 0
            query = ->
                $.get "offlinewithdraw/get?id=" + self.id()
                    .done (response) =>
                        if response.data.status is 9
                            clearTimeout timeoutId
                            self.reloadGrid()
                        else
                            timeoutId = setTimeout query, 5000
            query()
        close: ->
            nav.close()
        clear: ->
            @remarks("")
        
        getEventByName: (currentEvent) ->
            if((currentEvent == "cancel" or currentEvent == "accept") and @gridId() == "#accept-grid")
                return "acceptance_queue_changed"
            if((currentEvent == "cancel" or currentEvent == "release") and @gridId() == "#release-grid")
                return "release_queue_changed"              
        
        avcStatus: ->
            id = @id()
            dialog = new withdrawalLogDialogViewModel(id, "/OfflineWithdraw/AutoVerificationStatus")
            dialog.show()
        rvcStatus: ->
            id = @id()
            dialog = new withdrawalLogDialogViewModel(id, "/OfflineWithdraw/RiskProfileCheckStatus")
            dialog.show()
