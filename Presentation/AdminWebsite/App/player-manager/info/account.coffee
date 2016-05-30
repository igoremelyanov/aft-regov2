define ["i18next", "player-manager/send-new-password-dialog", "security/security", 'player-manager/change-vip-level-dialog', 'player-manager/change-payment-level-dialog', 'config'], (i18n, dialog, security, dialogVip, dialogPaymentLevel, config) ->
    class Account
        constructor: ->
            self = @
            
            @firstName = ko.observable()
               .extend({ required: true, minLength: 1, maxLength: 50 })
               .extend({
                   pattern: {
                       message: 'Invalid First Name',
                       params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
                   }
               })

            @lastName = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 20 })
                .extend({
                    pattern: {
                        message: 'Invalid Last Name',
                        params: '^[A-Za-z0-9]+(?:[ ._\'-][A-Za-z0-9]+)*$'
                    }
                })

            @dateOfBirth = ko.observable()
                .extend({ required: true, })
                
            @title = ko.observable()
            @titles = ko.observable()

            @gender = ko.observable()
            @genders = ko.observable()

            @email = ko.observable()
                .extend({ required: true, email: true, minLength: 1, maxLength: 50 })

            @phoneNumber = ko.observable()
                .extend({ required: true, number: true, minLength: 8, maxLength: 15 })

            @mailingAddressLine1 = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
                
            @mailingAddressLine2 = ko.observable()
                .extend({ maxLength: 50 })
                
            @mailingAddressLine3 = ko.observable().extend({ maxLength: 50 })
            @mailingAddressLine4 = ko.observable().extend({ maxLength: 50 })
            @mailingAddressCity = ko.observable()
            @mailingAddressPostalCode = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 10 })
            @mailingAddressStateProvince = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 20 })

            @physicalAddressLine1 = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
            @physicalAddressLine2 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressLine3 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressLine4 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressCity = ko.observable()
            @physicalAddressPostalCode = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 10 })
            @physicalAddressStateProvince = ko.observable()

            @securityQuestion = ko.observable()
            @securityAnswer = ko.observable()

            @country = ko.observable()
            @countryName = ko.computed(()->
                country = self.country()
                if country then country.name() else null
            )
            @countries = ko.observableArray()

            @contactPreference = ko.observable()
            @contactMethods = ko.observable()
            
            @accountAlertEmail = ko.observable no
            @accountAlertSms = ko.observable no
            @accountAlertsText = ko.computed =>
                text = []
                text.push "Email" if @accountAlertEmail()
                text.push "Sms" if @accountAlertSms()
                if text.length > 0 then text.join(", ") else "None"
            .extend
                validation: 
                    validator: =>
                        @accountAlertEmail() or @accountAlertSms()
    
                    message: i18n.t "app:playerManager.error.accountAlertsEmpty"
                    params: on
                    
            @marketingAlertEmail = ko.observable()
            @marketingAlertSms = ko.observable()
            @marketingAlertPhone = ko.observable()
            @marketingAlertsText = ko.computed =>
                text = []
                text.push "Email" if @marketingAlertEmail()
                text.push "Sms" if @marketingAlertSms()
                text.push "Phone" if @marketingAlertPhone()
                if text.length > 0 then text.join(", ") else "None"
            .extend
                validation: 
                    validator: =>
                        @marketingAlertEmail() or @marketingAlertSms() or @marketingAlertPhone()
    
                    message: i18n.t "app:playerManager.error.accountAlertsEmpty"
                    params: on
                  
            @paymentLevel = ko.observable()
            @paymentLevels = ko.observableArray()

            @paymentLevelName = ko.computed(()->
                level = self.paymentLevel()
                if level then level.name() else null
            )

            @registrationDate = ko.observable()
            @licensee =ko.observable()
            @brand = ko.observable()
            @currency = ko.observable()
            @culture = ko.observable()

            @vipLevel = ko.observable()
            @vipLevels = ko.observableArray()

            @vipLevelCode = ko.computed ()->
                vipLevel = self.vipLevel()
                if vipLevel then vipLevel.code() else null

            @vipLevelName = ko.computed ()->
                vipLevel = self.vipLevel()
                if vipLevel then vipLevel.name() else null
            
            @playerId = ko.observable()
            @detailsEditMode = ko.observable(false)
            @fullName = ko.observable()
            @username = ko.observable()
            @frozen = ko.observable()
            @freezeButtonText = ko.computed =>
                if @frozen() == true
                    "Unfreeze"
                else
                    "Freeze"
                    
            @isLocked = ko.observable()
            @isInactive = ko.observable()
            @isSelfExcluded = ko.observable()
            @isTimedOut = ko.observable()
            
            @isEditBtnVisible = ko.computed ()->
                security.isOperationAllowed(security.permissions.update, security.categories.playerManager)
            
            @isSetStatusBtnVisible = ko.computed ()->
                isActivated = !self.isInactive()
                return !isActivated && security.isOperationAllowed(security.permissions.activate, security.categories.playerManager) ||
                    isActivated && security.isOperationAllowed(security.permissions.deactivate, security.categories.playerManager)
            
            @activateButtonText = ko.computed ()->
                if !self.isInactive()
                    "Deactivate"
                else
                    "Activate"
            , @
            
            @unexcludeButtonText = ko.computed ()->
                if self.isSelfExcluded()
                    "Cancel exclusion"
                else if self.isTimedOut()
                    "Cancel time out"
            , @
            
            @canAssignVipLevel = ko.computed ()->
                security.isOperationAllowed(security.permissions.assignVipLevel, security.categories.playerManager)
          
            @canAssignPaymentLevel = ko.computed ()->
                security.isOperationAllowed(security.permissions.assignPaymentLevel, security.categories.playerManager)
          
            @initialData = { }
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.get config.adminApi("PlayerInfo/Get"), id: @playerId()
                .done((data)->
                    self.brandId = data.brandId
                    self.frozen data.frozen
                    self.isLocked data.isLocked
                    self.isInactive data.isInactive
                    self.isSelfExcluded data.isSelfExcluded
                    self.isTimedOut data.isTimedOut
                    
                    ko.mapping.fromJS(data, {}, self.initialData)
                    
                    self.registrationDate data.dateRegistered
                    self.licensee data.licensee
                    self.brand data.brand
                    self.currency data.currencyCode
                    self.culture data.culture
                    
                    $.ajax config.adminApi("PlayerManager/GetAddPlayerData")
                        .done (response) ->
                            ko.mapping.fromJS(response.data, {}, self)

                    $.ajax(config.adminApi("Brand/GetCountries"), {
                        async: false,
                        data: { brandId: self.brandId },
                        success: (response) ->
                            ko.mapping.fromJS(response, {}, self)
                            countries = self.countries()
                            $.each(countries, (index, value)-> 
                                if (value.code() == self.initialData.countryCode())
                                    self.initialData["country"] = ko.observable value
                            )
                        }
                    )

                    $.ajax(config.adminApi("PlayerManager/GetVipLevels?brandId=" + self.brandId), {
                        async: false
                        success: (response) ->
                            ko.mapping.fromJS(response, {}, self)
                            vipLevel = ko.utils.arrayFirst(self.vipLevels(), (thisVipLevel)->
                                thisVipLevel.id() == self.initialData.vipLevel()
                            )
                            self.vipLevel(vipLevel)
                    })

                    $.ajax config.adminApi('PlayerInfo/GetPlayerTitle?id=' + self.playerId())
                        .done((player)->
                            self.fullName(player.firstName + " " + player.lastName);
                            self.username(player.username);
                    )

                    $.ajax(config.adminApi("PlayerManager/GetPaymentLevels?brandId=" + self.brandId + "&currency=" + self.currency()), {
                        success: (response) ->
                            ko.mapping.fromJS(response, {}, self)
                            paymentLevel = ko.utils.arrayFirst(self.paymentLevels(), (thisPaymentLevel)->
                                thisPaymentLevel.id() == self.initialData.paymentLevel()
                            )
                            self.paymentLevel(paymentLevel)
                            self.setForm()
                    })

                )
        
        setForm:()->
            @firstName @initialData.firstName()
            @lastName @initialData.lastName()
            @dateOfBirth @initialData.dateOfBirth()
            @title @initialData.title()
            @gender @initialData.gender()
            @email @initialData.email()
            @phoneNumber @initialData.phoneNumber()
            @mailingAddressLine1 @initialData.mailingAddressLine1()
            @mailingAddressLine2 @initialData.mailingAddressLine2()
            @mailingAddressLine3 @initialData.mailingAddressLine3()
            @mailingAddressLine4 @initialData.mailingAddressLine4()
            @mailingAddressCity @initialData.mailingAddressCity()
            @mailingAddressPostalCode @initialData.mailingAddressPostalCode()
            @mailingAddressStateProvince @initialData.mailingAddressStateProvince()
            @physicalAddressLine1 @initialData.physicalAddressLine1()
            @physicalAddressLine2 @initialData.physicalAddressLine2()
            @physicalAddressLine3 @initialData.physicalAddressLine3()
            @physicalAddressLine4 @initialData.physicalAddressLine4()
            @physicalAddressCity @initialData.physicalAddressCity()
            @physicalAddressPostalCode @initialData.physicalAddressPostalCode()
            @physicalAddressStateProvince @initialData.physicalAddressStateProvince()
            @country @initialData.country()
            @contactPreference @initialData.contactPreference()
            @accountAlertSms @initialData.accountAlertSms()
            @accountAlertEmail @initialData.accountAlertEmail()
            @marketingAlertSms @initialData.marketingAlertSms()
            @marketingAlertEmail @initialData.marketingAlertEmail()
            @marketingAlertPhone @initialData.marketingAlertPhone()
            @securityQuestion @initialData.securityQuestion()
            @securityAnswer @initialData.securityAnswer()
        
        edit: ->
            @detailsEditMode on

        cancelEdit: ->
            @setForm()
            @detailsEditMode off
            
        resendActivationEmail: ->
            $.ajax(config.adminApi('PlayerInfo/ResendActivationEmail'), {
                type: "post",
                data: ko.toJSON({
                    id: @playerId()
                })
                contentType: 'application/json',
                success: (response) ->
                    if (response.result == "success")
                        alert "Activation Email sent!"
                    else
                        alert "failed to resend activation email"
            })
            
        showSendMessageDialog: (data)->
            id = data.playerId
            newPassword = ''
            sendBy = "Email"
            dialog.show(@, id, newPassword, sendBy)
            
        setStatus: ()->
            self = @
            $.ajax(config.adminApi('PlayerInfo/SetStatus'), {
                type: "post",
                data: ko.toJSON({
                    id: self.playerId(),
                    active: self.isInactive()
                })
                contentType: "application/json",
                success: (response)->
                    if (response.result == "success")
                        self.isInactive !response.active
                        #$(self.playerListSelector()).trigger("reloadGrid")
                    else
                        alert("Can't change status")
            })
            
        changeFreezeStatus: ()=>
            $.ajax config.adminApi('PlayerInfo/SetFreezeStatus'), {
                type: "post",
                data: ko.toJSON {
                    playerId: @playerId(),
                    freeze: !@frozen()
                }
                contentType: "application/json",
                success: (response) =>
                    if response.result != "success"
                        alert("Can't freeze/unfreeze account")
                    else
                        @frozen !@frozen()
            }
            
        unlock: ()=>
            $.ajax config.adminApi('PlayerInfo/Unlock'), {
                type: "post",
                data: ko.toJSON {
                    playerId: @playerId()
                }
                contentType: "application/json",
                success: (response) =>
                    if response.result != "success"
                        alert("Can't unlock the account")
                    else
                        @isLocked no
            }
            
        cancelExclusion: ()=>
            $.ajax config.adminApi('PlayerInfo/CancelExclusion'), {
                type: "post",
                data: ko.toJSON {
                    playerId: @playerId()
                }
                contentType: "application/json",
                success: (response) =>
                    if response.result != "success"
                        alert("Can't cancel exlusion on the account")
                    else
                        @isSelfExcluded no
                        @isTimedOut no
            }

        showChangeVipLevelDialog: (data)->
            id = data.playerId()
            brand = data.brand()
            userName = data.username()
            currentVipLevel = data.vipLevelCode()
            vipLevels = data.vipLevels
            dialogVip.show(@, id, brand, userName, currentVipLevel, vipLevels)
        
        showChangePaymentLevelDialog: (data)->
            id = data.playerId()
            licensee = data.licensee()
            brand = data.brand()
            currentPaymentLevel = data.paymentLevel().id()
            paymentLevels = data.paymentLevels
            dialogPaymentLevel.show(@, id, licensee,brand,currentPaymentLevel, paymentLevels)
                
        clearEdit: ()->
            @firstName("")
            @lastName("")
            @email("")
            @phoneNumber("")
            @mailingAddressLine1("")
            @mailingAddressLine2("")
            @mailingAddressLine3("")
            @mailingAddressLine4("")
            @mailingAddressCity("")
            @mailingAddressPostalCode("")
            @mailingAddressStateProvince("")
            @physicalAddressLine1("")
            @physicalAddressLine2("")
            @physicalAddressLine3("")
            @physicalAddressLine4("")
            @physicalAddressCity("")
            @physicalAddressPostalCode("")
            @physicalAddressStateProvince("")
            @accountAlertSms off
            @accountAlertEmail off
            @marketingAlertSms off
            @marketingAlertEmail off
            @marketingAlertPhone off
        
        saveEdit:()->
            self = @
            $(self.uiElement).parent().hide().prev().show() # show "Loading..."
            data = {
                PlayerId: @playerId,
                FirstName: @firstName(),
                LastName: @lastName(),
                DateOfBirth: @dateOfBirth(),
                Title: @title(),
                Gender: @gender(),
                Email: @email(),
                PhoneNumber: @phoneNumber(),
                MailingAddressLine1: @mailingAddressLine1(),
                MailingAddressLine2: @mailingAddressLine2(),
                MailingAddressLine3: @mailingAddressLine3(),
                MailingAddressLine4: @mailingAddressLine4(),
                MailingAddressCity: @mailingAddressCity(),
                MailingAddressPostalCode: @mailingAddressPostalCode(),
                MailingAddressStateProvince: @mailingAddressStateProvince(),
                PhysicalAddressLine1: @physicalAddressLine1(),
                PhysicalAddressLine2: @physicalAddressLine2(),
                PhysicalAddressLine3: @physicalAddressLine3(),
                PhysicalAddressLine4: @physicalAddressLine4(),
                PhysicalAddressCity: @physicalAddressCity(),
                PhysicalAddressPostalCode: @physicalAddressPostalCode(),
                PhysicalAddressStateProvince: @physicalAddressStateProvince(),
                CountryCode: self.country().code,
                ContactPreference: @contactPreference(),
                AccountAlertEmail: @accountAlertEmail(),
                AccountAlertSms: @accountAlertSms(),
                MarketingAlertEmail: @marketingAlertEmail(),
                MarketingAlertSms: @marketingAlertSms(),
                MarketingAlertPhone: @marketingAlertPhone(),
                PaymentLevelId: @paymentLevel().id
            }

            $.ajax
                type: "POST"
                url: config.adminApi('PlayerInfo/Edit')
                data: ko.toJSON(data)
                contentType: "application/json"
            .done (response)->
                $(self.uiElement).parent().show().prev().hide() # hide "Loading..."
                if (response.result == "success")
                    self.detailsEditMode false
                    response.data = "app:players.updated"
                    ko.mapping.fromJS(data, {}, self.initialData)
                    $("#player-grid").trigger("reload")
