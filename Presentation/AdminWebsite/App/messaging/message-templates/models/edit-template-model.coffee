define (require) ->
    i18n = require "i18next"
    baseModel = require "base/base-model"

    class EditTemplateModel extends baseModel
        constructor: ->
            super

            @id = @makeField()
                .extend required: true

            @licenseeName = ko.observable()
            
            @brandName = ko.observable()
            
            @languageName = ko.observable()
            
            @messageType = ko.observable()
            
            @messageTypeDisplayName = ko.computed =>
                i18n.t "messageTemplates.messageTypes." + @messageType()

            @messageDeliveryMethod = ko.observable()

            @messageDeliveryMethod.subscribe (messageDeliveryMethod) =>
                if messageDeliveryMethod is 'Email'
                    @isEmail true
                    @isSms false
                    @subject.isModified false
                else if messageDeliveryMethod is 'Sms'
                    @isSms true
                    @isEmail false
                    @subject ""
                    
            @messageDeliveryMethodDisplayName = ko.computed =>
                i18n.t "messageTemplates.deliveryMethods." + @messageDeliveryMethod()

            @isEmail = ko.observable()

            @isSms = ko.observable()

            @templateName = @makeField().extend
                required: true

            @subject = @makeField().extend
                required:
                    onlyIf: =>
                        @isEmail()

            @messageContent = @makeField().extend
                required: true

        mapto: -> 
            super [
                "licenseeName", 
                "brandName", 
                "languageName", 
                "messageType",
                "messageTypeDisplayName",
                "messageDeliveryMethod",
                "messageDeliveryMethodDisplayName",
                "isEmail",
                "isSms"]