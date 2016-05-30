define 	["nav", "i18next", "security/security", "dateTimePicker", "EntityFormUtil", "shell"],
(nav, i18N, security, dateTimePicker, efu, shell) ->
    class SignUpFraudTypeViewModel
        constructor: ->
            @message = ko.observable ""
            @messageClass = ko.observable()
            @form = new efu.Form @
            @isReadOnly = ko.observable false
            @configuration = ko.observable()
           
            @form.makeField "id", ko.observable()
                .lockValue true
           
            @form.makeField "fraudTypeName", ko.observable().extend
                required: yes
                maxLength: 50
            @form.makeField "remarks", ko.observable().extend
                required: yes
                maxLength: 200
           
            systemActionField = @form.makeField "systemAction", ko.observable().extend
                    required: true
                .hasOptions()
                
            systemActionField.setSerializer ()->
                    systemActionField.value().value
                    
            systemActionField.setDisplay ko.computed ()->
                    systemActionField.value()?.name
           
            @fraudRiskLevelsAssignControl = new efu.AssignControl()
            riskLevelsFields = @form.makeField "riskLevels", @fraudRiskLevelsAssignControl.assignedItems

            riskLevelsFields.setSerializer () ->
                ids = [];
                riskLevels = riskLevelsFields.value()

                i = 0
                while i < riskLevels.length
                  ids[i] = riskLevels[i].id
                  i++
                ids
                
            fieldsList = ["riskLevels",
            "fraudTypeName",
            "systemAction",
            "remarks"]
            
            efu.publishIds @, "signup-fraud-type-", fieldsList
            efu.addCommonMembers @
            @form.publishIsReadOnly fieldsList
        
        activate: (data) =>
            deferred = $.Deferred()
            @fields.id if data then data.id else null
            @submitted data.editMode == false
            
            if @fields.id()
                @loadConfiguration ()=>
                    @loadFraudRisks ()=>
                        @loadSystemActions ()=>
                            deferred.resolve()
            else
                @loadSystemActions ()=>
                    @loadFraudRisks ()=>
                        deferred.resolve()
            
            deferred.promise()
             
        loadConfiguration: (callback) =>
            deferred = $.Deferred()
            
            $.ajax "SignUpFraudTypes/GetById?id=" + @fields.id()
                .done (response) =>
                    @fields.systemAction response.systemAction
                    @fields.fraudTypeName response.fraudTypeName
                    @fields.remarks response.remarks
                    @configuration response

                    deferred.resolve()
                    @callCallback(callback)
        
        loadFraudRisks: (callback, configuration) =>
            deferred = $.Deferred()
            
            $.ajax "SignUpFraudTypes/GetFraudRiskLevels"
                .done (response) =>
                    assigned = [];
                    notAssigned = [];
                        
                    if @configuration()
                        for item in response.riskLevels
                            if _.contains(@configuration().riskLevels, item.id)
                                assigned.push(item)
                            else
                                notAssigned.push(item)
                    else
                        notAssigned = response.riskLevels
                            
                    @fraudRiskLevelsAssignControl.assignedItems assigned
                    @fraudRiskLevelsAssignControl.availableItems notAssigned
                        
                    deferred.resolve()
                    @callCallback(callback)
                
            deferred.promise()
        
        loadSystemActions: (callback, configuration) =>
            deferred = $.Deferred()
            
            $.ajax "SignUpFraudTypes/GetSystemActions"
                .done (response)=>
                    @form.fields.systemAction.setOptions response.systemActions
                    
                    if @configuration()
                        selectedOne = _.findLast response.systemActions, (item) =>
                            item.value == @configuration().systemAction
                            
                        @form.fields.systemAction.value selectedOne
                    
                    deferred.resolve()
                    @callCallback(callback)

            deferred.promise()
        
        callCallback: (callback) ->
            if callback
                callback()
        
        naming = {
            gridBodyId: "signup-fraud-types-list",
            editUrl: "SignUpFraudTypes/AddOrUpdate"
        }
        efu.addCommonEditFunctions(SignUpFraudTypeViewModel.prototype, naming)
        
        callCallback: (callback) ->
            if callback
                callback()

        serializeForm: ()->
            res = @form.getDataObject()
            JSON.stringify res

        handleSaveSuccess = SignUpFraudTypeViewModel.prototype.handleSaveSuccess
        handleSaveSuccess: (response) ->
            response.data = i18N.t "app:fraud.signUpFraudType.messages." + response.data.code
            handleSaveSuccess.call(this, response)
            nav.title i18N.t "app:fraud.signUpFraudType.titles.view"
            
        handleSaveFailure = SignUpFraudTypeViewModel.prototype.handleSaveFailure
        handleSaveFailure: (response) ->
            response.data = i18N.t "app:fraud.signUpFraudType.messages." + response.data.code
            handleSaveFailure.call(this, response)
            nav.title i18N.t "app:fraud.signUpFraudType.titles.failure"