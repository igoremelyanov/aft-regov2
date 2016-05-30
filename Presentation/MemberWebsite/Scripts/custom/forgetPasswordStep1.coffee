class ForgetPasswordModel extends FormBase
    constructor: ->
        super
        
        @message = ko.observable()
        @hasErrors = ko.observable(false)
        
        @id = ko.observable('').extend
            validatable: yes
        
        @submitForgetPassword = ->
            $.postJson '/api/ValidateResetPasswordRequest',
               Id : @id()
             .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.__valid__ no
                else
                    $.ajax
                        url:'/api/ResetPassword',
                        type:'post',
                        data:$('#forgetPasswordFormId').serialize()
                        
                    $('#alert-modal').one 'hidden.bs.modal', () =>
                        window.location.href = "/"
                        
                    popupAlert(i18n.t('resetPassword.checkYourEmail'), i18n.t('resetPassword.confirmStep1'))

    model = new ForgetPasswordModel
    model.errors = ko.validation.group(model);
    ko.applyBindings model, $("#forget-step1-wrapper")[0]
    $("#forget-step1-wrapper").i18n();
