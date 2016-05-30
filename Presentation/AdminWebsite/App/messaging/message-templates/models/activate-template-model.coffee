define (require) ->
    i18n = require "i18next"
    baseModel = require "base/base-model"

    class ActivateTemplateModel extends baseModel
        constructor: ->
            super

            @id = @makeField()

            @remarks = @makeField().extend
                required: true