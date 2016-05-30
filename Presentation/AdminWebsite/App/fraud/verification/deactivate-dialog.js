define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, id) {
        var self = this;

        self.parent = ko.observable(parent);
        self.id = ko.observable(id);

        self.isLoading = ko.observable(false);
        self.resultIsSuccess = ko.observable(false);
        self.submitted = ko.observable(false);
        self.cancelCloseText = ko.computed(function () {
            return self.submitted()
                ? i18n.t("app:common.close")
                : i18n.t("app:common.cancel");
        });
        self.remarks = ko.observable().extend({ required: true });
        self.canChangeStatus = ko.computed(function () {
            return !self.isLoading() && !self.submitted();
        });
        self.errors = ko.validation.group(self);
        self.errorsArray = ko.observableArray();
        
    };

    customModal.prototype.clear = function () {
        this.remarks("");
    };

    customModal.prototype.changeStatus = function () {
        var self = this;
        self.isLoading(true);
        self.errorsArray.removeAll();

        if (!self.isValid()) {
            self.errors.showAllMessages();
            self.isLoading(false);
            return;
        }

        var data = {
            id: self.id(),
            remarks: self.remarks()
        };

        $.post("/AutoVerification/Deactivate", data, function (response) {
            $('#verification-manager-list').trigger("reload");
            self.resultIsSuccess(response.Success === true);  
            if (!self.resultIsSuccess()) {
                for (var i = 0; i < response.fields.length; ++i) {
                    var error = response.fields[i].errors[0];
                    //TODO: Here I have to determine the error codes that could be returned and add them to the sources
                    self.errorsArray.push(i18n.t("app:fraud.autoVerification.errors.deactivation." + error));
                }
            }
          
            self.submitted(true);
            self.isLoading(false);
        });
    };

    customModal.prototype.cancelClose = function () {
        dialog.close(this);
    };

    customModal.show = function (parent, id) {
        return dialog.show(new customModal(parent, id));
    };

    return customModal;
});