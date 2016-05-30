define ["nav", 'durandal/app', "shell", "bonus/bonusCommon", "i18next", "config", "controls/grid"],
(nav, app, shell, common, i18N, config) ->    
    class ViewModel
        constructor: ->
            @shell = shell
            @config = config
            @i18N = i18N
            @playerId = ko.observable()
            @redemptionId = ko.observable null
            @canBeCanceled = ko.observable no
            @search = ko.observable()
        activationFormatter: -> common.redemptionActivationFormatter @ActivationState
        typeFormatter: -> common.typeFormatter @TemplateType
        reloadGrid: ->     $('#redemption-grid').trigger "reload"
        compositionComplete: =>
            $("#redemption-grid").on "gridLoad selectionChange", (e, row) =>
                @redemptionId row.id
                @canBeCanceled row.data.CanBeCanceled is "true"
            $(document).on "redemptions_changed", @reloadGrid
        detached: => $(document).off "redemptions_changed", @reloadGrid
        openViewTab: ->
            if @redemptionId()
                nav.open
                    path: "player-manager/info/bonus/view-redemption"
                    title: i18N.t "playerManager.bonusHistory.view"
                    data: 
                        playerId: @playerId()
                        redemptionId: @redemptionId()
                        
        activate: (data) -> @playerId data.playerId
        cancel: ->
            if @redemptionId()
                app.showMessage i18N.t('playerManager.bonusHistory.messages.cancelRedemption'),
                    i18N.t('playerManager.bonusHistory.messages.confirmCancellation'),
                    [ text: i18N.t('common.booleanToYesNo.true'), value: yes
                    text: i18N.t('common.booleanToYesNo.false'), value: no ],
                    false,
                    style: width: "450px"
                .then (confirmed) =>
                    if confirmed
                        $.post config.adminApi("/BonusHistory/Cancel"),
                            playerId: @playerId()
                            redemptionId: @redemptionId()
                        .done (data) =>
                            if data.Success
                                $(document).trigger "redemptions_changed"
                                @canBeCanceled no
                                app.showMessage(
                                    i18N.t("playerManager.bonusHistory.messages.canceledSuccessfully"), 
                                    i18N.t("playerManager.bonusHistory.cancel"), 
                                    [i18N.t("common.close")])
                            else
                                app.showMessage data.Errors[0].ErrorMessage, i18N.t("common.error"), [i18N.t("common.close")]