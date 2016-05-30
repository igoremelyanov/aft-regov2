class Login
    constructor: ->
        super
       
        @username = ko.observable("").extend
            validator: yes
        @password = ko.observable("").extend
            validator: yes
        @captcha = ko.observable("").extend
            validator: yes
        @hasErrors = ko.observable no
        @messages = ko.observableArray []
        @requestInProgress = ko.observable no
        
        @signIn = ->
            @clearValidations()
            @requestInProgress yes
            
            $.postJson '/api/Login',
                UserName: self.username()
                Password: self.password()
             .done ->
                camelCaseProperties response
                redirect "/Home/PlayerProfile"
             .fail (jqXHR) ->
                response = JSON.parse jqXHR.responseText
                
                camelCaseProperties(response);
                @messages []
                @hasErrors yes
                
                if response.unexpected
                    @messages.push 'Unexpected error occurred.'
                else
                    errors = response.errors
                    if errors and errors.length
                        errors.forEach (error) ->
                            messageKey = "app:apiResponseCodes." + error.message
                            if error.params.length is 0
                                @messages.push $.i18n.t messageKey
                            else
                                params = new Object
                                error.params.forEach (param) ->
                                    params[param.name] = param.value
                                @messages.push $.i18n.t messageKey, params
                    else if response.message
                        @messages.push response.message
                
                $('#login-messages').modal()
                
            .always ->
                @requestInProgress no
                    
        
        @clearValidations = ->
            @username.setError no
            @password.setError no
        
        @toRegister = ->
            redirect "/Home/Register"
            
        @logout = ->
            $.postJson('/api/Logout')
             .done (response) ->
                redirect "/"
             .fail ->
                @messages []
                @messages.push 'Unexpected error occurred.'
                $('#login-messages').modal()
        
        @resetPasswordUsername = ko.observable().extend
            validator: yes

        @resetPasswordEmail = ko.observable().extend
            validator: yes
        
        @resetPassword ->
            $.postJson 'api/ResetPassword',
                Username: self.resetPasswordUsername()
                Email: self.resetPasswordEmail()
             .success ->
                $("#forgotPassword .step-1").hide().next().fadeIn()
                console.log "Password Changed"
             .fail (jqXHR) ->
                response = JSON.parse jqXHR.responseText
                error = response.message.charAt(0).toLowerCase() + response.message.slice(1)
                @messages []
                @messages.push $.t "login.validationMessages." + error
                $('#login-messages').modal()
        
        @hideResetPassword ->
            $("#forgotPassword").modal "hide"
        
        $(document).ready ->
            $('#forgotPassword').on 'show.bs.modal', ->
                $('.modal-password-step:first').show().next().hide()

ko.extenders.validator = (target) ->
    target.hasError = ko.observable no
    target.messages = ko.observableArray []
    target.error = ko.observable()
    target.errorMessage = ko.observable()

    target.setError = (val) ->
        target.hasError val
        if !val
            target.messages []

    target.addErrorMessage = (message) ->
        target.messages.push message

    return target
        
ko.applyBindings new LoginModel(), document.getElementById "login-wrapper"
ko.applyBindings new LoginModel(), document.getElementById "modal-login"