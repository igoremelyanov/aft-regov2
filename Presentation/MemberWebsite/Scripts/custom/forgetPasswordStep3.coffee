class ResetPasswordLastModel extends FormBase
    constructor: ->
        super
        $("#page").i18n();
        @playerId = ko.observable()
        @newPassword = ko.observable()
        @confirmPassword = ko.observable().extend
            validatable: yes
          
        @submitResetPassword = =>
            $.postJson '/api/ValidateConfirmResetPasswordRequest',
                NewPassword : @newPassword()
                ConfirmPassword : @confirmPassword()
                PlayerId: @playerId()
            .success (response) =>
                if response.hasError
                    $.each response.errors, (propName) =>
                        observable = @[propName]
                        observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.__valid__ no
                else
                    $.ajax
                        url:'/api/ConfirmResetPasswordRequest',
                        type:'post',
                        data:$('#confirm-reset-password-form').serialize()
                    .success (reponse) =>
                        localStorage.setItem("reset", "success")
                        redirect '/'
                          
            no
          
        do =>
        @playerId $("[name=playerId]").val()
          
viewModel = new ResetPasswordLastModel()
ko.applyBindings viewModel, document.getElementById "forget-step3-wrapper"
$("#forget-step3-wrapper").i18n();