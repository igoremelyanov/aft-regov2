define(['i18next', 'EntityFormUtil', 'shell', "nav", 'moment', 'datePicker'], function (i18n, efu, shell, nav, moment) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var self = this;
        var vmSerial = serial++;
        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);
        var licenseeField = form.fields.licensee;
        licenseeField.value.subscribe(function () {
            self.loadBrands();
        });
        licenseeField.setSerializer(function () {
            return licenseeField.value().id;
        });

        efu.setupBrandField2(this);
        this.playerPrefix = ko.observable();
        var brandField = form.fields.brand;
        brandField.value.subscribe(function () {
            self.loadBrandData();
        });
        brandField.setSerializer(function () {
            return brandField.value().id;
        });

        form.makeField("username", ko.observable().extend({
            required: true,
            minLength: 6,
            maxLength: 12,
            pattern: {
                params: '^[A-Za-z0-9-_\.\']*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriods", { fieldName: i18n.t("app:playerManager.add.userName") })
            }
        }));

        form.makeField("password", ko.observable().extend({
            required: true,
            minLength: 6,
            maxLength: 12,
            validation: {
                validator: function (val) {
                    return val.indexOf(' ') === -1;
                },
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriods", { fieldName: i18n.t("app:playerManager.add.password") })
            }
        }));

        form.makeField("passwordConfirm", ko.observable().extend({
            required: true,
            equal: {
                params: this.fields.password,
                message: i18n.t("app:playerManager.add.confirmPasswordDoesntMatch")
            }
        }));

        form.makeField("gender", ko.observable()).hasOptions();

        form.makeField("title", ko.observable()).hasOptions();

        form.makeField("firstName", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 50,
            pattern: {
                params: '^[a-zA-Z0-9\-\' _\.]+$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriods", { fieldName: i18n.t("app:playerManager.add.firstName") })
            }
        }));

        form.makeField("lastName", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 50,
            pattern: {
                params: '^[a-zA-Z0-9\-\' _\.]+$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriods", { fieldName: i18n.t("app:playerManager.add.lastName") })
            }
        }));

        form.makeField("dateOfBirth", ko.observable().extend({
            required: true,
            validation: {
                validator: function (val) {
                    var date = new Date(val);

                    var age = new Date().getFullYear() - date.getFullYear();

                    return age >= 18;
                },
                message: i18n.t("app:common.validationMessages.ageIsUnderAllowed"),
                params: true
            }
        }));

        this.dateOfBirthPickerFieldId = ko.observable("dateOfBirthPicker-" + vmSerial);

        form.makeField("email", ko.observable().extend({
            required: true,
            email: true,
            minLength: 1,
            maxLength: 50,
        }));

        form.makeField("phoneNumber", ko.observable().extend({
            required: true,
            minLength: 8,
            maxLength: 15,
            pattern: {
                params: '^((\\+)|(00)|(\\*)|())[0-9]{3,14}((\\#)|())$',
                message: "Enter valid phone number"
            }

        }));

        form.makeField("country", ko.observable()).hasOptions();

        form.makeField("currency", ko.observable()).hasOptions();

        form.makeField("culture", ko.observable()).hasOptions();

        form.makeField("affiliateCode", ko.observable().extend({
            minLength: 2,
            maxLength: 6,
            pattern: {
                params: '^[A-Za-z0-9-_\.\']*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriods")
                    .replace("__fieldName__", i18n.t("app:playerManager.add.affiliateCode"))
            }
        }));

        var isInactiveField = form.makeField("isInactive", ko.observable()).hasOptions();

        isInactiveField.setSerializer(function() {
            return isInactiveField.value().value;
        });

        var securityQuestionIdField = form.makeField("securityQuestionId", ko.observable()).hasOptions();

        securityQuestionIdField.setSerializer(function () {
            var securityQuestion = securityQuestionIdField.value();
            return securityQuestion.id;
        });

        form.makeField("securityAnswer", ko.observable().extend({
            required: true
        }));

        createAddressField("mailingAddressLine1", i18n.t("app:playerManager.add.addressLine1"), true);
        createAddressField("mailingAddressLine2", i18n.t("app:playerManager.add.addressLine2"), false);
        createAddressField("mailingAddressLine3", i18n.t("app:playerManager.add.addressLine3"), false);
        createAddressField("mailingAddressLine4", i18n.t("app:playerManager.add.addressLine4"), false);

        form.makeField("mailingAddressCity", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 50,
            pattern: {
                params: '^[A-Za-z0-9 -\.\']*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces")
                    .replace("__fieldName__", i18n.t("app:playerManager.add.city"))
            }
        }));

        form.makeField("mailingAddressPostalCode", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 10,
            pattern: {
                params: '^[A-Za-z0-9-]*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashes")
                    .replace("__fieldName__", i18n.t("app:playerManager.add.postalCode"))
            }
        }));

        form.makeField("mailingAddressStateProvince", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 20
        }));

        createAddressField("physicalAddressLine1", i18n.t("app:playerManager.add.addressLine1"), true);
        createAddressField("physicalAddressLine2", i18n.t("app:playerManager.add.addressLine2"), false);
        createAddressField("physicalAddressLine3", i18n.t("app:playerManager.add.addressLine3"), false);
        createAddressField("physicalAddressLine4", i18n.t("app:playerManager.add.addressLine4"), false);

        form.makeField("physicalAddressCity", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 50,
            pattern: {
                params: '^[A-Za-z0-9 -\.\']*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces")
                    .replace("__fieldName__", i18n.t("app:playerManager.add.city"))
            }
        }));

        form.makeField("physicalAddressPostalCode", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 10,
            pattern: {
                params: '^[A-Za-z0-9-]*$',
                message: i18n.t("app:common.validationMessages.onlyAlphanumericDashes")
                    .replace("__fieldName__", i18n.t("app:playerManager.add.postalCode"))
            }
        }));

        form.makeField("physicalAddressStateProvince", ko.observable().extend({
            required: true,
            minLength: 1,
            maxLength: 20
        }));

        form.makeField("contactPreference", ko.observable()).hasOptions();

        form.makeField("accountAlertEmail", ko.observable());
        form.makeField("accountAlertSms", ko.observable());

        var that = this;
        form.makeField("accountAlerts", ko.observable().extend({
            validation: {
                validator: function () {
                    return that.fields.accountAlertEmail() || that.fields.accountAlertSms();
                },
                message: i18n.t("app:playerManager.error.accountAlertsEmpty"),
                params: true
            }
        }));

        form.makeField("idStatus", ko.observable()).hasOptions();

        form.makeField("internalAccount", ko.observable());

        form.makeField("comments", ko.observable().extend({ maxLength: 1500 }));

        efu.publishIds(this, "add-player-", [
            "licensee",
            "brand",
            "username",
            "password",
            "passwordConfirm",
            "gender",
            "title",
            "firstName",
            "lastName",
            "dateOfBirth",
            "email",
            "phoneNumber",
            "country",
            "currency",
            "culture",
            "affiliateCode",
            "isInactive",
            "securityQuestionId",
            "securityAnswer",
            "mailingAddressLine1",
            "mailingAddressLine2",
            "mailingAddressLine3",
            "mailingAddressLine4",
            "mailingAddressCity",
            "mailingAddressPostalCode",
            "mailingAddressStateProvince",
            "physicalAddressLine1",
            "physicalAddressLine2",
            "physicalAddressLine3",
            "physicalAddressLine4",
            "physicalAddressCity",
            "physicalAddressPostalCode",
            "physicalAddressStateProvince",
            "contactPreference",
            "accountAlerts",
            "accountAlertEmail",
            "accountAlertSms",
            "idStatus",
            "internalAccount",
            "comments"
        ], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly([
            "licensee",
            "brand",
            "gender",
            "title",
            "country",
            "currency",
            "culture",
            "isInactive",
            "securityQuestionId",
            "contactPreference",
            "idStatus"
        ]);

        this.isSameAddress = ko.observable(true);

        this.isSameAddress.subscribe(function (isSameAddress) {
            if (!isSameAddress) {
                form.fields.physicalAddressLine1.value("");
                form.fields.physicalAddressLine2.value("");
                form.fields.physicalAddressLine3.value("");
                form.fields.physicalAddressLine4.value("");
                form.fields.physicalAddressCity.value("");
                form.fields.physicalAddressPostalCode.value("");
                form.fields.physicalAddressStateProvince.value("");
            }
        });

        subscribeAddressValue(form.fields.mailingAddressLine1, form.fields.physicalAddressLine1);
        subscribeAddressValue(form.fields.mailingAddressLine2, form.fields.physicalAddressLine2);
        subscribeAddressValue(form.fields.mailingAddressLine3, form.fields.physicalAddressLine3);
        subscribeAddressValue(form.fields.mailingAddressLine4, form.fields.physicalAddressLine4);
        subscribeAddressValue(form.fields.mailingAddressCity, form.fields.physicalAddressCity);
        subscribeAddressValue(form.fields.mailingAddressPostalCode, form.fields.physicalAddressPostalCode);
        subscribeAddressValue(form.fields.mailingAddressStateProvince, form.fields.physicalAddressStateProvince);

        function createAddressField(fieldName, fieldLabel, required) {
            form.makeField(fieldName, ko.observable().extend({
                required: required,
                minLength: 1,
                maxLength: 50,
                pattern: {
                    params: '^[A-Za-z0-9 -\.\']*$',
                    message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces")
                        .replace("__fieldName__", fieldLabel)
                }
            }));
        };

        function subscribeAddressValue(mailingAddressField, physicalAddressField) {
            mailingAddressField.value.subscribe(function (addressValue) {
                if (self.isSameAddress()) {
                    physicalAddressField.value(addressValue);
                }
            });
        };
    };

    ViewModel.prototype.activate = function () {
        var self = this;
        var deferred = $.Deferred();

        self.form.fields.isInactive.setOptions([
            { name: "Active", value: 0 },
            { name: "Inactive", value: 1 }
        ]);

        $.ajax(config.adminApi("PlayerManager/GetAddPlayerData")).done(function (response) {
            var data = response.data;
            self.form.fields.licensee.setOptions(data.licensees);
            self.form.fields.gender.setOptions(data.genders);
            self.form.fields.title.setOptions(data.titles);
            self.form.fields.contactPreference.setOptions(data.contactMethods);
            self.form.fields.idStatus.setOptions(data.idStatuses);
            self.form.fields.securityQuestionId.setOptions(data.securityQuestions);
            deferred.resolve();
        });

        return deferred.promise();
    };

    ViewModel.prototype.formatDate = function (date, isDisplay) {
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();

        return year + "/" + formatDateNumber(month) + "/" + formatDateNumber(day);

        function formatDateNumber(number) {
            return number < 10
                ? "0" + number
                : number;
        }
    };

    ViewModel.prototype.loadBrands = function () {
        var self = this;
        var licenseeId = self.form.fields.licensee.value().id;

        $.ajax(config.adminApi("PlayerManager/GetAddPlayerBrands?licenseeId=" + licenseeId)).done(function (response) {
            self.form.fields.brand.setOptions(response.data.brands);
        });
    };

    ViewModel.prototype.loadBrandData = function () {
        var self = this;
        var brand = self.form.fields.brand.value();
        self.playerPrefix(brand.playerPrefix);

        $.ajax(config.adminApi("PlayerManager/GetAddPlayerBrandData?brandId=" + brand.id)).done(function (response) {
            var data = response.data;
            self.form.fields.country.setOptions(data.countries);
            self.form.fields.currency.setOptions(data.currencies);
            self.form.fields.culture.setOptions(data.cultures);
        });
    };

    var naming = {
        gridBodyId: "player-grid",
        editUrl: config.adminApi("PlayerManager/Add")
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    ViewModel.prototype.serializeForm = function () {
        //TODO: Remove licensee from form.
        var data = this.form.getDataObject();
        return JSON.stringify(data);
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        response.data = "app:players.created";
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:playerManager.list.view"));
        $("#player-grid").trigger("reload");
    };

    return ViewModel;
});