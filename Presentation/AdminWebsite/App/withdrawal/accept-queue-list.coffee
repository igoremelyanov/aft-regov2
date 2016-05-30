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
            nav.open path: "withdrawal/withdrawal-verify", title: "View", data: id: @rowId(), event: "view", gridId: "#accept-grid"
        accept: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Accept", data: id: @rowId(), event: "accept", gridId: "#accept-grid"
        cancel: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Cancel", data: id: @rowId(), event: "cancel", gridId: "#accept-grid"
        
        reloadAcceptanceQueueGrid = ->
            $("#accept-grid .ui-jqgrid-btable").trigger "reloadGrid"
        
        $(document).on "acceptance_queue_changed": ->
            setTimeout reloadAcceptanceQueueGrid, 1800