 define (require) -> 
    nav = require "nav"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    userModel = require "admin/admin-manager/model/user-model"
    config = require "config"
        
    reloadGrid = ->
        $('#user-grid').trigger "reload"
    
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = config.adminApi("AdminManager/UpdateUser")
            @contentType "application/json"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
        onsave: ->   
            reloadGrid()
            @success i18N.t "app:admin.messages.userSuccessfullyUpdated"
            
            @Model.clearLock yes
            
            nav.title i18N.t "app:admin.adminManager.viewUser"
            @readOnly true  
                
        activate: (data) ->
            if not data.id()?
                @readOnly true
                return
            super 
         
            @Model = new userModel()
            @Model.ignore "password", "passwordConfirmation"
            
            $.get config.adminApi("AdminManager/GetEditData"), id: data.id
            .done (data) =>
                @Model.mapfrom data.user  
                @Model.licensees data.licensees
    
    new ViewModel()