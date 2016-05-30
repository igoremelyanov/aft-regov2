define (require) ->
    require "controls/grid"
    nav = require "nav"
    IdentityRemarksDialog = require "player-manager/info/remarks-dialog/remarks-dialog"
    confirmation = require "player-manager/info/confirm-dialog/confirm-dialog"
    config = require "config"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            @config = require "config"
            @playerId = ko.observable()
            @selectedRowId = ko.observable()
            @status = ko.observable()
            
            @verifyEnabled = ko.computed () =>
                @selectedRowId() && @status() == 'Pending';
                
            @unverifyEnabled = ko.computed () =>
                @selectedRowId() && @status() == 'Pending';
                
        activate: (data) ->
            @playerId data.playerId
            
        attached: (view) ->
            self = this
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $(view).on "click", ".identity-remark", ->
                id = $(@).parents("tr").first().attr "id"
                remark = $(@).attr "title"
                (new IdentityRemarksDialog id, remark).show ->
                    $grid.trigger "reload"
                
            $(view).on "click", ".jqgrow", ->
                self.selectedRowId $(@).attr "id"
                table = $grid.find('.ui-jqgrid-btable')
                data = table.jqGrid('getRowData', self.selectedRowId())
                self.status data.VerificationStatus

        upload: ->
            nav.open
                path: 'player-manager/documents-upload/upload-identification-doc'
                title: "Upload document"
                data: {
                    playerId: @playerId()
                }
                
        verify: ->
            confirm = new confirmation((onSuccessFunc, onFailedFunc)=>
                $.ajax(config.adminApi('PlayerInfo/VerifyIdDocument'), {
                    type: "post",
                    data: ko.toJSON({
                        id: @selectedRowId()
                    })
                    contentType: "application/json",
                    success: (response) ->
                        if response.result == 'success'
                            $('#id-documents-grid').trigger('reload')
                            if onSuccessFunc
                                onSuccessFunc()
                        else
                            if onFailedFunc
                                onFailedFunc(response.data)
                })
            , "Are you sure you want to verify player's submitted documents?"
            , 'Documents have been sucessfully verified.'
            )
            confirm.show()
            
        unverify: ->
            confirm = new confirmation((onSuccessFunc)=>
                $.ajax(config.adminApi('PlayerInfo/UnverifyIdDocument'), {
                    type: "post",
                    data: ko.toJSON({
                        id: @selectedRowId()
                    })
                    contentType: "application/json",
                    success: (response) ->
                        $('#id-documents-grid').trigger('reload')
                        if onSuccessFunc
                            onSuccessFunc()
                })
            , "Are you sure you want to unverify player's submitted documents?"
            , 'Documents have been sucessfully unverified.'
            )
            confirm.show()   