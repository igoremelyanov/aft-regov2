define (require) ->
    require "controls/grid"
    config = require "config"
    efu = require "EntityFormUtil"
    moment = require "moment"
    Dialog = require "controls/confirmation-dialog/confirm-dialog"

    dateFormat = 'YYYY/MM/DD HH:mm:ss Z'

    class ViewModel
        constructor: ->
            @isLoading = false
        
            @playerId = ko.observable()
            @timeZoneOffset = ko.observable()
            @submitted = ko.observable()
            @message = ko.observable()
            @messageClass = ko.observable()

            @isTimeOutEnabled = ko.observable(false)
            @isTimeOutEnabled.subscribe (value) =>
                if value && !@isLoading
                    @isSelfExclusionEnabled no
                    @timeOutStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat

            @timeOut = ko.observable()
            @timeOut.subscribe (val) =>
                if @isTimeOutEnabled()
                    @timeOutStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat

            @timeOuts = ko.observable [{ id: 0, name: '24 hrs' },
            { id: 1, name: '1 week' },
            { id: 2, name: '1 month' },
            { id: 3, name: '6 weeks' }]
            @timeOutStartDate = ko.observable()
            @timeOutStartDate.subscribe (val) =>
                if (val && !@isLoading)
                    $.get "ResponsibleGambling/GetTimeOutEndDate", timeOutType:  @timeOut(), startDate : val
                        .done (date) =>
                            @timeOutEndDate moment(date).zone(@timeZoneOffset).format dateFormat
            @timeOutEndDate = ko.observable()
        
            @isSelfExclusionEnabled = ko.observable(false)
            @isSelfExclusionEnabled.subscribe (value) =>
                if value && !@isLoading
                    @isTimeOutEnabled no
                    @selfExclusionStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat
            @selfExclusion = ko.observable()
            @selfExclusion.subscribe (val) =>
                if @isSelfExclusionEnabled()
                    @selfExclusionStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat
                    
            @selfExclusions = ko.observable [{ id: 0, name: '6 months' },
            { id: 1, name: '1 year' },
            { id: 2, name: '5 years' },
            { id: 3, name: 'permanent' }]
            @selfExclusionStartDate = ko.observable()
            @selfExclusionStartDate.subscribe (val) =>
                if (val && !@isLoading)
                    $.get "ResponsibleGambling/GetSelfExclusionEndDate", exclusionType:  @selfExclusion(), startDate : val
                        .done (date) =>
                            @selfExclusionEndDate moment(date).zone(@timeZoneOffset).format dateFormat
            @selfExclusionEndDate = ko.observable()

        update:=>
            @message ''
        
            confirm = new Dialog {
                caption: 'Player Manager',
                question: 'Are you sure you want self exclude the player?',
                yesAction: ()=>
                    $.post "ResponsibleGambling/UpdateSelfExclusion", {
                        playerId:  @playerId(),
                        isTimeOutEnabled: @isTimeOutEnabled(),
                        timeOut: @timeOut(),
                        timeOutStartDate: @timeOutStartDate(),
                        isSelfExclusionEnabled: @isSelfExclusionEnabled(),
                        selfExclusion: @selfExclusion(),
                        selfExclusionStartDate: @selfExclusionStartDate()
                    }
                        .done (response) =>
                            if response.result == 'success'
                                @message 'Account has been successfully self-excluded.'
                                @messageClass 'alert-success'
                            else
                                @message 'Fail.'
                                @messageClass 'alert-danger'
                }
                
            confirm.show()

        activate: (data) =>
            @playerId data.playerId
            
            $.get "ResponsibleGambling/GetData", playerId:  @playerId()
                .done (response) =>
                    if response
                        @isLoading = yes
                    
                        @timeZoneOffset = response.timeZoneOffset
                        
                        @timeOut response.timeOut
                        @isTimeOutEnabled response.isTimeOutEnabled
                        if response.timeOutStartDate
                            @timeOutStartDate moment(response.timeOutStartDate).zone(@timeZoneOffset).format dateFormat
                        else
                            @timeOutStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat
                        @timeOutEndDate moment(response.timeOutEndDate).zone(@timeZoneOffset).format dateFormat
                        
                        @selfExclusion response.selfExclusion
                        @isSelfExclusionEnabled response.isSelfExclusionEnabled
                        if response.selfExclusionStartDate
                            @selfExclusionStartDate moment(response.selfExclusionStartDate).zone(@timeZoneOffset).format dateFormat
                        else
                            @selfExclusionStartDate moment(new Date()).zone(@timeZoneOffset).format dateFormat
                        @selfExclusionEndDate moment(response.selfExclusionEndDate).zone(@timeZoneOffset).format dateFormat

                        
                        @isLoading = no