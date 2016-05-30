class @ResponsibleGambling extends FormBase
    constructor: (@id) ->
        super
        @message = ko.observable('')
        @submitted = ko.observable()
        
        @isTimeOutEnabled = ko.observable(false)
        @isTimeOutEnabled.subscribe (value) =>
            if value
                @isSelfExclusionEnabled no
        @timeOut = ko.observable()
        @timeOuts = ko.observable [{ id: 0, name: '24 hrs' },
        { id: 1, name: '1 week' },
        { id: 2, name: '1 month' },
        { id: 3, name: '6 weeks' }]
        
        @isSelfExclusionEnabled = ko.observable(false)
        @isSelfExclusionEnabled.subscribe (value) =>
            if value
                @isTimeOutEnabled no
        @selfExclusion = ko.observable()
        @selfExclusions = ko.observable [{ id: 0, name: '6 months' },
        { id: 1, name: '1 year' },
        { id: 2, name: '5 years' },
        { id: 3, name: 'permanent' }]

    save: =>
        question = if @isSelfExclusionEnabled() then "Are you sure you want to SelfExclude?" else "Are you sure you want to Time-Out?"
        
        @message(question)        
        $('#responsiblegaming-alert').modal()    

    logout: () =>
        $.postJson('/api/Logout')
            .done (response) =>
                redirect '/Home/Acknowledgement?id=' + @id()
    
    closeModal: () =>
      $('#responsiblegaming-alert button.close').trigger 'click'

    submitResponsible: () =>
        if @isSelfExclusionEnabled()
            @submit "/api/SelfExclude",
                PlayerId: @id()
                Option: @selfExclusion()
            , =>
                @editing no
                @logout()
        else if @isTimeOutEnabled()
            @submit "/api/TimeOut",
                PlayerId: @id()
                Option: @timeOut()
            , =>
                @editing no
                @logout()
