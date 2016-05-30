define(['plugins/dialog', 'i18next'], function (dialog, i18n) {
    var config = require("config");

    var customModal = function (parent, id, newPassword, sendBy) {
        var self = this;
        this.parent = ko.observable(parent);
        this.id = ko.observable(id);

        this.generateNewPassword = ko.observable(newPassword == '');

        this.newPassword = ko.observable(newPassword).extend({ minLength : 6, maxLength: 12 });
        this.sendBy = ko.observable(sendBy);

        this.message = ko.observable();
        this.errorMessages = ko.observableArray();
        this.submitted = ko.observable(false);
        this.errors = ko.validation.group(self);
        this.hasError = ko.observable();
    };

    customModal.prototype.ok = function () {
        if (this.isValid()) {
            var self = this;
            var action = config.adminApi('PlayerManager/SendNewPassword');
            self.hasError(false);
            self.errorMessages.removeAll();
            $.ajax(action, {
                data: ko.toJSON({ PlayerId: self.id(), NewPassword: self.newPassword(), SendBy: self.sendBy() }),
                type: "post",
                contentType: "application/json",
                success: function (response) {
                    if (response.result === "success") {
                        self.message(i18n.t("app:players.newPasswordSent"));
                            self.submitted(true);
                    } else {
                        for (var i = 0; i < response.fields.length; i++) {
                            var field = response.fields[i];
                            for (var j = 0; j < field.errors.length; j++) {
                                self.errorMessages.push(field.errors[j]);
                            }
                        }
                        self.hasError(true);
                    }
                }
            });

            dialog.close();
        }
        else {
            this.errors.showAllMessages();
        }
    };

    customModal.prototype.cancel = function () {
        dialog.close(this, { isCancel: !this.submitted() });
    };

    customModal.show = function (parent, id, newPassword, sendBy) {
        return dialog.show(new customModal(parent, id, newPassword, sendBy));
    };

    return customModal;
});