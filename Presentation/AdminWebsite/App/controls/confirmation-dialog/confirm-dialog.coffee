define (reguire) ->
    dialog = require 'plugins/dialog'
    i18n = require 'i18next'
    
    class ConfirmDialog
        constructor: (settings) ->
            @caption = ko.observable 'Caption'
            if settings.caption
                @caption settings.caption
        
            @yesCaption = ko.observable i18n.t 'common.yes'
            if settings.yesCaption
                @yesCaption settings.yesCaption
                
            @noCaption = ko.observable i18n.t 'common.no'
            if settings.noCaption
                @noCaption settings.noCaption
                
            @yesAction = () -> {}
            if settings.yesAction
                @yesAction = settings.yesAction
                
            @noAction = () -> {}
            if settings.noAction
                @noAction = settings.noAction
                
            @question = ko.observable('Question?')
            if settings.question
                @question settings.question

        show : ->
            dialog.show @

        noClick: ->
            if @noAction
                @noAction()
            dialog.close @

        yesClick: ->
            if @yesAction
                @yesAction()
            dialog.close @