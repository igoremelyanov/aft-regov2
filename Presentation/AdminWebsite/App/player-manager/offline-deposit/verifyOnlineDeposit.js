define(["i18next"], function (i18n) {
    var nav = require('nav');
    var security = require('security/security');

    var serial = 0;

    var viewModel = function () {
        var vmSerial = serial++;

        this.remarksId = ko.observable("verify-online-deposit-request-Remarks-" + vmSerial);

        this.id = ko.observable();
        this.licensee = ko.observable();
        this.brandName = ko.observable();
        this.username = ko.observable();
        this.playerName = ko.observable();
        this.referenceCode = ko.observable();
        this.status = ko.observable();
        this.paymentMethod = ko.observable();
        this.currencyCode = ko.observable();
        this.amount = ko.observable();
        this.depositType = ko.observable();
        this.dateSubmitted = ko.observable();
        this.remarks = ko.observable().extend({ required: true, minLength: 1, maxLength: 200 });

        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.displayMessage = ko.observable(false);
        this.messageClass = ko.observable();
        this.action = ko.observable();
        this.errors = ko.validation.group(this);

        this.close = function () {
            nav.close();
        };

        this.loadOnlineDeposit = function (callback) {
            var self = this;
            $.ajax('/OnlineDeposit/Get/' + self.id())
                .done(function (response) {
                    ko.mapping.fromJS(response.data, {}, self);
                    self.remarks.isModified(false);

                    if (callback != null && callback != undefined)
                        callback();
                });
        };

        this.activate = function (data) {
            this.action(data.action);
            this.id(data.requestId);
            this.loadOnlineDeposit();
        };

        this.getActionString = function () {
            var self = this;
            switch (self.action() ) {
                case "verify":
                    return "Verified";
                case "unverify":
                    return "Unverified";
                case "reject":
                    return "Rejected";
                case "approve":
                    return "Approved";
                default:
                    return "";
            }            
        };
        
        this.submit = function () {
            var self = this;
            if (!self.isValid()) {
                self.errors.showAllMessages();
                return;
            }
            var params = {
                id: self.id(),
                remarks: self.remarks()
            };
            $.post('/onlineDeposit/' + this.action(), params,
                function (response) {
                    self.loadOnlineDeposit(function () {
                        if (typeof response.result != "undefined" && response.result == "failed") {
                            self.messageClass("alert-danger");
                            self.message(response.data);
                            self.displayMessage(true);
                        } else {
                            self.messageClass("alert-success");
                            var message = i18n.t("app:payment.depositSuccessfully" + self.getActionString());
                            self.message(message);
                            self.displayMessage(true);
                            self.submitted(true);
                            var title = i18n.t("app:payment.viewDeposit" + self.getActionString());
                            nav.title(title);

                            $('#deposit-verify-grid').trigger("reload");

                            $('#deposit-approve-grid').trigger("reload");
                        }
                    });
                })
                .fail(function (response) {
                    self.messageClass("alert-danger");
                    self.message(response.responseJSON.Message);
                    self.displayMessage(true);
                }
            );
        };
    };
    return viewModel;
});