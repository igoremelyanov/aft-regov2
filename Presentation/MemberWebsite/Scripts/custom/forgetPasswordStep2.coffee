class SecurityAnswerModel extends FormBase
    constructor: ->
        super
        @playerId = ko.observable()
        @token = ko.observable()
        @question = ko.observable "(loading...)"
        @answer = ko.observable().extend
            validatable: yes

        @securityQuestion = =>
            ## Doesn't work ? 
            $.getJson '/api/securityQuestions'
             .done (response) =>
                console.log response.securityQuestions
        
        @submitSecurityAnswerCheck = =>
            $.postJson '/api/ValidateSecurityAnswerRequest',
               PlayerId : @playerId()
               Answer : @answer()
             .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.__valid__ no
                else
                    $("#security-answer-form").submit()
                    redirect "/Home/ForgetPasswordStep3?token=" + @token()

            no
            
        do =>
            @playerId $("[name=playerId]").val()
            @token $("[name=token]").val()
            
            $.get "/api/getSecurityQuestion",
                PlayerId: @playerId()
            , (response) =>
                @question response.securityQuestion

    viewModel = new SecurityAnswerModel()
    ko.applyBindings viewModel, document.getElementById "forget-step2-wrapper"
    $("#forget-step2-wrapper").i18n();