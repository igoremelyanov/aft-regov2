define(['plugins/dialog', 'i18next'], function (dialog, i18n) {
    var config = require("config");

    var customModal = function (parent, playerId, licensee, brand, currentPaymentLevel, paymentLevels) {
        var self = this;

        self.parent = ko.observable(parent);
        self.playerId = ko.observable(playerId);
        self.licensee = ko.observable(licensee);
        self.brand = ko.observable(brand);

        var paymentLevel = ko.utils.arrayFirst(paymentLevels(), function (thisPaymentLevel) { return thisPaymentLevel.id() === currentPaymentLevel; });
        var optionsPaymentLevels = ko.observableArray();
        optionsPaymentLevels(paymentLevels.slice(0));
        optionsPaymentLevels.remove(function (thisPaymentLevel) {
            return thisPaymentLevel.id() == currentPaymentLevel;
        });

        self.currentPaymentLevel = ko.observable(paymentLevel.name());
        self.newPaymentLevel = ko.observable(paymentLevel.id());
        self.remarks = ko.observable("");
        self.paymentLevels = optionsPaymentLevels;

        self.message = ko.observable();
        self.submitted = ko.observable(false);
        self.errors = ko.validation.group(self);
        self.hasError = ko.observable();

        self.closeText = ko.computed(function () {
            return self.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
        });

        self.closeClass = ko.computed(function () {
            return self.submitted() ? "btn-default" : "btn-primary";
        });
    };

    customModal.prototype.ok = function () {
        var self = this;

        if (!self.isValid()) {
            self.errors.showAllMessages();
            return;
        }

        self.hasError(false);
        dialog.showMessage(i18n.t("app:playerManager.changePaymentLevel.confirmMessage"), i18n.t("app:playerManager.changePaymentLevel.title"), ["Yes", "No"])
            .then(function (result) {
                if (result === "Yes") {
                    var action = config.adminApi('PlayerManager/ChangePaymentLevel');

                    var data = ko.toJSON({
                        PlayerId: self.playerId,
                        PaymentLevelId: self.newPaymentLevel,
                        Remarks: self.remarks
                    });

                    $.ajax(action, {
                        data: data,
                        type: "post",
                        contentType: "application/json",
                        success: function(response) {
                            if (response.result === "success") {
                                self.message(i18n.t("app:players.paymentLevelChanged"));
                                self.submitted(true);

                                var newPaymentLevel = ko.utils.arrayFirst(self.paymentLevels(), function(thisPaymentLevel) {
                                    return thisPaymentLevel.id() == self.newPaymentLevel();
                                });

                                if (self.parent().constructor.name === "Account") {
                                    self.parent().paymentLevel(newPaymentLevel);
                                } else
                                    self.parent().form.fields.paymentLevel.value(newPaymentLevel);
                                $("#player-grid").trigger("reload");
                            } else {
                                self.hasError(true);
                                if ("fields" in response) {
                                    var fields = response.fields;
                                    var err = fields[0].errors[0];
                                    self.message(i18n.t("app:playerManager.changePaymentLevel."+err));
                                }                                
                            }
                        }
                    });
                } else {
                    dialog.close(self);
                }
            });
    };

    customModal.prototype.cancel = function () {
        dialog.close(this, { isCancel: !this.submitted() });
    };

    customModal.show = function (parent, id, licensee, brand, currentPaymentLevel, paymentLevels) {
        return dialog.show(new customModal(parent, id, licensee, brand, currentPaymentLevel, paymentLevels));
    };


    return customModal;
});