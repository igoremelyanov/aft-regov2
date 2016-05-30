﻿define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);

        efu.setupBrandField2(this);

        this.assignControl = new efu.AssignControl();

        var productsField = form.makeField("products", this.assignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:brand.noAssignedProduct"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        }));
	    
        productsField.setSerializer(function() {
            var ids = [];
            var products = productsField.value();

            for (var i = 0; i < products.length; i++) {
                ids[i] = products[i].id;
            }
            return ids;
        });

        efu.publishIds(this, "brand-product-", ["licensee", "brand"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand"]);

        this.isActive = ko.observable();
    }

    ViewModel.prototype.load = function (deferred) {
        var self = this;
        var formFields = self.form.fields;

        var getLicenseesUrl = function () {
            return "Licensee/GetLicensees";
        };

        var getBrandsUrl = function () {
            return "Licensee/GetActiveAndInactiveBrandsOnly?licensee=" + self.fields.licensee().id;
        };

        efu.loadLicensees2(getLicenseesUrl, formFields.licensee, function () {
            var licensees = formFields.licensee.options();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:common.noBrand"));
                self.messageClass("alert-danger");
                return;
            } else {
                self.message(null);
                self.messageClass(null);
            }
            var licenseeId = efu.getBrandLicenseeId(shell);
            efu.selectLicensee2(formFields.licensee, licenseeId);

            efu.loadBrands2(getBrandsUrl, formFields.brand, function () {
                var brands = formFields.brand.options();
                if (brands == null || brands.length == 0) {
                    // TODO report error, etc.
                }
                var brandId = shell.brand().id();
                efu.selectBrand2(formFields.brand, brandId);

                self.loadProducts(function () {
                    formFields.licensee.value.subscribe(function () {
                        efu.loadBrands2(getBrandsUrl, formFields.brand);
                    });

                    formFields.brand.value.subscribe(function () {
                        self.loadProducts();
                    });

                    deferred.resolve();
                });
            });
        });
    };

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function () {
        var deferred = $.Deferred();
        this.load(deferred);
        return deferred;
    };

    ViewModel.prototype.loadProducts = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("BrandProduct/getAssignData?brandId=" + brandId)).done(function (response) {
                self.assignControl.availableItems(response.data.availableProducts);
                self.assignControl.assignedItems(response.data.assignedProducts);

                efu.callCallback(callback, callbackOwner);

                self.assignControl.assignedItems.isModified(false);
                self.isActive(response.data.isActive);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        gridBodyId: "brand-product-list",
        editUrl: config.adminApi("BrandProduct/Assign")
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        response.data = "app:product.successfullyAssigned";
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:product.viewAssignedProducts"));
    };

    return ViewModel;
});