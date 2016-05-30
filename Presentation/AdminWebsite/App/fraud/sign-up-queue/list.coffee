define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    jgu = require "JqGridUtil"
    config = require "config"
    dateTimePicker = require "dateTimePicker"
    dateBinders = require "dateBinders"
    Dialog = require "fraud/sign-up-queue/dialog"
    
    class ViewModel
        constructor: ->
            @period = ko.observable()
            @username = ko.observable()
            @startDate = ko.observable()
            @endDate = ko.observable()
            @remarks = ko.observable()
            @tag = ko.observableArray()
            @checked = ko.observable(false)
            @fraudTypes = ko.observableArray()
            
            @getFilter = ()=>
                filter = {
                    username: @username(),
                    startDate: @startDate(),
                    endDate: @endDate(),
                    tags: @tag(),
                    period: @period()
                }
                return ko.toJSON filter

        playerInfo: (data, event)->
            id = event.target.id
            nav.open path: "player-manager/info", title: i18n.t("app:playerManager.list.playerInfo"), data: playerId: id
        close: ->
            nav.close()
        onChange: (data, event)->
            console.log data
        activate: (data) =>
            deferred = $.Deferred()
            $.ajax "signup/fraudtypes?", {
                success: (response) =>
                    @fraudTypes response
                    deferred.resolve()
            }
        attached: (view) =>
            @grid = findGrid view
                
        update: () =>
            self = @
            data = {data: []}
            $($("input[type=checkbox]:checked", @grid).get().reverse()).each ->
                tr = $(@).parents("tr")
                id = $("td:first", tr).text()
                val = $("select", tr).first().val()
                data.data.push {playerId: id, fraudTypeId: val}
                data.remarks = self.remarks()
            (new Dialog data).show ->
                    self.remarks ""
                    $(self.grid).trigger "reload"
