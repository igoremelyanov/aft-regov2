define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    userModel = require "admin/admin-manager/model/user-model"
    security = require "security/security"
    config = require "config"
    
    reloadGrid = ->
        $('#user-grid').trigger "reload"
        
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = config.adminApi("AdminManager/CreateUser")
            @contentType "application/json"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
        onsave: ->
            reloadGrid()
            @success i18N.t "app:admin.messages.userSuccessfullyCreated"

            nav.title i18N.t "app:admin.adminManager.viewUser"
            @readOnly true  
            @Model.clearLock yes
                
        activate: =>
            super
            
            @Model = new userModel()
            
            $.get config.adminApi("AdminManager/GetEditData")
            .done (data) => 
                @Model.licensees data.licensees
                if @Model.isLicenseeLocked()
                    @Model.assignedLicensees security.licensees()
            
                @Model.clearLock no

    new ViewModel()