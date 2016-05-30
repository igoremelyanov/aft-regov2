define(['nav', 'i18next', 'EntityFormUtil', 'CommonNaming', 'JqGridUtil', 'ResizeManager', 'shell', 'security/security'],
    function (nav, i18n, efu, CommonNaming, jgu, ResizeManager, shell, security) {
        var config = require("config");
        var serial = 0;

        function ViewModel() {
            var vmSerial = serial++;
            this.serial = vmSerial;

            this.disabled = ko.observable(true);
            this.editMode = ko.observable(false);

            var form = new efu.Form(this);
            this.form = form;

            efu.setupLicenseeField(this);

            efu.setupBrandField(this);

            this.brandId = function () {
                return this.form.fields.brand.fields.brand;
            };

            this.isLicenseeLocked = ko.computed(function () {
                return (security.licensees() && security.licensees().length <= 1)
                 && !security.isSuperAdmin();
            });

            this.isSingleBrand = ko.computed(function () {
                return security.isSingleBrand();
            });

            this.currencies = ko.observableArray();
            form.makeField("currency", ko.observable());

            form.makeField("code", ko.observable().extend({
                required: true,
                maxLength: 20,
                pattern: {
                    message: i18n.t("app:payment.codeCharError"),
                    params: "^[a-zA-Z0-9-_]+$"
                }
            }));

            form.makeField("name", ko.observable().extend({
                required: true,
                maxLength: 50,
                pattern: {
                    message: i18n.t("app:payment.nameCharError"),
                    params: "^[a-zA-Z0-9-_ ]+$"
                }
            }));

            form.makeField("enableOfflineDeposit", ko.observable(true)).defaultTo(false);
            form.makeField("enableOnlineDeposit", ko.observable(true)).defaultTo(false);

            form.makeField("maxBankFee", ko.observable(0).extend({
                formatDecimal: 2,
                validatable: true,
                required: true,
                min: {
                    message: "Entered amount must be greater or equals to 0.",
                    params: 0
                },
                max: {
                    message: "Entered amount is bigger than allowed.",
                    params: 2147483647
                }
            }));

            form.makeField("bankFeeRatio", ko.observable(0).extend({
                formatDecimal: 2,
                validatable: true,
                required: true,
                min: {
                    message: "Entered amount must be greater or equals to 0.",
                    params: 0
                },
                max: {
                    message: "Entered amount is bigger than allowed.",
                    params: 2147483647
                }
            }));

            form.makeField("isDefault", ko.observable()).defaultTo(false);

            this.bankAccountGrid = {};
            this.bankAccountGrid.naming = new CommonNaming("payment-level-bank-accounts-" + this.serial);

            this.paymentGatewaySettingGrid = {};
            this.paymentGatewaySettingGrid.naming = new CommonNaming("payment-level-payment-gateway-settings" + this.serial);

            var self = this;
            this.selectedBankAccounts = {};
            form.makeField("bankAccounts", null).setClear(function () {
                self.selectedBankAccounts = {};
                self.bankAccountGrid.grid.jqGrid("resetSelection");

            }).setSave(function () {
                var bankAccounts = [];
                for (var rowid in self.selectedBankAccounts) {
                    bankAccounts.push(rowid);
                }
                self.fields.bankAccounts = bankAccounts;
            });

            form.makeField("internetSameBankSelection", ko.observableArray());
            form.makeField("atmSameBankSelection", ko.observableArray());
            form.makeField("counterDepositSameBankSelection", ko.observableArray());
            form.makeField("internetDifferentBankSelection", ko.observableArray());
            form.makeField("atmDifferentBankSelection", ko.observableArray());
            form.makeField("counterDepositDifferentBankSelection", ko.observableArray());

            this.selectedPaymentGatewaySettings = {};
            form.makeField("paymentGatewaySettings", null).setClear(function () {
                self.selectedPaymentGatewaySettings = {};
                self.paymentGatewaySettingGrid.grid.jqGrid("resetSelection");
            }).setSave(function () {
                var paymentGatewaySettings = [];
                for (var rowid in self.selectedPaymentGatewaySettings) {
                    paymentGatewaySettings.push(rowid);
                }
                self.fields.paymentGatewaySettings = paymentGatewaySettings;
            });

            efu.publishIds(this, "payment-level-", ["licensee", "brand", "currency", "code", "name", "enableOfflineDeposit", "enableOnlineDeposit", "default", "maxBankFee", "bankFeeRatio"], "-" + vmSerial);

            efu.addCommonMembers(this);

            form.publishIsReadOnly(["licensee", "brand", "currency", "code", "isDefault", "maxBankFee", "bankFeeRatio"]);

            this.accountIdSearchId = ko.observable("payment-level-bank-account-id-search-" + this.serial);
            this.searchButtonId = ko.observable("payment-level-bank-account-search-button-" + this.serial);
        }

        var naming = {
            gridBodyId: "payment-levels-list",
            editUrl: "PaymentLevel/Save"
        };
        efu.addCommonEditFunctions(ViewModel.prototype, naming);

        function fillRequiredToUploadBankFields(fieldName) {
            var colNameAttr = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
            fieldName = fieldName.charAt(0).toLowerCase() + fieldName.slice(1) + "Selection";

            var rowIdArray = $('input[type=checkbox][col-name="' + colNameAttr + '"]:checked')
                .map(function () {
                    return $(this).attr('rowId');
                }).get();

            _.forEach(rowIdArray, function (id) {
                this.fields[fieldName].push(id);
            }, this);
        }

        var commonSave = ViewModel.prototype.save;
        ViewModel.prototype.save = function () {
            this.form.onSave();

            _.forEach(["internetSameBank",
            "atmSameBank",
            "counterDepositSameBank",
            "internetDifferentBank",
            "atmDifferentBank",
            "counterDepositDifferentBank"], fillRequiredToUploadBankFields, this);

            commonSave.call(this);
        };

        var commonHandleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
        ViewModel.prototype.handleSaveSuccess = function (response) {
            var self = this;
            self.fields.id = response.paymentLevelId;
            self.bankAccountGrid.grid
                .jqGrid("setGridParam", {
                    postData: {
                        filters: JSON.stringify({
                            groupOp: "AND",
                            rules: [
                                { field: "PaymentLevels", data: response.paymentLevelId },
                                { field: "Bank.Brand", data: this.brand().id() },
                                { field: "CurrencyCode", data: self.fields.currency() }
                            ]
                        })
                    },
                    beforeSelectRow: function () {
                        return false;
                    }
                }).trigger("reloadGrid");

            self.paymentGatewaySettingGrid.grid.jqGrid("setGridParam", {
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [
                            { field: "Brand", data: this.brand().id() },
                            { field: "PaymentLevels", data: response.paymentLevelId }
                        ]
                    })
                },
                beforeSelectRow: function () {
                    return false;
                }
            }).trigger("reloadGrid");

            nav.closeViewTab("brandId", this.brandId());
            commonHandleSaveSuccess.call(this, response);

            nav.title(i18n.t("app:payment.viewLevel"));
            $("#" + naming.gridBodyId).trigger("reload");
        };

        var commonHandleSaveFailure = ViewModel.prototype.handleSaveFailure;
        ViewModel.prototype.handleSaveFailure = function (response) {
            /*        var fields = response.fields;
                    for (var i = 0; i < fields.length; ++i) {
                        var name = fields[i].name;
                        if (name == "Brand" || name == "BankAccounts") {
                            var error = JSON.parse(fields[i].errors[0]);
                            this.message(i18n.t(error.text, error.variables));
                            this.messageClass("alert-danger");
                            break;
                        }
                    }*/
            commonHandleSaveFailure.call(this, response);
        };

        ViewModel.prototype.clear = function () {
            this.form.clear();
        };

        ViewModel.prototype.loadAccountGrid = function () {
            var self = this;
            var gridNaming = this.bankAccountGrid.naming;

            function selectBankAccount(rowid, status) {
                if (!status) {
                    delete self.selectedBankAccounts[rowid];
                } else {
                    self.selectedBankAccounts[rowid] = true;
                }
            }

            function formatter(cellvalue, options, rowObject) {
                var html = '<input data-bind="disable: submitted() || disabled()" rowId="' + options.rowId + '" col-name="' + options.colModel.name + '" type="checkbox"' + (cellvalue === "True" ? ' checked="checked"' : '') + '/>';

                return html;
            }

            jgu.makeDefaultGrid(this, gridNaming, {
                url: "/PaymentLevel/GetBankAccounts",
                colModel: [
                    jgu.defineColumn("AccountId", 150, i18n.t("app:payment.bankAccountId")),
                    jgu.defineColumn("Bank.Name", 150, i18n.t("app:payment.bankName")),
                    jgu.defineColumn("Branch", 120, i18n.t("app:payment.branch")),
                    jgu.defineColumn("AccountName", 150, i18n.t("app:payment.accountName")),
                    jgu.defineColumn("AccountNumber", 150, i18n.t("app:payment.accountNumber")),
                    jgu.defineColumn("InternetSameBank", 150, "Internet (Same Bank)", { formatter: formatter }),
                    jgu.defineColumn("AtmSameBank", 150, "Atm (Same Bank)", { formatter: formatter }),
                    jgu.defineColumn("CounterDepositSameBank", 200, "Counter Deposit (Same Bank)", { formatter: formatter }),
                    jgu.defineColumn("InternetDifferentBank", 150, "Internet (Different Bank)", { formatter: formatter }),
                    jgu.defineColumn("AtmDifferentBank", 150, "Atm (Different Bank)", { formatter: formatter }),
                    jgu.defineColumn("CounterDepositDifferentBank", 200, "Counter Deposit (Different Bank)", { formatter: formatter })
                ],
                sortname: "AccountId",
                sortorder: "asc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [
                            { field: "Bank.Brand", data: this.brand() != null ? this.brand().id() : "00000000-0000-0000-0000-000000000000" },
                            { field: "CurrencyCode", data: this.fields.currency() },
                            { field: "PaymentLevels", data: (self.editMode() === true) ? null : self.fields.id }
                        ]
                    })
                },
                multiselect: true,
                loadComplete: function () {
                    self.bankAccountGrid.grid = $('#' + gridNaming.gridBodyId);
                    if (self.submitted()) {
                        self.bankAccountGrid.grid.jqGrid("hideCol", "cb");
                    } else {
                        self.bankAccountGrid.grid.jqGrid("showCol", "cb");
                        for (var rowid in self.selectedBankAccounts) {
                            self.bankAccountGrid.grid.jqGrid("setSelection", rowid);
                        }
                    }

                    // apply binding to dynamically inserted ui parts
                    for (var rowid in self.selectedBankAccounts) {
                        var element = document.getElementById(rowid);
                        if (element != null) {
                            ko.cleanNode(element);
                            ko.applyBindings(self, element);
                        }
                    }
                },
                onSelectRow: selectBankAccount,
                onSelectAll: function (aRowids, status) {
                    for (var i = 0; i < aRowids.length; ++i) {
                        selectBankAccount(aRowids[i], status);
                    }
                }
            });

            $("#" + this.searchButtonId()).click(function (event) {
                jgu.setParamReload(self.bankAccountGrid.grid, "AccountId", $("#" + self.accountIdSearchId()).val());
                event.preventDefault();
            });

            jgu.applyStyle("#" + gridNaming.pagerId);



            this.resizeManager = new ResizeManager(gridNaming);
            this.resizeManager.fixedHeight = 400;
            this.resizeManager.bindResize();
        };

        ViewModel.prototype.loadPaymentGatewaySettingsGrid = function () {
            var self = this;
            var gridNaming = this.paymentGatewaySettingGrid.naming;

            function selectPaymentGatewaySetting(rowid, status) {
                if (!status) {
                    delete self.selectedPaymentGatewaySettings[rowid];
                } else {
                    self.selectedPaymentGatewaySettings[rowid] = true;
                }
            }

            jgu.makeDefaultGrid(this, gridNaming, {
                url: "/PaymentLevel/GetPaymentGatewaySettings",
                colModel: [
                    jgu.defineColumn("OnlinePaymentMethodName", 300, i18n.t("app:payment.paymentGateway.onlinePaymentMethodName"))
                ],
                sortname: "OnlinePaymentMethodName",
                sortorder: "asc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [
                            { field: "Brand", data: this.brand() != null ? this.brand().id() : "00000000-0000-0000-0000-000000000000" },
                            { field: "PaymentLevels", data: (this.editMode() === true) ? null : self.fields.id }
                        ]
                    })
                },
                multiselect: true,
                loadComplete: function () {
                    self.paymentGatewaySettingGrid.grid = $('#' + gridNaming.gridBodyId);
                    if (self.submitted()) {
                        self.paymentGatewaySettingGrid.grid.jqGrid("hideCol", "cb");
                    } else {
                        self.paymentGatewaySettingGrid.grid.jqGrid("showCol", "cb");
                        for (var rowid in self.selectedPaymentGatewaySettings) {
                            self.paymentGatewaySettingGrid.grid.jqGrid("setSelection", rowid);
                        }
                    }
                },
                onSelectRow: selectPaymentGatewaySetting,
                onSelectAll: function (aRowids, status) {
                    for (var i = 0; i < aRowids.length; ++i) {
                        selectPaymentGatewaySetting(aRowids[i], status);
                    }
                }
            });

            jgu.applyStyle("#" + gridNaming.pagerId);

            this.resizeManager = new ResizeManager(gridNaming);
            this.resizeManager.fixedHeight = 400;
            this.resizeManager.bindResize();
        };

        ViewModel.prototype.activate = function (data) {
            var deferred = $.Deferred();
            this.fields.id = data ? data.id : null;
            this.editMode(data != undefined ? data.editMode : true);
            this.submitted(this.editMode() == false);
            if (this.fields.id) {
                this.loadPaymentLevel(deferred);
            } else {
                this.load(deferred);
            }
            return deferred.promise();
        };

        ViewModel.prototype.compositionComplete = function () {
            this.loadAccountGrid();
            this.loadPaymentGatewaySettingsGrid();
        };

        ViewModel.prototype.loadPaymentLevel = function (deferred) {
            var self = this;
            $.ajax("PaymentLevel/GetById?id=" + this.fields.id, {
                success: function (response) {
                    self.load(deferred, response);
                }
            });
        };

        ViewModel.prototype.load = function (deferred, paymentLevel) {
            var self = this;

            if (paymentLevel) {
                self.fields.name(paymentLevel.name);
                self.fields.code(paymentLevel.code);
                self.form.fields.code.isSet(true);
                self.fields.enableOfflineDeposit(paymentLevel.enableOfflineDeposit);
                self.fields.enableOnlineDeposit(paymentLevel.enableOnlineDeposit);
                self.fields.isDefault(paymentLevel.isDefault);
                self.fields.maxBankFee(paymentLevel.maxBankFee);
                self.fields.bankFeeRatio(paymentLevel.bankFeeRatio);
                self.form.fields.isDefault.isSet(true);
                var bankAccounts = paymentLevel.bankAccounts;
                for (var i = 0; i < bankAccounts.length; ++i) {
                    self.selectedBankAccounts[bankAccounts[i]] = true;
                }
                var paymentGatewaySettings = paymentLevel.paymentGatewaySettings;
                for (var i = 0; i < paymentGatewaySettings.length; ++i) {
                    self.selectedPaymentGatewaySettings[paymentGatewaySettings[i]] = true;
                }
            }

            this.loadLicensees(function () {
                var licensees = self.licensees();
                if (licensees == null || licensees.length == 0) {
                    self.message(i18n.t("app:payment.noBrandForLevel"));
                    self.messageClass("alert-danger");
                    self.disabled(true);
                    deferred.resolve();
                    return;
                } else {
                    self.message(null);
                    self.messageClass(null);
                    self.disabled(false);
                }
                var licenseeId;
                if (paymentLevel) {
                    licenseeId = paymentLevel.brand.licensee.id;
                    self.form.fields["licensee"].isSet(true);
                }
                else {
                    licenseeId = efu.getBrandLicenseeId(shell);
                }
                efu.selectLicensee(self, licensees, licenseeId);

                this.loadBrands(function () {
                    var brands = self.brands();
                    var brandId = paymentLevel ? paymentLevel.brand.id : shell.brand().id();
                    efu.selectBrand(self, brands, brandId);
                    if (paymentLevel) {
                        self.form.fields["brand"].isSet(true);
                    }

                    this.loadCurrencies(function () {
                        if (paymentLevel) {
                            self.fields.currency(paymentLevel.currency);
                            self.form.fields["currency"].isSet(true);
                        }

                        this.licensee.subscribe(function () {
                            self.loadBrands();
                        });

                        this.brand.subscribe(function () {
                            self.loadCurrencies();
                            self.loadPaymentGatewaySettings();
                        });

                        this.fields.currency.subscribe(function () {
                            self.selectedBankAccounts = {};
                            self.loadBankAccounts();
                        });

                        deferred.resolve();
                    }, this);
                }, this);
            }, this);
        };

        ViewModel.prototype.getLoadLicenseesUrl = function () {
            return "Licensee/Licensees?useFilter=true";
        };

        ViewModel.prototype.loadLicensees = function (callback, callbackOwner) {
            efu.loadLicensees(this, callback, callbackOwner);
        };

        ViewModel.prototype.getLoadBrandsUrl = function () {
            return config.adminApi("Brand/Brands?useFilter=true&licensees=" + this.licensee().id());
        };

        ViewModel.prototype.loadBrands = function (callback, callbackOwner) {
            efu.loadBrands(this, callback, callbackOwner);
        };

        ViewModel.prototype.loadCurrencies = function (callback, callbackOwner) {
            var self = this;
            if (self.brand() != null) {
                $.ajax("PaymentLevel/GetBrandCurrencies", {
                    data: { brandId: self.brand().id() },
                    success: function (response) {
                        ko.mapping.fromJS(response, {}, self);
                        var selected = self.currencies()[0];
                        if (selected) {
                            self.form.fields["currency"].defaultTo(selected);
                            self.fields.currency(selected);
                            self.fields.currency.valueHasMutated();
                        }

                        if (callback) {
                            callback.call(callbackOwner);
                        }
                    }
                });
            } else if (callback) {
                callback.call(callbackOwner);
            }
        };

        ViewModel.prototype.loadBankAccounts = function () {
            if (this.bankAccountGrid.grid) {
                jgu.setParamsReload(this.bankAccountGrid.grid, [
                    { field: "Bank.Brand", data: this.brand().id() },
                    { field: "CurrencyCode", data: this.fields.currency() }
                ]);
            }
        };

        ViewModel.prototype.loadPaymentGatewaySettings = function () {
            var self = this;
            if (self.paymentGatewaySettingGrid.grid) {
                jgu.setParamsReload(self.paymentGatewaySettingGrid.grid, [
                    { field: "Brand", data: self.brand().id() },
                    { field: "PaymentLevels", data: (self.editMode() === true) ? null : self.fields.id }
                ]);
            }
        };
        ViewModel.prototype.detach = function () {
            this.resizeManager.unbindResize();
        };

        return ViewModel;
    });