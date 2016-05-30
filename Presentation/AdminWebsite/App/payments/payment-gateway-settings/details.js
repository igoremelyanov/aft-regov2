define(['nav', 'i18next', "EntityFormUtil", "shell"], function (nav, i18n, efu, shell) {
    var config = require("config");
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        this.disabled = ko.observable(false);
        this.editMode = ko.observable(false);
        this.pageMode = ko.observable('');

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);
        this.form.fields["licensee"]
            .setSerializer(function () {
                return this.value().id;
            });
        efu.setupBrandField2(this);

        form.makeField("id", ko.observable()).lockValue(true);
        this.id = function () {
            return this.form.fields.id.value();
        };
        form.makeField("channel", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));

        form.makeField("entryPoint", ko.observable().extend({
            required: true, minLength: 1, maxLength: 100,
            pattern: {
                params: '^[A-Za-z0-9-_\.\/\:]*$',
                message: i18n.t("app:common.validationMessages.urlFormatError")                    
            }
        }));
        form.makeField("remarks", ko.observable().extend({ required: true, minLength: 1, maxLength: 200 }));

        var onlinePaymentMethodNameField = form.makeField("onlinePaymentMethodName",ko.observable().extend({
            required: true, minLength: 2, maxLength: 100,
            pattern: {
                message: i18n.t("app:common.validationMessages.onlyAlphabeticSpaces").replace("__fieldName__", i18n.t("app:payment.paymentGateway.onlinePaymentMethodName")),
                params: '^[A-Za-z0-9 ]*$',
            }
        }));
        onlinePaymentMethodNameField.setDisplay(ko.computed(function () {
            return onlinePaymentMethodNameField.value();
        }));

        var paymentGatewayNameField = form.makeField("paymentGatewayName", ko.observable().extend({ required: true })).hasOptions();
        paymentGatewayNameField.setSerializer(function () {
            return this.value().id;
        });

        efu.publishIds(this, "payment-gateway-settings-", [
            "licensee",
            "brand",
            "onlinePaymentMethodName",
            "paymentGatewayName",
            "channel",
            "entryPoint",
            "remarks"
        ], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "onlinePaymentMethodName", "paymentGatewayName", "channel"]);
    }

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function (data) {
        var self = this;

        var deferred = $.Deferred();
        self.fields.id(data ? data.id : null);
        self.editMode(data ? data.editMode : false);
        self.pageMode(data ? data.pageMode : 'View');
        if (self.fields.id()) {
            self.loadPaymentGatewaySettings(deferred);
            self.submitted(this.editMode() == false);
        } else {
            self.load(deferred);
        }
        return deferred.promise();
    };

    ViewModel.prototype.loadPaymentGatewaySettings = function (deferred) {
        var self = this;
        $.ajax("PaymentGatewaySettings/GetById?id=" + this.fields.id(), {
            success: function (response) {
                self.load(deferred, response.data);
            }
        });
    };

    ViewModel.prototype.load = function (deferred, data) {
        var self = this;
        var licenseeField = this.form.fields.licensee;
        var brandField = this.form.fields.brand;
        var paymentGatewayNameField = this.form.fields.paymentGatewayName;
        if (data) {
            self.fields.onlinePaymentMethodName(data.onlinePaymentMethodName);
            self.form.fields.onlinePaymentMethodName.isSet(true);
            self.fields.paymentGatewayName(data.paymentGatewayName);
            self.fields.channel(data.channel);
            self.form.fields.channel.isSet(true);
            self.fields.entryPoint(data.entryPoint);
            self.fields.remarks(data.remarks);
        }

        var getLicenseesUrl = function () {
            return "Licensee/Licensees?useFilter=true";
        };

        var getBrandsUrl = function () {
            return config.adminApi("Brand/Brands?useFilter=true&licensees=" + licenseeField.value().id);
        };

        efu.loadLicensees2(getLicenseesUrl, licenseeField, function () {
            var licenseeId = efu.getBrandLicenseeId(shell);
            var licensees = licenseeField.options();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:common.noBrand"));
                self.messageClass("alert-danger");
                self.disabled(true);
                return;
            } else {
                self.message(null);
                self.messageClass(null);
                self.disabled(false);
            }
            if (data) {
                licenseeId = data.licenseeId;
                self.form.fields["licensee"].isSet(true);
            }
            efu.selectLicensee2(licenseeField, licenseeId);

            efu.loadBrands2(getBrandsUrl, brandField, function () {
                var brandId = data ? data.brandId : shell.brand().id();
                efu.selectBrand2(brandField, brandId);
                if (data) {
                    self.form.fields["brand"].isSet(true);
                }
                self.loadPaymentGateways(function() {                                      
                    if (data) {
                        paymentGatewayNameField.setDisplay(data.paymentGatewayName);
                        self.fields.paymentGatewayName(data.paymentGatewayName);
                        self.form.fields["paymentGatewayName"].isSet(true);

                        efu.selectOption(paymentGatewayNameField, function (item) {
                            return item.id == data.paymentGatewayName;
                        });
                    }
                });

                licenseeField.value.subscribe(function () {
                    efu.loadBrands2(getBrandsUrl, brandField);
                });

                brandField.value.subscribe(function () {                    
                    self.loadPaymentGateways();                    
                });
            });            
        });

        deferred.resolve();
    };

    ViewModel.prototype.loadPaymentGateways = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax("PaymentGatewaySettings/GetPaymentGateways?brandId=" + brandId).done(function (response) {
                if (response.data.paymentGateways.length === 0) {
                    self.message(i18n.t("app:payment.paymentGateway.paymentGatewaysNotFound"));
                    self.messageClass("alert-danger");
                    self.submitted(true);
                } else {
                    self.form.fields.paymentGatewayName.setOptions(response.data.paymentGateways);
                }

                efu.callCallback(callback, callbackOwner);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };
    var naming = {
        gridBodyId: "payment-gateway-settings-grid",
        editUrl: "PaymentGatewaySettings/save"
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    var commonSave = ViewModel.prototype.save;
    ViewModel.prototype.save = function () {
        this.form.onSave();
        commonSave.call(this);
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {        
        var self = this;
        nav.closeViewTab("id", this.id());
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:payment.viewPaymentGatewaySettings"));
        var message = (self.pageMode() === 'Add') ? 'CreatedSuccessfully' : 'UpdatedSuccessfully';
        self.message(i18n.t("app:payment.paymentGateway." + message));
        self.pageMode('View');
    };
    
    ViewModel.prototype.handleSaveFailure = function (response) {        
        var self = this;
        self.message(i18n.t("app:payment.paymentGateway." + response.data));
        self.messageClass("alert-danger");
    };

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    return ViewModel;
});