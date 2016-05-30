define(['nav', 'i18next', "EntityFormUtil", "shell", "CommonNaming", "JqGridUtil", "ResizeManager", "durandal/app"], function (nav, i18n, efu, shell, CommonNaming, jgu, ResizeManager,app) {
    var config = require("config");
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        var self = this;


        self.mainPageGridId = "payment-level-settings-grid";
        self.playerGridId = "set-payment-level-grid";
        self.resultGridId = "set-payment-level-result-grid";
        self.playerGridSelectAllId = "cb_set-payment-level-grid"+vmSerial+"-list";
        self.message = ko.observable();
        self.submitted = ko.observable(false);
        self.errors = ko.validation.group(self);
        self.hasError = ko.observable();        

        self.playerGrid = {};
        self.playerGrid.naming = new CommonNaming(self.playerGridId + vmSerial);
        self.resultGrid = {};
        self.resultGrid.naming = new CommonNaming(self.resultGridId + vmSerial);

        self.remarks = ko.observable();
        self.paymentLevels = ko.observableArray();
        self.brandId = ko.observable();
        self.currency = ko.observable();
        self.playerData = ko.observableArray();
        self.newPaymentLevel = ko.observable();
        self.playerPaymentLevelIds = ko.observableArray();
    }


    ViewModel.prototype.activate = function (data) {
        var self = this;

        var deferred = $.Deferred();

        self.playerData(data.playerData);
        self.brandId(data.brandId);
        self.currency(data.currency);
        var ids = ko.utils.arrayMap(self.playerData(), function (player) { return player.PaymentLevelId });
        self.playerPaymentLevelIds(ko.utils.arrayGetDistinctValues(ids));

        self.load(deferred);

        return deferred.promise();
    };

    ViewModel.prototype.load = function (deferred) {
        var self = this;

        self.loadPaymentLevels();

        deferred.resolve();
    };  

    ViewModel.prototype.save = function () {
        var self = this;

        if (!self.isValid()) {
            self.errors.showAllMessages();
            return;
        }

        self.hasError(false);
        app.showMessage(i18n.t("app:playerManager.paymentLevelSettings.confirmMessage"), i18n.t("app:playerManager.paymentLevelSettings.title"), ["Yes", "No"])
            .then(function (result) {
                if (result === "Yes") {
                    var action = config.adminApi('PlayerManager/ChangePlayersPaymentLevel');

                    var grid = self.playerGrid.grid;
                    var newPaymentLevelId = self.newPaymentLevel();
                    var selectedData = grid.jqGrid('getGridParam', 'selarrrow');
                    var data = ko.toJSON({
                        PlayerIds: selectedData,
                        PaymentLevelId:newPaymentLevelId ,
                        Remarks: self.remarks
                    });

                    $.ajax(action, {
                        data: data,
                        type: "post",
                        contentType: "application/json",
                        success: function (response) {
                            if (response.result === "success") {
                                self.message(i18n.t("app:playerManager.paymentLevelSettings.paymentLevelChanged"));
                                
                                var newPaymentLevel =ko.utils.arrayFirst(self.paymentLevels(), function(item) {
                                    return item.id === newPaymentLevelId;
                                });                                                                       

                                var resultData = [];
                                ko.utils.arrayForEach(selectedData, function (playerid) {
                                    var row = grid.getRowData(playerid);                                    
                                    row.NewPaymentLevelName = newPaymentLevel.name;
                                    resultData.push(row);
                                });

                                self.loadResultGrid(resultData);
                                self.submitted(true);
                                nav.title(i18n.t("app:common.view"));
                                $("#" +self.mainPageGridId).trigger("reload");
                            } else {
                                self.hasError(true);
                                if ("fields" in response) {
                                    var fields = response.fields;
                                    var err = fields[0].errors[0];
                                    self.message(i18n.t("app:playerManager.paymentLevelSettings." + err));
                                }
                            }
                        }
                    });
                } else {
                    dialog.close(self);
                }
            });
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    ViewModel.prototype.loadPaymentLevels = function () {
        var self = this;
        $.ajax(config.adminApi("PlayerManager/GetPaymentLevels?brandId=" + self.brandId() + "&currency=" + self.currency()), {
            success: function (response) {
                var allPaymentLevels = response.paymentLevels;

                self.paymentLevels(allPaymentLevels.slice(0));

                self.paymentLevels.remove(function (thisPaymentLevel) {
                    return self.playerPaymentLevelIds.indexOf(thisPaymentLevel.id)>-1;
                });                
            }
        });
    };

    ViewModel.prototype.compositionComplete = function () {
        var self = this;
        self.loadPlayerGrid();
    };

    ViewModel.prototype.loadPlayerGrid = function () {
        var self = this;
        var gridNaming = self.playerGrid.naming;        

        jgu.makeDefaultGrid(self, gridNaming, {
            data: self.playerData(),
            datatype: 'local',
            colModel: [
                jgu.defineColumn("LicenseeName", 150, i18n.t("app:common.licensee")),
                jgu.defineColumn("BrandName", 150, i18n.t("app:brand.name")),
                jgu.defineColumn("Username", 150, i18n.t("app:common.username")),
                jgu.defineColumn("FullName", 150, i18n.t("app:common.playerName")),
                jgu.defineColumn("PaymentLevelName", 150, i18n.t("app:payment.level"))
            ],
            rowNum: 100,
            multiselect: true,
            loadComplete: function () {
                self.playerGrid.grid = $('#' + gridNaming.gridBodyId);
            },
            onSelectRow: function (rowId, status) {},
            onSelectAll: function (rowIds, status) {}
        });

        jgu.applyStyle("#" + gridNaming.pagerId);

        self.resizeManager = new ResizeManager(gridNaming);
        self.resizeManager.fixedHeight = 330;
        self.resizeManager.bindResize();

        self.selectAllPlayer();
    };

    ViewModel.prototype.loadResultGrid = function (resultData) {
        var self = this;
        var gridNaming = self.resultGrid.naming;

        jgu.makeDefaultGrid(self, gridNaming, {
            data: resultData,
            datatype: 'local',
            colModel: [
                jgu.defineColumn("LicenseeName", 150, i18n.t("app:common.licensee")),
                jgu.defineColumn("BrandName", 150, i18n.t("app:brand.name")),
                jgu.defineColumn("Username", 150, i18n.t("app:common.username")),
                jgu.defineColumn("FullName", 150, i18n.t("app:common.playerName")),
                jgu.defineColumn("PaymentLevelName", 200, i18n.t("app:payment.previousLevel")),
                jgu.defineColumn("NewPaymentLevelName", 200, i18n.t("app:payment.newLevel"))
            ],
            rowNum: 100,
            loadComplete: function () {
                self.resultGrid.grid = $('#' + gridNaming.gridBodyId);
            },
            onSelectAll: function (rowIds, status) { },
            beforeSelectRow: function () { return false; }
        });
    };

    ViewModel.prototype.selectAllPlayer = function () {
        var self = this;
        $('#'+self.playerGridSelectAllId).trigger('click').attr('checked', 'checked');
    }
    return ViewModel;
});