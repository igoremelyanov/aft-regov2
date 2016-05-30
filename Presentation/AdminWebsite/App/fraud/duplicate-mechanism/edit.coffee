define 	["nav", "i18next", "security/security", "dateTimePicker", "EntityFormUtil", "shell"],
(nav, i18N, security, dateTimePicker, efu, shell) ->
    class DuplicateMechanismConfigViewModel
        constructor: ->
            @message = ko.observable ""
            @messageClass = ko.observable
            @form = new efu.Form @
            @isReadOnly = ko.observable false
            efu.setupLicenseeField2 @
            efu.setupBrandField2 @
            @dummyObservable = ko.observable()
            
            @form.makeField "id", ko.observable()
                .lockValue true
            
            @form.makeField "deviceIdExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "firstNameExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "lastNameExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "fullNameExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "usernameExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "addressExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "signUpIpExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "mobilePhoneExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "dateOfBirthExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "emailAddressExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "zipCodeExactScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            
            @form.makeField "deviceIdFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "firstNameFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "lastNameFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "fullNameFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "usernameFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "addressFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "signUpIpFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "mobilePhoneFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "dateOfBirthFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "emailAddressFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "zipCodeFuzzyScore", ko.observable().extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            
            @form.makeField "noHandlingScoreMax", ko.observable(0).extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "noHandlingScoreMin", ko.observable(0).extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3,
                validation:
                    validator: (val) =>
                        parseInt(@fields.noHandlingScoreMax()) >= parseInt(val)
                    message: "'From' value should be less or equals to 'To'."
                    
            @form.makeField "noHandlingSystemAction", ko.observable().extend  { required: true }
            @form.makeField "noHandlingDescr", ko.observable().extend { required: true }
            
            @form.makeField "recheckScoreMax", ko.observable(0).extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "recheckScoreMin", ko.observable(0).extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
                validation:
                    validator: (val) =>
                        parseInt(@fields.recheckScoreMax()) >= parseInt(val)
                    message: "'From' value should be less or equals to 'To'."
                    
            @form.makeField "recheckSystemAction", ko.observable().extend { required: true }
            @form.makeField "recheckDescr", ko.observable().extend { required: true }
            
            @form.makeField "fraudulentScoreMax", ko.observable(0).extend
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
            @form.makeField "fraudulentScoreMin", ko.observable(0).extend 
                required: true,
                formatInt:
                    allowNegative: no,
                    allowEmpty: no,
                maxLength: 3
                validation:
                    validator: (val) =>
                        parseInt(@fields.fraudulentScoreMax()) >= parseInt(val)
                    message: "'From' value should be less or equals to 'To'."
                    
            @form.makeField "fraudulentSystemAction", ko.observable().extend { required: true }
            @form.makeField "fraudulentDescr", ko.observable().extend { required: true }
            
            fieldsList = ["licensee", 
            "brand",
            
            "deviceIdExactScore",
            "firstNameExactScore",
            "lastNameExactScore",
            "fullNameExactScore",
            "usernameExactScore",
            "addressExactScore",
            "signUpIpExactScore",
            "mobilePhoneExactScore",
            "dateOfBirthExactScore",
            "emailAddressExactScore",
            "zipCodeExactScore",
            
            "deviceIdFuzzyScore",
            "firstNameFuzzyScore",
            "lastNameFuzzyScore",
            "fullNameFuzzyScore",
            "usernameFuzzyScore",
            "addressFuzzyScore",
            "signUpIpFuzzyScore",
            "mobilePhoneFuzzyScore",
            "dateOfBirthFuzzyScore",
            "emailAddressFuzzyScore",
            "zipCodeFuzzyScore",
            
            "noHandlingScoreMin",
            "noHandlingScoreMax",
            "noHandlingSystemAction",
            "noHandlingDescr",
            
            "recheckScoreMin",
            "recheckScoreMax",
            "recheckSystemAction",
            "recheckDescr",
            
            "fraudulentScoreMin",
            "fraudulentScoreMax",
            "fraudulentSystemAction",
            "fraudulentDescr"]
            
            @actions = ko.observableArray [{
                text: "No Action",
                value: 0
            },
            {
                text: "Freeze Account",
                value: 1
            },
            {
                text: "Disable Bonus",
                value: 2
            },
            {
                text: "Deactivate",
                value: 3
            }]
            
            @totalExactScore = ko.computed ()=>
                score = 0
                
                deviceIdExactScore = parseInt @fields.deviceIdExactScore()
                if not isNaN deviceIdExactScore
                    score += deviceIdExactScore
                    
                firstNameExactScore = parseInt @fields.firstNameExactScore()
                if not isNaN firstNameExactScore
                    score += firstNameExactScore
                    
                lastNameExactScore = parseInt @fields.lastNameExactScore()
                if not isNaN lastNameExactScore
                    score += lastNameExactScore
                    
                fullNameExactScore = parseInt @fields.fullNameExactScore()
                if not isNaN fullNameExactScore
                    score += fullNameExactScore
                    
                usernameExactScore = parseInt @fields.usernameExactScore()
                if not isNaN usernameExactScore
                    score += usernameExactScore
                    
                addressExactScore = parseInt @fields.addressExactScore()
                if not isNaN addressExactScore
                    score += addressExactScore
                    
                signUpIpExactScore = parseInt @fields.signUpIpExactScore()
                if not isNaN signUpIpExactScore
                    score += signUpIpExactScore
                    
                mobilePhoneExactScore = parseInt @fields.mobilePhoneExactScore()
                if not isNaN mobilePhoneExactScore
                    score += mobilePhoneExactScore
                    
                dateOfBirthExactScore = parseInt @fields.dateOfBirthExactScore()
                if not isNaN dateOfBirthExactScore
                    score += dateOfBirthExactScore
                    
                emailAddressExactScore = parseInt @fields.emailAddressExactScore()
                if not isNaN emailAddressExactScore
                    score += emailAddressExactScore
                    
                zipCodeExactScore = parseInt @fields.zipCodeExactScore()
                if not isNaN zipCodeExactScore
                    score += zipCodeExactScore
                    
                score
            
            @totalExactScore.subscribe (newValue) =>
                @fields.fraudulentScoreMax newValue
                
            @totalFuzzyScore = ko.computed ()=>
                score = 0
                
                deviceIdFuzzyScore = parseInt @fields.deviceIdFuzzyScore()
                if not isNaN deviceIdFuzzyScore
                    score += deviceIdFuzzyScore
                    
                firstNameFuzzyScore = parseInt @fields.firstNameFuzzyScore()
                if not isNaN firstNameFuzzyScore
                    score += firstNameFuzzyScore
                    
                lastNameFuzzyScore = parseInt @fields.lastNameFuzzyScore()
                if not isNaN lastNameFuzzyScore
                    score += lastNameFuzzyScore
                    
                fullNameFuzzyScore = parseInt @fields.fullNameFuzzyScore()
                if not isNaN fullNameFuzzyScore
                    score += fullNameFuzzyScore
                    
                usernameFuzzyScore = parseInt @fields.usernameFuzzyScore()
                if not isNaN usernameFuzzyScore
                    score += usernameFuzzyScore
                    
                addressFuzzyScore = parseInt @fields.addressFuzzyScore()
                if not isNaN addressFuzzyScore
                    score += addressFuzzyScore
                    
                signUpIpFuzzyScore = parseInt @fields.signUpIpFuzzyScore()
                if not isNaN signUpIpFuzzyScore
                    score += signUpIpFuzzyScore
                    
                mobilePhoneFuzzyScore = parseInt @fields.mobilePhoneFuzzyScore()
                if not isNaN mobilePhoneFuzzyScore
                    score += mobilePhoneFuzzyScore
                    
                dateOfBirthFuzzyScore = parseInt @fields.dateOfBirthFuzzyScore()
                if not isNaN dateOfBirthFuzzyScore
                    score += dateOfBirthFuzzyScore
                    
                emailAddressFuzzyScore = parseInt @fields.emailAddressFuzzyScore()
                if not isNaN emailAddressFuzzyScore
                    score += emailAddressFuzzyScore
                    
                zipCodeFuzzyScore = parseInt @fields.zipCodeFuzzyScore()
                if not isNaN zipCodeFuzzyScore
                    score += zipCodeFuzzyScore
                    
                score
            
            efu.publishIds @, "duplicate-", fieldsList
            efu.addCommonMembers @
            @form.publishIsReadOnly fieldsList
            
            @getLicenseesUrl = () -> 
                    "Licensee/Licensees?useFilter=true"
                
            @getBrandsUrl = () =>
                    "DuplicateMechanism/GetBrands?licensee=" + this.form.fields.licensee.value().id +
                        "&configId=" + @fields.id()
                    
            @form.fields.licensee.value.subscribe () =>
                efu.loadBrands2 @getBrandsUrl, @form.fields.brand
                
            @configuration = undefined
                
        loadLicensees: (callback) =>
            licenseeId = efu.getBrandLicenseeId shell
            licensees = @form.fields.licensee.options()
            if @configuration
                licenseeId = @configuration.licensee;
                #@form.fields["licensee"].isSet(true);
            efu.selectLicensee2 @form.fields.licensee, licenseeId
            efu.loadBrands2 @getBrandsUrl, @form.fields.brand, () =>
                @callCallback(callback)

        loadBrands: (callback) =>
            brandId = if @configuration then @configuration.brand else shell.brand().id()
            efu.selectBrand2 @form.fields.brand, brandId
            #if @configuration
            #    @form.fields["brand"].isSet(true);
            
            @callCallback(callback)
            
        naming = {
            gridBodyId: "duplicate-configurations-list",
            editUrl: "DuplicateMechanism/AddOrUpdate"
        }
        efu.addCommonEditFunctions(DuplicateMechanismConfigViewModel.prototype, naming)
        
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
            $.ajax "DuplicateMechanism/GetById?id=" + @fields.id(), {
                success: (response) =>
                    @configuration = response
                    @fillViewModel()
                    @load(deferred)
            }

        load: (deferred) =>
            efu.loadLicensees2 @getLicenseesUrl, @form.fields.licensee, () =>
                @loadLicensees () =>
                    @loadBrands () =>
                        deferred.resolve()
                
        callCallback: (callback) ->
            if callback
                callback()
                
        serializeForm: ()->
            res = @form.getDataObject()
            JSON.stringify res
        
        fillViewModel: () =>
            @fields.deviceIdExactScore @configuration.deviceIdExactScore
            @fields.firstNameExactScore @configuration.firstNameExactScore
            @fields.lastNameExactScore @configuration.lastNameExactScore
            @fields.fullNameExactScore @configuration.fullNameExactScore
            @fields.usernameExactScore @configuration.usernameExactScore
            @fields.addressExactScore @configuration.addressExactScore
            @fields.signUpIpExactScore @configuration.signUpIpExactScore
            @fields.mobilePhoneExactScore @configuration.mobilePhoneExactScore
            @fields.dateOfBirthExactScore @configuration.dateOfBirthExactScore
            @fields.emailAddressExactScore @configuration.emailAddressExactScore
            @fields.zipCodeExactScore @configuration.zipCodeExactScore
            
            @fields.deviceIdFuzzyScore @configuration.deviceIdFuzzyScore
            @fields.firstNameFuzzyScore @configuration.firstNameFuzzyScore
            @fields.lastNameFuzzyScore @configuration.lastNameFuzzyScore
            @fields.fullNameFuzzyScore @configuration.fullNameFuzzyScore
            @fields.usernameFuzzyScore @configuration.usernameFuzzyScore
            @fields.addressFuzzyScore @configuration.addressFuzzyScore
            @fields.signUpIpFuzzyScore @configuration.signUpIpFuzzyScore
            @fields.mobilePhoneFuzzyScore @configuration.mobilePhoneFuzzyScore
            @fields.dateOfBirthFuzzyScore @configuration.dateOfBirthFuzzyScore
            @fields.emailAddressFuzzyScore @configuration.emailAddressFuzzyScore
            @fields.zipCodeFuzzyScore @configuration.zipCodeFuzzyScore
            
            @fields.noHandlingScoreMin @configuration.noHandlingScoreMin
            @fields.noHandlingScoreMax @configuration.noHandlingScoreMax
            @fields.noHandlingSystemAction @configuration.noHandlingSystemAction
            @fields.noHandlingDescr @configuration.noHandlingDescr

            @fields.recheckScoreMin @configuration.recheckScoreMin
            @fields.recheckScoreMax @configuration.recheckScoreMax
            @fields.recheckSystemAction @configuration.recheckSystemAction
            @fields.recheckDescr @configuration.recheckDescr

            @fields.fraudulentScoreMin @configuration.fraudulentScoreMin
            @fields.fraudulentScoreMax @configuration.fraudulentScoreMax
            @fields.fraudulentSystemAction @configuration.fraudulentSystemAction
            @fields.fraudulentDescr @configuration.fraudulentDescr
        
        save = DuplicateMechanismConfigViewModel.prototype.save
        save:() =>
            hasErrors = false;
            
            if (!hasErrors)
                save.call @
                
        handleSaveSuccess = DuplicateMechanismConfigViewModel.prototype.handleSaveSuccess
        handleSaveSuccess: (response) ->
            response.data = i18N.t "app:fraud.duplicateMechanism.messages." + response.data.code
            handleSaveSuccess.call(this, response)
            nav.title i18N.t "app:fraud.duplicateMechanism.titles.view"
            
        handleSaveFailure = DuplicateMechanismConfigViewModel.prototype.handleSaveFailure
        handleSaveFailure: (response) ->
            response.data = response.data
            handleSaveFailure.call(this, response)
            nav.title i18N.t "app:fraud.duplicateMechanism.titles.failure"