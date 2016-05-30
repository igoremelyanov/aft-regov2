# CoffeeScript
define ["./bonusBase", "bonus/bonusCommon", "komapping", "nav", "config", "i18next", "dateBinders"], 
(bonusBase, common, mapping, nav, config, i18N) ->
    class AddEditBonusModel extends bonusBase
        constructor: ->
            super()
            #Small hack to suppress validation on initial form enter
            @ActiveTo.subscribe =>	@ActiveTo.isModified no
            @vLicenseeName = ko.computed => @getBrandField "LicenseeName"
            @vBrandName = ko.computed => @getBrandField "BrandName"
            @vRequireBonusCode = ko.computed => 
                thisTemplate = ko.utils.arrayFirst @templates(), (template) => template.Id is @TemplateId()
                thisTemplate?.RequireBonusCode
            @Code.extend required: 
                    params: true
                    message: i18N.t "common.requiredField"
                    onlyIf: @vRequireBonusCode
            
        getBrandField: (fieldName) =>
            thisTemplate = ko.utils.arrayFirst @templates(), (template) => template.Id is @TemplateId()
            if thisTemplate?
                thisTemplate[fieldName]
            else
                @emptyCaption()
                
        submit: =>
            if @isValid()
                objectToSend = JSON.parse mapping.toJSON @, 
                    ignore: common.getIgnoredFieldNames @

                $.ajax
                    type: "POST"
                    url: config.adminApi("Bonus/CreateUpdate")
                    data: ko.toJSON(objectToSend)
                    dataType: "json"
                    contentType: "application/json"
                .done (response) => @processResponse response
            else
                @errors.showAllMessages()
            
        processResponse: (response) =>
            if response.Success
                @cancel()
                $(document).trigger "bonuses_changed"

                obj = id: response.BonusId
                obj[if @Id() is undefined then "created" else "edited"] = yes
                nav.open
                    path: "bonus/bonus-manager/view-bonus"
                    title: i18N.t "bonus.bonusManager.view"
                    data: obj
            else
                response.Errors.forEach (element) =>
                    if element.PropertyName is "Bonus"
                        @serverErrors [element.ErrorMessage]
                    else
                        setError @[element.PropertyName], element.ErrorMessage
                @errors.showAllMessages()
            
        activate: (input) =>
            $(document).on "bonus_templates_changed", @reloadTemplates
            $.get config.adminApi("Bonus/GetRelatedData"), id: input?.id
                .done (response) =>
                    @templates response.Templates
                    if input?
                        mapping.fromJS response.Bonus, {}, @
                        @TemplateId.valueHasMutated()
        detached: => $(document).off "bonuses_changed", @reloadTemplates