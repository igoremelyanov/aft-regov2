define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    editBankModel = require "payments/banks/models/edit-bank-model"

    class EditViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/Banks/Edit"
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:banks.validation." + error

        onsave: (data) ->
            $(document).trigger "bank_changed"
            nav.close()
            nav.open
                path: "payments/banks/view"
                title: i18n.t "app:common.view"
                key: @Model.id()
                data: 
                    id: @Model.id()
                    message: i18n.t "app:banks.updated"
                    
        activate: (data) =>
            super
            $.get "/Banks/View?id=" + data.id
                .done (response) =>
                    @Model = new editBankModel(response.data)