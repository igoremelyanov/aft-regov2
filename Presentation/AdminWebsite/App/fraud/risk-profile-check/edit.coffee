define 	["nav", "i18next", "security/security", "dateTimePicker", "EntityFormUtil", "shell"],
(nav, i18N, security, dateTimePicker, efu, shell) ->
    class RiskProfileCheckViewModel
        constructor: ->
            @message = ko.observable ""
            @messageClass = ko.observable
            @form = new efu.Form @
            @isReadOnly = ko.observable false
            efu.setupLicenseeField2 @
            efu.setupBrandField2 @
            @dummyObservable = ko.observable()
            
            @operators = ko.observableArray [{ text: ">", value: 0}, { text: "<", value: 1}, { text: ">=", value: 2}, { text: "<=", value: 3}]
            
            currencyField = @form.makeField "currency", ko.observable().extend  {required: true}
                .hasOptions()
            currencyField.setSerializer ()->
                currencyField.value()
            .setDisplay ko.computed ()->
                currencyField.value()
                
            vipLevelsField = @form.makeField("vipLevels", ko.observableArray([]).extend
                validation: [{
                    validator: (val) =>
                        val.length > 0
                    message: "Should be at least one"
                }])
                .hasOptions()
            vipLevelsField.setSerializer ()->
                _.map vipLevelsField.value(), (item) =>
                    item.id
                
            #Setup account age fields
            @form.makeField "hasAccountAge", ko.observable(false)
            @form.makeField "accountAge", ko.observable(1).extend {
                formatInt:
                    allowNegative: no,
                    allowEmpty: yes
                validatable: true,
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasAccountAge() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }]
            }
            @form.makeField "accountAgeOperator", ko.observable()
            @accountAgeOperatorTitle = ko.computed ()=>
                return @getOperator(@fields.accountAgeOperator())
                
            #Setup deposit count fields
            @form.makeField "hasDepositCount", ko.observable(false)
            @form.makeField "totalDepositCountOperator", ko.observable()
            @totalDepositCountOperatorTitle = ko.computed ()=>
                return @getOperator(@fields.totalDepositCountOperator())
            @form.makeField "totalDepositCountAmount", ko.observable(1).extend {
                formatInt:
                    allowNegative: no,
                    allowEmpty: yes
                validatable: true,
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasDepositCount() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }],
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            #Payment Methods Check
            @form.makeField "hasPaymentMethodCheck", ko.observable(false)
            @paymentMethodsAssignControl = new efu.AssignControl()
            paymentMethods = @form.makeField "paymentMethods", @paymentMethodsAssignControl.assignedItems.extend
                required:
                    message: "At least one payment method should be selected"
                    onlyIf: ko.computed =>
                        @form.fields.hasPaymentMethodCheck.value()
                        
            paymentMethods.setSerializer () ->
                ids = [];
                values = paymentMethods.value()

                i = 0
                while i < values.length
                  ids[i] = values[i].id
                  i++
                ids
            
            #Setup total withdrawal amount fields
            @form.makeField "hasWithdrawalCount", ko.observable(false)
            @form.makeField "totalWithdrawalCountOperator", ko.observable()
            @totalWithdrawalCountOperatorTitle = ko.computed () =>
                return @getOperator(@fields.totalWithdrawalCountOperator())
            @form.makeField "totalWithdrawalCountAmount", ko.observable(1).extend {
                formatInt:
                    allowNegative: no,
                    allowEmpty: yes
                required: true,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasWithdrawalCount() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }],
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            #Fraud risk levels
            @form.makeField "hasFraudRiskLevel", ko.observable(false)
            @fraudRiskLevelsAssignControl = new efu.AssignControl()
            riskLevelsFields = @form.makeField "riskLevels", @fraudRiskLevelsAssignControl.assignedItems.extend
                required:
                    message: "At least one risk level should be selected"
                    onlyIf: ko.computed =>
                        @form.fields.hasFraudRiskLevel.value()
                    
            riskLevelsFields.setSerializer () ->
                ids = [];
                riskLevels = riskLevelsFields.value()

                i = 0
                while i < riskLevels.length
                  ids[i] = riskLevels[i].id
                  i++
                ids
                
            #Bonuses
            @form.makeField "hasBonusCheck", ko.observable(false)
            @bonusesAssignControl = new efu.AssignControl()
            bonusesFields = @form.makeField "bonuses", @bonusesAssignControl.assignedItems.extend
                required:
                    message: "At least one bonus should be selected"
                    onlyIf: ko.computed =>
                        @form.fields.hasBonusCheck.value()
                
            bonusesFields.setSerializer () ->
                ids = [];
                bonuses = bonusesFields.value()

                i = 0
                while i < bonuses.length
                  ids[i] = bonuses[i].id
                  i++
                ids
                
            #Setup WinLoss criteria fields
            @form.makeField "hasWinLoss", ko.observable(false)
            @form.makeField "winLossOperator", ko.observable()
            @winLossOperatorTitle = ko.computed ()=>
                return @getOperator(@fields.winLossOperator())
            @form.makeField "winLossAmount", ko.observable(0.0).extend {
                formatInt:
                    allowNegative: yes,
                    allowEmpty: yes
                validatable: true,
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasWinLoss() || val >= -2147483648
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: -2147483648 }
                }],
                min: { params: -2147483648, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: -2147483648 } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"},
                required: true
            }
            
            #Setup WithdrawalAverageChange criteria fields
            @form.makeField "hasWithdrawalAverageChange", ko.observable(false)
            @form.makeField "withdrawalAverageChangeOperator", ko.observable()
            @withdrawalAverageChangeOperatorTitle = ko.computed ()=>
                return @getOperator(@fields.withdrawalAverageChangeOperator())
            @form.makeField "withdrawalAverageChangeAmount", ko.observable(0.0).extend {
                formatInt:
                    allowNegative: yes,
                    allowEmpty: yes
                required: true
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasWithdrawalAverageChange() || val >= -2147483648
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: -2147483648 }
                }],
                min: { params: -2147483648, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: '–2147483648' } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            #Setup WithdrawalAverageChange criteria fields
            @form.makeField "hasWinningsToDepositIncrease", ko.observable(false)
            @form.makeField "winningsToDepositIncreaseOperator", ko.observable()
            @winningsToDepositIncreaseOperatorTitle = ko.computed ()=>
                return @getOperator(@fields.winningsToDepositIncreaseOperator())
            @form.makeField "winningsToDepositIncreaseAmount", ko.observable(0.0).extend {
                formatInt:
                    allowNegative: yes,
                    allowEmpty: yes
                required: true
                validation: [{
                    validator: (val) =>
                        @dummyObservable()
                        !@fields.hasWinningsToDepositIncrease() || val >= -2147483648
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: -2147483648 }
                }],
                min: { params: -2147483648, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: '–2147483648' } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            @form.makeField "id", ko.observable()
                .lockValue true
            
            fieldsList = ["licensee", 
            "brand", 
            "currency", 
            "vipLevels",
            
            "hasAccountAge",
            "accountAge",
            "accountAgeOperator",
            
            "hasDepositCount",
            "totalDepositCountOperator",
            "totalDepositCountAmount",
            
            "hasPaymentMethodCheck",
            "paymentMethods",
            
            "hasWithdrawalCount",
            "totalWithdrawalCountOperator",
            "totalWithdrawalCountAmount",
            
            "hasFraudRiskLevel", 
            "riskLevels",
            
            "hasBonusCheck", 
            "bonuses",
            
            "hasWinLoss",
            "winLossOperator",
            "winLossAmount",
            
            "hasWithdrawalAverageChange",
            "withdrawalAverageChangeOperator",
            "withdrawalAverageChangeAmount",
            
            "hasWinningsToDepositIncrease",
            "winningsToDepositIncreaseOperator",
            "winningsToDepositIncreaseAmount"]
            
            efu.publishIds @, "risk-check-", fieldsList
            efu.addCommonMembers @
            @form.publishIsReadOnly fieldsList
            
            @getLicenseesUrl = () -> 
                    "Licensee/Licensees?useFilter=true"
                
            @getBrandsUrl = () =>
                    "Licensee/GetActiveBrands?licensee=" + this.form.fields.licensee.value().id
                    
            @form.fields.licensee.value.subscribe () =>
                efu.loadBrands2 @getBrandsUrl, @form.fields.brand
                
            @form.fields.brand.value.subscribe () =>
                $(@uiElement).parent().hide().prev().show()
                $.when(@loadCurrencies(), @loadVipLevels(), @loadFraudRisks(), @loadBonuses())
                    .done (r1,r2,r3,r4) =>
                        $(@uiElement).parent().show().prev().hide()

            @configuration = undefined
        
        activate: (data) =>
            deferred = $.Deferred()
            @fields.id if data then data.id else null
            @submitted data.editMode == false
            
            if @fields.id()
                @loadConfiguration deferred
            else
                @load deferred
            
            deferred.promise()
            
        loadConfiguration: (deferred) =>
            $.ajax "riskprofilecheck/GetById?id=" + @fields.id(), {
                success: (response) =>
                    @configuration = response
                    @fillViewModel()
                    @load(deferred)
            }
        
        load: (deferred) =>
            efu.loadLicensees2 @getLicenseesUrl, @form.fields.licensee, () =>
                @loadLicensees () =>
                    @loadBrands () =>
                        $.when(@loadCurrencies(),
                            @loadVipLevels(),
                            @loadFraudRisks(),
                            @loadBonuses(),
                            @loadPaymentMethods())
                            .done () =>
                                deferred.resolve()
        
        fillViewModel: () =>
            @fields.hasAccountAge @configuration.hasAccountAge
            @fields.accountAgeOperator @configuration.accountAgeOperator
            @fields.accountAge @configuration.accountAge
            
            @fields.hasDepositCount @configuration.hasDepositCount
            @fields.totalDepositCountOperator @configuration.totalDepositCountOperator
            @fields.totalDepositCountAmount @configuration.totalDepositCountAmount
            
            @fields.hasPaymentMethodCheck @configuration.hasPaymentMethodCheck
            # payment methods
            
            @fields.hasWithdrawalCount @configuration.hasWithdrawalCount
            @fields.totalWithdrawalCountAmount @configuration.totalWithdrawalCountAmount
            @fields.totalWithdrawalCountOperator @configuration.totalWithdrawalCountOperator
            
            @fields.hasFraudRiskLevel @configuration.hasFraudRiskLevel
            # risk levels
            
            @fields.hasBonusCheck @configuration.hasBonusCheck
            # bonuses
            
            @fields.hasWinLoss @configuration.hasWinLoss
            @fields.winLossAmount @configuration.winLossAmount
            @fields.winLossOperator @configuration.winLossOperator
            
            @fields.hasWithdrawalAverageChange @configuration.hasWithdrawalAverageChange
            @fields.withdrawalAverageChangeOperator @configuration.withdrawalAverageChangeOperator
            @fields.withdrawalAverageChangeAmount @configuration.withdrawalAverageChangeAmount
            
            @fields.hasWinningsToDepositIncrease @configuration.hasWinningsToDepositIncrease
            @fields.winningsToDepositIncreaseOperator @configuration.winningsToDepositIncreaseOperator
            @fields.winningsToDepositIncreaseAmount @configuration.winningsToDepositIncreaseAmount
        
        loadLicensees: (callback) =>
            licenseeId = efu.getBrandLicenseeId shell
            licensees = @form.fields.licensee.options()
            if @configuration
                licenseeId = @configuration.licensee;
                @form.fields["licensee"].isSet(true);
            efu.selectLicensee2 @form.fields.licensee, licenseeId
            efu.loadBrands2 @getBrandsUrl, @form.fields.brand, () =>
                @callCallback(callback)

        loadBrands: (callback) =>
            brandId = if @configuration then @configuration.brand else shell.brand().id()
            efu.selectBrand2 @form.fields.brand, brandId
            if @configuration
                @form.fields["brand"].isSet(true);
            
            @callCallback(callback)

        loadCurrencies: (callback) =>
            deferred = $.Deferred()
            
            brandId = @getBrandId()
            if brandId
                $.ajax "autoverification/getcurrencies?brandId=" + brandId
                    .done (response )=>
                        @form.fields.currency.setOptions response.currencies
                        if @configuration
                            efu.selectOption @form.fields.currency, (item) =>
                                item == @configuration.currency
                                
                        deferred.resolve()
                        @callCallback(callback)
            else
                deferred.resolve()
                @callCallback(callback)

            deferred.promise()

        loadVipLevels: (callback) =>
            deferred = $.Deferred()
        
            brandId = @getBrandId()
            if brandId
                $.ajax "RiskProfileCheck/getviplevels?brandId=" + brandId
                    .done (response) =>
                        @form.fields.vipLevels.setOptions response.vipLevels

                        filter = (id) ->
                            for vipLevel in response.vipLevels
                                if vipLevel.id == id
                                    return vipLevel

                        if @configuration && @configuration.vipLevels
                            selectedVipLevels = _.filter response.vipLevels, (level) =>
                                _.any @configuration.vipLevels, (levelId) =>
                                    level.id == levelId
                            
                            if selectedVipLevels.length != 0
                                @form.fields.vipLevels.value selectedVipLevels
                        
                        deferred.resolve()
                        @callCallback(callback)
            else
                deferred.resolve()
                @callCallback(callback)
            
            deferred.promise()
            
        loadFraudRisks: (callback) =>
            deferred = $.Deferred()
            
            brandId = @getBrandId()
            if brandId
                $.ajax "RiskProfileCheck/getfraudrisklevels?brandId=" + brandId
                    .done (response) =>
                        assigned = [];
                        notAssigned = [];
                        
                        if (@configuration)
                            for item in response.riskLevels
                                if _.contains(@configuration.riskLevels, item.id)
                                    assigned.push(item)
                                else
                                    notAssigned.push(item)
                        else
                            notAssigned = response.riskLevels
                            
                        @fraudRiskLevelsAssignControl.assignedItems assigned
                        @fraudRiskLevelsAssignControl.availableItems notAssigned
                        
                        deferred.resolve()
                        @callCallback(callback)
            else
                deferred.resolve()
                @callCallback(callback)
                
            deferred.promise()
                
        loadPaymentMethods: (callback) =>
            deferred = $.Deferred()
            
            brandId = @getBrandId()
            if brandId
                $.ajax "RiskProfileCheck/GetPaymentMethods"
                    .done (response) =>
                        assigned = [];
                        notAssigned = [];
                        
                        if (@configuration)
                            for item in response.paymentMethods
                                if _.contains(@configuration.paymentMethods, item.id)
                                    assigned.push(item)
                                else
                                    notAssigned.push(item)
                        else
                            notAssigned = response.paymentMethods
                            
                        @paymentMethodsAssignControl.assignedItems assigned
                        @paymentMethodsAssignControl.availableItems notAssigned
                        
                        deferred.resolve()
                        @callCallback(callback)
            else
                deferred.resolve()
                @callCallback(callback)
                
            deferred.promise()
        
        loadBonuses: (callback) =>
            deferred = $.Deferred()
            
            brandId = @getBrandId()
            if brandId
                $.ajax "RiskProfileCheck/GetBonuses?brandId=" + brandId
                    .done (response) =>
                        assigned = [];
                        notAssigned = [];
                        
                        if (@configuration)
                            for item in response.bonuses
                                if _.contains(@configuration.bonuses, item.id)
                                    assigned.push(item)
                                else
                                    notAssigned.push(item)
                        else
                            notAssigned = response.bonuses
                            
                        @bonusesAssignControl.assignedItems assigned
                        @bonusesAssignControl.availableItems notAssigned
                        
                        deferred.resolve()
                        @callCallback(callback)
            else
                deferred.resolve()
                @callCallback(callback)
                
            deferred.promise()
                
        getBrandId : => 
            brand = @form.fields.brand.value()
            if brand then brand.id else null
            
        getOperator: (id) ->
            if id == 0
                return ">"
            if id == 1
                return "<"
            if id == 2
                return ">="
            if id == 3
                return "<="
        
        naming = {
            gridBodyId: "risk-profile-check-list",
            editUrl: "riskProfileCheck/AddOrUpdate"
        }
        efu.addCommonEditFunctions(RiskProfileCheckViewModel.prototype, naming)
        
        callCallback: (callback) ->
            if callback
                callback()
        
        serializeForm: ()->
            res = @form.getDataObject()
            JSON.stringify res
        
        save = RiskProfileCheckViewModel.prototype.save
        save:() =>
            hasErrors = false;
            @dummyObservable new Date()
            
            if (!hasErrors)
                save.call this

        handleSaveSuccess = RiskProfileCheckViewModel.prototype.handleSaveSuccess
        handleSaveSuccess: (response) ->
            response.data = i18N.t "app:fraud.riskProfileCheck.messages." + response.data.code
            handleSaveSuccess.call(this, response)
            nav.title i18N.t "app:fraud.riskProfileCheck.titles.view"
            
        handleSaveFailure = RiskProfileCheckViewModel.prototype.handleSaveFailure
        handleSaveFailure: (response) ->
            response.data = response.data
            handleSaveFailure.call(this, response)
            nav.title i18N.t "app:fraud.riskProfileCheck.titles.failure"