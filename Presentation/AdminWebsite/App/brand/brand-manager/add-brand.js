define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;        
        var form = new efu.Form(this);
        this.form = form;        

        efu.setupLicenseeField2(this);
        var licenseeField = form.fields.licensee;
        licenseeField.value.subscribe(function(licensee) {
            form.fields.enablePlayerPrefix.value(true);
        });
        licenseeField.setSerializer(function () {
            return licenseeField.value().id;
        });

        var typeField = form.makeField("type", ko.observable()).hasOptions();
        typeField.setSerializer(function () {
            return typeField.value();
        })
        .setDisplay(ko.computed(function () {
            return typeField.value();
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:brand.nameCharError"),
                params: "^[a-zA-Z0-9-_.]+$"
            }
        }));

        form.makeField("code", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:brand.codeCharError"),
                params: "^[a-zA-Z0-9]+$"
            }
        }));

        form.makeField("email", ko.observable().extend({
            required: true,
            email: true
        }));

        form.makeField("smsNumber", ko.observable().extend({
            required: true,
            number: true,
            minLength: 8,
            maxLength: 15
        }));

        var websiteUrlField = form.makeField("websiteUrl", ko.observable().extend({
            required: true,
        }));

        websiteUrlField.setSerializer(function() {
            var url = websiteUrlField.value();
            return url.indexOf('http://') === 0 || url.indexOf('https://') === 0
                ? url
                : 'http://' + url;
        });

        form.makeField("enablePlayerPrefix", ko.observable(true));

        form.makeField("playerPrefix", ko.observable().extend({
            maxLength: 3,
            pattern: {
                message: i18n.t("app:brand.playerPrefixCharError"),
                params: "^[a-zA-Z0-9_.]+$",
            },
            required: {
                onlyIf: function() {
                    return form.fields.enablePlayerPrefix.value();
                }
            }
        }));

        form.makeField("playerActivationMethod", ko.observable().extend({ required: true })).hasOptions();

        form.makeField("internalAccounts", ko.observable(0).extend({
            required: true,
            min: 0,
            max: 10
        }));        
       
        var timeZoneIdFiedld = form.makeField("timeZoneId", ko.observable().extend({ required: true })).hasOptions();
        timeZoneIdFiedld.setSerializer(function () {
            return timeZoneIdFiedld.value().id;
        })
        .setDisplay(ko.computed(function () {
            var timezone = timeZoneIdFiedld.value();
            return timezone ? timezone.displayName : "";
        }));

        efu.publishIds(this, "brand-", ["licensee", "type", "name", "code", "email", "smsNumber", "websiteUrl", "enablePlayerPrefix", "playerPrefix", "playerActivationMethod", "internalAccounts", "timeZoneId"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee"]);

        form.fields.enablePlayerPrefix.value.subscribe(function (playerPrefixEnabled) {
            if (!playerPrefixEnabled) form.fields.playerPrefix.value(null);
        });

        this.licenseePrefixRequired = ko.computed(function() {
            var licensee = form.fields.licensee.value();
            var licenseePrefixRequired = licensee && licensee.prefixRequired;

            return licenseePrefixRequired;
        });
    }

    ViewModel.prototype.activate = function () {
        var self = this;
        var deferred = $.Deferred();

        $.ajax(config.adminApi("Brand/GetAddData")).done(function (response) {
            self.form.fields.licensee.setOptions(response.licensees);
            self.form.fields.type.setOptions(response.types);
            self.form.fields.timeZoneId.setOptions(response.timeZones);
            self.form.fields.playerActivationMethod.setOptions(response.playerActivationMethods);
            deferred.resolve();
        });        

        return deferred.promise();
    };

    var naming = {
        gridBodyId: "brand-grid",
        editUrl: config.adminApi("Brand/Add")
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.handleSaveSuccess = function (response) {
        shell.reloadData();
        $('#' + naming.gridBodyId).trigger("reload");
        nav.close();
        nav.open({
            path: 'brand/brand-manager/view-brand',
            title: i18n.t("app:brand.view"),
            data: {
                id: response.data.id,
                message: i18n.t("app:brand.brandCreated")
            }
        });
    };

    ViewModel.prototype.clear = function () {
        var enablePlayerPrefix = this.form.fields.enablePlayerPrefix.value();
        this.form.clear();
        this.form.fields.enablePlayerPrefix.value(enablePlayerPrefix);
    };

    return ViewModel;
});