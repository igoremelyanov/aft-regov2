define (require) ->
    nav = require "nav"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    userModel = require "admin/admin-manager/model/user-model"
    config = require "config"

    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = config.adminApi("AdminManager/ResetPassword")
            @contentType "application/json"
            
        onsave: (data) =>   
            #toastr.success i18N.t "app:admin.messages.passwordResetSuccessful"
            @success i18N.t "app:admin.messages.passwordResetSuccessful"
            @Model.mapfrom(data.data)
            @readOnly true
                
        activate: (data) ->
            super
            
            @Model = new userModel()
            @Model.ignore "allowedBrands", "currencies"
            @Model.ignoreClear "username", "firstName", "lastName", "allowedBrands"
            
            $.get config.adminApi("AdminManager/GetEditData"), id: data.id
            .done (data) =>
                @Model.mapfrom(data.user)
