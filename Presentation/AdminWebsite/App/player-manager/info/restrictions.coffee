define ["i18next", "config", "dateTimePicker"], (i18n, config) ->

    class Restrictions
    
        constructor: ->
            @playerId = ko.observable()
            @message = ko.observable()
            @messageClass = ko.observable()
            @exempt = ko.observable(false)
            
            @exemptFrom = ko.observable()
                .extend {
                    required: true
                }            
                
            @exemptTo = ko.observable()
                .extend {
                    required: true
                }
                
            @exemptLimit = ko.observable()
                .extend {
                    required: true,
                    pattern: {
                        message: i18n.t("app:common.enterPositiveInt"),
                        params: "^[0-9]+$"
                    }
                }
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.get config.adminApi('PlayerInfo/GetExemptionData'), id: @playerId
                .done (data)->
                    self.exempt data.exemptWithdrawalVerification
                    self.exemptFrom data.exemptFrom
                    self.exemptTo data.exemptTo
                    self.exemptLimit data.exemptLimit
            
        submitExemption: ->
            self = this
            $.ajax
                type: "POST"
                url: config.adminApi('PlayerInfo/SubmitExemption')
                data: ko.toJSON({
                    PlayerId: @playerId(),
                    Exempt: @exempt(),
                    ExemptFrom: @exemptFrom(),
                    ExemptTo: @exemptTo(),
                    ExemptLimit: @exemptLimit()
                })
                contentType: "application/json"
              .done (response) ->
                    if (response.result == "failed")
                        self.message i18n.t(response.data)
                        self.messageClass "alert-danger"
                    else
                        self.message i18n.t("app:exemption.updated")
                        self.messageClass "alert-success"
