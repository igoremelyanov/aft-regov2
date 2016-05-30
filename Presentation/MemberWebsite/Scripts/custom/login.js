var LoginModel = function () {
    var self = this;

    self.username = ko.observable("").extend({ validator: true });
    self.password = ko.observable("").extend({ validator: true });
    self.captcha = ko.observable("").extend({ validator: true });
    self.hasErrors = ko.observable(false);
    self.messages = ko.observableArray([]);
    self.requestInProgress = ko.observable(false);

    self.signIn = function () {
        localStorage.clear();
        self.clearValidations();
        self.requestInProgress(true);

        $.postJson('/api/Login', {
            UserName: self.username(),
            Password: self.password()
        })
        .done(function (response) {
            self.hasErrors(false);
            camelCaseProperties(response);
            redirect("/Home/Overview");
        })
        .fail(function (jqXHR) {
            var response = JSON.parse(jqXHR.responseText);
            camelCaseProperties(response);
            self.messages([]);
            var message;

            if (response.unexpected) {
                message = $.i18n.t('common.unexpectedError');
                self.messages.push(message);
                self.setMessageError(message);
            } else {
                var error = response.errors[0];
                var key = error.message;
                var prefix = "app:apiResponseCodes.";
                var params = {};

                error.params.forEach(function(param) {
                    params[param.name] = param.value;
                });

                if (params.hasOwnProperty("length"))
                    params.length = $.i18n.t(prefix + key + "Length." + params.length);

                // Responsible Gambling Or Account locked
                if (key === "AccountLocked" || key === "TimedOut" || key === "SelfExcluded" || key === "SelfExcludedPermanent") {

                    // Close the login popup
                    self.hasErrors(false);
                    $('#loginForm').modal('hide');

                    // Prepare title and message for the alert popup
                    message = $.i18n.t(prefix + key, params);
                    var title = $.i18n.t(prefix + key + 'Title', params);

                    // Show alert popop
                    popupAlert(title, message);
                }
                else {
                    message = $.i18n.t(prefix + key, params);
                    self.setMessageError(message);    
                }
            }
        })
        .always(function () {
            self.requestInProgress(false);
        });
    };

    self.clearValidations = function () {
        self.username.setError(false);
        self.password.setError(false);
    };

    self.toRegister = function () {
        redirect("/Home/Register");
    };

    self.setMessageError = function(message) {
        $('.form-group.message').find('.msg').text(message);
        self.hasErrors(true);
    };

    self.logout = function () {
        $.postJson('/api/Logout')
            .done(function (response) {
                redirect("/");
            })
        .fail(function () {
            self.messages([]);
            self.messages.push('Unexpected error occurred.');
            $('#login-messages').modal();
        });
    };

    // RESET PASSWORD //

    self.resetPasswordUsername = ko.observable()
    .extend({
        validator: true
    });

    self.resetPasswordEmail = ko.observable()
    .extend({
        validator: true
    });

    self.resetPassword = function () {
        $.postJson('api/ResetPassword', {
            Username: self.resetPasswordUsername(),
            Email: self.resetPasswordEmail()
        }).success(function () {
            $("#forgotPassword .step-1").hide().next().fadeIn();
        }).fail(function (jqXHR) {
            var response = JSON.parse(jqXHR.responseText);
            var error = response.message.charAt(0).toLowerCase() + response.message.slice(1);
            self.messages([]);
            self.messages.push($.t("login.validationMessages." + error));
            $('#login-messages').modal();
        });
    }

    self.hideResetPassword = function () {
        $("#forgotPassword").modal("hide");
    }

    self.initLogin = function() {
        self.hasErrors(false);
        self.clearValidations();
        self.username('');
        self.password('');
    };

    $(document).ready(function () {
        $('#forgotPassword').on('show.bs.modal', function () {
            $('.modal-password-step:first').show()
                .next().hide();
        });
    });
};

ko.extenders.validator = function (target) {
    target.hasError = ko.observable(false);
    target.messages = ko.observableArray([]);
    target.error = ko.observable();
    target.errorMessage = ko.observable();

    target.setError = function (val) {
        target.hasError(val);
        if (!val) {
            target.messages([]);
        }
    };

    target.addErrorMessage = function (message) {
        target.messages.push(message);
    }
    target.subscribe(function(newVal, oldVal) {
        target.setError(false);
    });

    return target;
};

ko.applyBindings(new LoginModel(), document.getElementById("login-controls"));
ko.applyBindings(new LoginModel(), document.getElementById("modal-login"));