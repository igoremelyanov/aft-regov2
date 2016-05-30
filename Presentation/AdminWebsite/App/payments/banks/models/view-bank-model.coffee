define (require) ->
    baseModel = require "base/base-model"

    class ViewTemplateModel extends baseModel
        constructor: ->
            super
            @message = @makeField()
            @licenseeName = @makeField()
            @brandName = @makeField()
            @bankId = @makeField()
            @name = @makeField()
            @country = @makeField()
            @remarks = @makeField()