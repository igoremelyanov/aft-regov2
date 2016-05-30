define(["i18next"], function (i18n) {
    var nav = require('nav');
    var security = require('security/security');

    require([ 
        '/Scripts/swipebox/js/jquery.swipebox.js'
    ], function(data) {

    });

    

    var serial = 0;

    var viewModel = function () {
        var vmSerial = serial++;

        this.remarksId = ko.observable("verify-deposit-request-Remark-" + vmSerial);

        this.id = ko.observable();
        this.licensee = ko.observable();
        this.brand = ko.observable();
        this.username = ko.observable();
        this.playerName = ko.observable();
        this.transactionNumber = ko.observable();
        this.playerAccountName = ko.observable();
        this.playerAccountNumber = ko.observable();
        this.referenceNumber = ko.observable();
        this.amount = ko.observable();
        this.currencyCode = ko.observable();
        this.bankName = ko.observable();
        this.bankAccountId = ko.observable();
        this.bankAccountName = ko.observable();
        this.bankAccountNumber = ko.observable();
        this.bankProvince = ko.observable();
        this.bankBranch = ko.observable();
        this.status = ko.observable();
        this.transferType = ko.observable();
        this.depositType = ko.observable();
        this.offlineDepositType = ko.observable();
        this.paymentMethod = ko.observable();
        this.playerRemark = ko.observable();
        this.action = ko.observable();
        this.documentUris = ko.observableArray();
        this.isRemarkRequired = ko.computed(function (_this) {
            return function () {
                return _this.action() === 'unverify';
            }
        }(this));
        this.remark = ko.observable().extend({
            validation: {
                validator: function (_this) {
                    return function (val) {
                        return (_this.isRemarkRequired() && val != null) || !_this.isRemarkRequired();
                    }
                }(this),
                message: 'The field is required.'
            }
        });
        this.selectedReason = ko.observable().extend({ required: true });
        this.unverifyReasons = ko.observableArray();
        this.idFrontImage = ko.observable();
        this.unverifyReason = ko.observable();
        this.idBackImage = ko.observable();
        this.receiptImage = ko.observable();
        this.created = ko.observable();
        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.displayMessage = ko.observable(false);
        this.messageClass = ko.observable();
        this.availableBankAccounts = ko.observableArray();
        this.selectedBankAccountId = ko.observable();
        this.selectedBankAccount = ko.computed((function (_this) {
            return function () {
                var account = _.find(_this.availableBankAccounts(), function (elem) {
                    return elem.id === _this.selectedBankAccountId();
                });
                return account;
            }
        })(this));
        this.errors = ko.validation.group(this);

        this.close = function () {
            nav.close();
        };

        this.loadOfflineDeposit = function (callback) {
            var self = this;

            $.ajax('/offlineDeposit/GetBankAccounts?depositId=' + self.id())
                .done(function (accountsResponse) {
                    self.availableBankAccounts(accountsResponse.data.bankAccounts);

                    $.ajax('/offlineDeposit/get/' + self.id())
                        .done(function (response) {
                            ko.mapping.fromJS(response.data, {}, self);
                            self.remark.isModified(false);
                            self.selectedBankAccountId(response.data.identifier);

                            if (self.idFrontImage())
                                self.documentUris.push({
                                    uri: 'image/Show?fileId=' + self.idFrontImage() + '&playerId=' + response.data.playerId,
                                    title: 'Id Front'
                                });

                            if (self.idBackImage())
                                self.documentUris.push({
                                    uri: 'image/Show?fileId=' + self.idBackImage() + '&playerId=' + response.data.playerId,
                                    title: 'Id Back'
                                });

                            if (self.receiptImage())
                                self.documentUris.push({
                                    uri: 'image/Show?fileId=' + self.receiptImage() + '&playerId=' + response.data.playerId,
                                    title: 'Receipt'
                                });

                            if (callback != null && callback != undefined)
                                callback();
                        });
                });
        };

        this.activate = function (data) {
            this.action(data.action);
            this.id(data.requestId);
            this.loadOfflineDeposit();
        };

        this.compositionComplete = function (data) {
            $('.swipebox').swipebox();
        };

        this.submit = function () {
            var self = this;
            if (!self.isValid()) {
                self.errors.showAllMessages();
                return;
            }
            var params = {
                id: self.id(),
                remark: self.remark()
            };

            if (this.action() === 'unverify')
                params.unverifyReason = self.selectedReason().code();

            if (this.action() === 'verify')
                params.bankAccountId = self.selectedBankAccountId();

            $.post('/offlineDeposit/' + this.action(), params,
                function (response) {
                    self.loadOfflineDeposit(function () {
                        if (typeof response.result != "undefined" && response.result == "failed") {
                            self.messageClass("alert-danger");
                            self.message(response.data);
                            self.displayMessage(true);
                        } else {
                            self.messageClass("alert-success");
                            self.message(i18n.t(response.data));
                            self.displayMessage(true);
                            self.submitted(true);
                            var title = self.action() == "verify"
                                ? i18n.t("app:payment.offlineDepositRequest.viewVerified")
                                : i18n.t("app:payment.offlineDepositRequest.viewUnverified");
                            nav.title(title);

                            $('#deposit-verify-grid').trigger("reload");

                            if (self.action() == 'verify')
                                $('#deposit-approve-grid').trigger("reload");
                            else
                                $('#offline-deposit-confirm-grid').trigger("reload");
                        }
                    });
                });
        };
    };

    return viewModel;
});

