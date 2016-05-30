define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    jgu = require "JqGridUtil"
    security = require "security/security"
    withdrawalLogDialogViewModel = require "withdrawal/withdrawal-log-dialog"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
        playerInfo: (data, event)->
            id = event.target.id
            nav.open path: "player-manager/info", title: i18n.t("app:playerManager.list.playerInfo"), data: playerId: id
        avcStatus: (data, event)->
            id = event.target.id
            dialog = new withdrawalLogDialogViewModel(id, "/OfflineWithdraw/AutoVerificationStatus")
            dialog.show()
        riskStatus: (data, event)->
            id = event.target.id
            dialog = new withdrawalLogDialogViewModel(id, "/OfflineWithdraw/RiskProfileCheckStatus")
            dialog.show()
        riskTemplate: (val)->
            if (val != '-')
                return val+'(<a data-id="@Id" style="color: #428bca" data-bind="click: $root.riskStatus" href="#">details</a>)'
        avcTemplate: (val)->
            if (val != '-')
                return val+'(<a data-id="@Id" style="color: #428bca" data-bind="click: $root.avcStatus" href="#">details</a>)'
            
            val

        documents: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Documents", data: id: @rowId(), event: "documents", gridId: "#withdraw-verify-grid"
        investigate: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Investigate", data: id: @rowId(), event: "investigate", gridId: "#withdraw-verify-grid"
        view: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "View", data: id: @rowId(), event: "view", gridId: "#withdraw-verify-grid"
        verify: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Verify", data: id: @rowId(), event: "verify", gridId: "#withdraw-verify-grid"
        cancel: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Cancel", data: id: @rowId(), event: "cancel", gridId: "#withdraw-verify-grid"
        unverify: ->
            nav.open path: "withdrawal/withdrawal-verify", title: "Unverify", data: id: @rowId(), event: "unverify", gridId: "#withdraw-verify-grid"