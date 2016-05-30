define ['plugins/dialog', 'config'], (dialog, config) ->
    class ActivateDeactivateBonusModal
        constructor: (@bonusId) ->
            [@remarks, @isActive, @error] = ko.observables()
        ok: ->
            $.ajax
                type: "POST"
                url: config.adminApi "Bonus/ChangeStatus"
                data: ko.toJSON
                    id: @bonusId
                    isActive: @isActive()
                    remarks: @remarks()
                contentType: "application/json"
            .done (data) =>
                if data.Success is false
                    @error data.Errors[0].ErrorMessage
                else
                    $(document).trigger "bonuses_changed"
                    @cancel()
            
        cancel: ->  dialog.close @
        show: (isActive) ->    
            @isActive isActive
            dialog.show @