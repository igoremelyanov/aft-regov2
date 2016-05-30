﻿define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    userModel = require "admin/admin-manager/model/user-model"
    config = require "config"
    
    class ResetPasswordDialog extends baseViewModel
        constructor: (userId) ->
            super
            @userId = ko.observable userId
            @SavePath = config.adminApi("AdminManager/ResetPassword")
            @contentType "application/json"
        
        onsave: (data) =>
            #toastr.success i18N.t "app:admin.messages.passwordResetSuccessful"
            @success i18N.t "app:admin.messages.passwordResetSuccessful"
            @Model.mapfrom(data.data)
            @isReadOnly false
            
        activate: (data) ->
            super
            
            @Model = new userModel()
            @Model.ignore "allowedBrands", "currencies"
            @Model.ignoreClear "username", "firstName", "lastName", "allowedBrands"
            _id = @userId()
            
            $.get config.adminApi("AdminManager/GetEditData"), id: _id
            .done (data) =>
                @Model.mapfrom(data.user)
                
        cancel: ->
            dialog.close @
            
        show: ->
            dialog.show @