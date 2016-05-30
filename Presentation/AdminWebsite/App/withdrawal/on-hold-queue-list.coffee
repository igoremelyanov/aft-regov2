define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    jgu = require "JqGridUtil"
    security = require "security/security"
    autoVerificationStatusDialog = require "withdrawal/withdrawal-log-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
        playerInfo: (data, event)->
            id = event.target.id
            nav.open path: "player-manager/info", title: i18n.t("app:playerManager.list.playerInfo"), data: playerId: id
        view: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "View", data: id: @rowId(), event: "view", gridId: "#on-hold-grid"
        verify: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Verify", data: id: @rowId(), event: "verify", gridId: "#on-hold-grid"
        cancel: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Cancel", data: id: @rowId(), event: "cancel", gridId: "#on-hold-grid"
        unverify: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Unverify", data: id: @rowId(), event: "unverify", gridId: "#on-hold-grid"