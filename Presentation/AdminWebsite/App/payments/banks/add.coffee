define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    addBankModel = require "payments/banks/models/add-bank-model"

    class AddViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/Banks/Add"
            @Model = new addBankModel()
        
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
                key: data.data
                data: 
                    id: data.data
                    message: i18n.t "app:banks.created"