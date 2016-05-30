# CoffeeScript
define ['i18next', 'shell', 'moment', 'config', 'controls/grid'], (i18n, shell, moment, config) ->           
    class TransactionsAdv
        constructor: ->
            @shell = shell
            @moment = moment
            @config = config
            [@playerId, @currentWallet] = ko.observables()
            [@wallets, @walletsName] = ko.observableArrays()
            @transactionTypeNames = ko.observable {}
            
        activate: (data) ->
            @playerId data.playerId
            $.when [
                $.get @config.adminApi('PlayerInfo/GetWalletTemplates'), playerId: @playerId()
                .done (response) =>
                    @walletsName.push wallet.name for wallet in response
                
                $.get @config.adminApi('PlayerInfo/GetTransactionTypes')
                .done (response) =>
                    for item in response.types
                        @transactionTypeNames()[item.name] = i18n.t "playerManager.transactions.types.#{item.name}"

            ]...
                
        attached: (view) ->
                $grid = findGrid view
                $("form", view).submit ->
                    $grid.trigger "reload"
                    off
                
        typeFormatter: -> i18n.t "playerManager.transactions.types.#{@Type}"