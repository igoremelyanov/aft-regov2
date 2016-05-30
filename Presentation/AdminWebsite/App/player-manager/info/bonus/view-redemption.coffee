# CoffeeScript
define ["nav", "komapping", "bonus/bonusCommon", "i18next", "config"], 
(nav, mapping, common, i18N, config) ->
    class ViewRedemptionModel
        constructor: ->
            @i18N = i18N
            @config = config
            [@LicenseeName, @BrandName, @Username, @BonusName, @ActivationState, 
            @RolloverState, @Amount, @LockedAmount, @Rollover, @activationData] = ko.observables()
            @vActivationState = ko.computed =>
                common.redemptionActivationFormatter @ActivationState()
            @vRolloverState = ko.computed =>
                common.redemptionRolloverFormatter @RolloverState()

        attached: (view) ->
                $grid = findGrid view
                $("form", view).submit ->
                    $grid.trigger "reload"
                    off
    
        activate: (activationData) ->
            @activationData activationData
            $.get config.adminApi('/BonusHistory/Get'), activationData
                .done (data) => mapping.fromJS data, {}, @
                
        eventTypeFormatter: -> i18N.t "playerManager.bonusHistory.eventTypes.#{@DataType}"
        eventDescriptionFormatter: -> 
            template = i18N.t "playerManager.bonusHistory.eventTemplates.#{@DataType}"
            if @Data?
                for field of @Data
                    template = template.replace "{#{field}}", @Data[field]
                
            template
            
        cancel: -> nav.close()