    class RegisterModel
        constructor:->
            @firstName = ko.observable("").extend({ validatable: yes });
            @lastName = ko.observable("").extend({ validatable: yes });
            @email = ko.observable("").extend({ validatable: yes });
            @phoneNumber = ko.observable("").extend({ validatable: yes })
            @mailingAddressLine1 = ko.observable("").extend({ validatable: yes });
            #@MailingAddressLine2 = ko.observable("").extend({ validatable: yes });
            #@MailingAddressLine3 = ko.observagetLoble("").extend({ validatable: yes });
            #@MailingAddressLine4 = ko.observable("").extend({ validatable: yes });
            @mailingAddressCity = ko.observable().extend({ validatable: yes });
            @mailingAddressStateProvince = ko.observable().extend({ validatable: yes });
            @mailingAddressPostalCode = ko.observable("").extend({ validatable: yes });
            @countryCode = ko.observable().extend({ validatable: yes });
            @currencyCode = ko.observable().extend({ validatable: yes });
            @captcha = ko.observable().extend({ validatable: yes });
            @captchaCheck = ko.observable().extend({ validatable: yes });
            @cultureCode = ko.observable().extend({ validatable: yes });
            @username = ko.observable("").extend({ validatable: yes });
            @password = ko.observable("").extend({ validatable: yes });
            @passwordConfirm = ko.observable("").extend({ validatable: yes });
            @dayOB = ko.observable(0).extend({ validatable: yes });
            @monthOB = ko.observable(0).extend({ validatable: yes });
            @yearOB = ko.observable(0).extend({ validatable: yes });
            @title = ko.observable().extend({ validatable: yes });
            @contactPreference = ko.observable().extend({ validatable: yes });
            @securityQuestionId = ko.observable().extend({ validatable: yes });
            @securityAnswer = ko.observable().extend({ validatable: yes });
            #ReferralId = ko.computed(function () {
            #    return getParameterByName("referralId");
            #});
            @dateOfBirth = ko.computed =>
                @yearOB() + "/" + @monthOB() + "/" + @dayOB()
            .extend { validatable: yes }
            @physicalAddressLine1 = ko.computed =>
                @mailingAddressLine1()
            .extend { validatable: yes }
            @physicalAddressCity = ko.computed =>
                @mailingAddressPostalCode()
            .extend { validatable: yes }
            @physicalAddressPostalCode = ko.computed =>
                @mailingAddressPostalCode()
            .extend { validatable: yes }
            @brandId = ko.computed =>
                "00000000-0000-0000-0000-000000000138"
            .extend { validatable: yes }
            @cultureCode = ko.computed =>
                findCookieValue("CultureCode")
            .extend { validatable: yes }
            @gender = ko.computed =>
                $('input[name="Gender"]:checked').val()
            .extend { validatable: yes }
        
            @over18 = ko.observable no
            @acceptTerms = ko.observable no
        
        submitRegistration: =>
            serializedForm = $('#register-step1-form').serializeArray()
            serializedForm.push {
                name: "Gender"
                value: $('input[name="Gender"]:checked').val()
            }, {
                name: "PhoneNumber"
                value: $('#phone-number').intlTelInput("getNumber").replace('+', '00')
            }, {
                name: "CurrencyCode"
                value: "RMB"
            }, {
                name: "CountryCode"
                value: "CN"
            }
        
            $.ajax
                url: '/api/ValidateRegisterInfo'
                type: 'post',
                data: $.param(serializedForm)
            .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        if observable
                            observable.error = i18n.t "apiResponseCodes." + response.errors[propName]
                            observable.__valid__ no
                            observable.isModified yes
                        else
                            alert response.errors[propName]
                else
                    $.ajax
                        url: '/api/Register'
                        type: 'post'
                        data: $.param(serializedForm)
                    .success (response) =>
                        redirect("/Home/RegisterStep2");
                    .fail () =>
                        alert('Unexpected error occured.')
                        
                    #$('#alert-modal')
                    #    .find('p.title').text(i18n.t('resetPassword.checkYourEmail')).end()
                    #    .find('p.message').text(i18n.t('resetPassword.confirmStep1')).end()
                    #    .modal()

    model = new RegisterModel();

    ko.applyBindings(model, document.getElementById("register-wrapper"));