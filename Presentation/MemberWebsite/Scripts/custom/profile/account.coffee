class @AccountProfile extends FormBase
    constructor: (@id) ->
        super
        self = @
        self.username = ko.observable()

        self.currentPassword = ko.observable().extend
            required:
                params: true
                message: $.i18n.t "app:resetPassword.FieldIsRequired"
        
        self.newPassword = ko.observable().extend
            required:
                params: true
                message: $.i18n.t "app:resetPassword.FieldIsRequired"
            minLength:
                params: 6
                message: $.i18n.t "app:resetPassword.PasswordIsNotWithinItsAllowedRange"
            maxLength:
                params: 12
                message: $.i18n.t "app:resetPassword.PasswordIsNotWithinItsAllowedRange"                

        self.confirmPassword = ko.observable().extend
            required:
                params: true
                message: $.i18n.t "app:resetPassword.FieldIsRequired"                
            validation:
                validator: (val) =>
                    val is self.newPassword()
                message: $.i18n.t "app:resetPassword.PasswordsCombinationIsNotValid"
                    
        self.changePasswordSuccessful = ko.observable false
        self.changePasswordFormVisible = ko.observable false
        ko.validation.group self

    changePassword: =>    
        self = @
        if self.isValid()
            $.post "/api/ChangePassword",
                Username: self.username()
                OldPassword: self.currentPassword()
                NewPassword: self.newPassword()
            .success (response) ->
                self.changePasswordSuccessful true
                self.changePasswordFormVisible false
                self.resetField self.currentPassword
                self.resetField self.newPassword
                self.resetField self.confirmPassword
            .fail (response) ->
                responseText = JSON.parse response.responseText
                field = if responseText.message is 'UsernamePasswordCombinationIsNotValid' then self.currentPassword else self.newPassword
                errorMessage = $.i18n.t "app:resetPassword." + responseText.message
                field.error = errorMessage
                field.__valid__ false
        else
            self.errors.showAllMessages()
     
    showChangePasswordForm: =>
        @changePasswordSuccessful false
        @changePasswordFormVisible true
        
    resetField: (field)=>
        field null
        field.__valid__ true