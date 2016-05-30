define ['plugins/dialog'], (dialog) ->
    class WithdrawalLogDialogViewModel
        constructor: (id, @url) ->
            @id = ko.observable(id)
            @statuses= ko.observableArray()
            @playerName = ko.observable()
            @brandName = ko.observable()
            @licenseeName = ko.observable()
            @statusSuccess = ko.observable()
            
            #This is boolean property that indicates if this is Auto verification check(true) or Risk profile check(false).
            @avcVerificationType = ko.observable()              
                
            @loadStatus id
        loadStatus: (id)->
            $.post @url,
                id: @id()
            .done (data) =>
                @statuses data.statuses
                @playerName data.PlayerName
                @brandName data.BrandName
                @licenseeName data.LicenseeName
                @statusSuccess data.StatusSuccess 
                @avcVerificationType(@url == "/OfflineWithdraw/AutoVerificationStatus")
        close: ->
            dialog.close @
        show: ->
            dialog.show @
