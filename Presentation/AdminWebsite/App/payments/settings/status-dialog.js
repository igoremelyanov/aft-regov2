﻿define(['i18next', 'plugins/dialog'], function (i18n, dialog) {

    var customModal = function (activated, id) {
        var self = this;
        self.activated = ko.observable(activated);
        self.remarks = ko.observable().extend({ required: true });
        self.submitted = ko.observable(false);
        self.action = ko.computed(function () {
            return self.activated()
                ? i18n.t("app:common.deactivate")
                : i18n.t("app:common.activate");
        });
        self.message = ko.observable();
        self.resultClass = ko.observable();
        self.errors = ko.validation.group(this);
        self.id = id;
    };

    customModal.prototype.changeStatus = function () {
        var self = this;

        if (self.isValid()) {
            var action;
            if (self.activated()) {
                action = '/paymentSettings/disable';
            } else {
                action = '/paymentSettings/enable';
            }
            $.post(action, { id: self.id, remarks: self.remarks() })
                .done(function (response) {
                    if (response.result === "success") {
                        self.message(!self.activated() ?
                            i18n.t("app:payment.settings.enabledSuccess") :
                            i18n.t("app:payment.settings.disabledSuccess"));

                        self.resultClass("alert alert-success");
                        self.submitted(true);
                    } else {
                        self.message(i18n.t("app:payment.settings."+response.data));
                        self.resultClass("alert alert-danger");
                        self.submitted(true);
                    }
                    $('#payment-settings-grid').trigger("reload");
                });
        } else
            self.errors.showAllMessages();
    };

    customModal.prototype.cancel = function () {
        dialog.close(this);
    };

    customModal.prototype.clear = function () {
        this.remarks("");
    };

    customModal.show = function (activated, id) {
        return dialog.show(new customModal(activated, id));
    };

    return customModal;
});