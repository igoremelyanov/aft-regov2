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
        release: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Release", data: id: @rowId(), event: "release", gridId: "#release-grid"
        cancel: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Cancel", data: id: @rowId(), event: "cancel", gridId: "#release-grid"
            
        reloadReleaseQueueGrid = ->
            $("#release-grid .ui-jqgrid-btable").trigger "reloadGrid"
        
        $(document).on "release_queue_changed": ->
            setTimeout reloadReleaseQueueGrid, 1800