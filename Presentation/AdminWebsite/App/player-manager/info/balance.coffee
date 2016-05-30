define (require) ->
    config = require "config"

    class Balance
    
        constructor: ->
            @playerId = ko.observable()
            @balance = ko.observable({})
            @gameBalance = ko.observable({})
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.ajax config.adminApi('/PlayerInfo/GetBalances?playerId=' + @playerId())
                .done (data)->
                    self.balance {
                        currency: data.balance.currency,
                        mainBalance: data.balance.mainBalance,
                        bonusBalance: data.balance.bonusBalance,
                        playableBalance: data.balance.playableBalance,
                        freeBalance: data.balance.freeBalance,
                        totalBonus: data.balance.totalBonus,
                        depositCount: data.balance.depositCount,
                        totalDeposit: data.balance.totalDeposit,
                        withdrawalCount: data.balance.withdrawalCount,
                        totalWithdrawal: data.balance.totalWithdrawal,
                        totalWin: data.balance.totalWin,
                        totalLoss: data.balance.totalLoss,
                        totalAdjustments: data.balance.totalAdjustments,
                        totalCreditsRefund: data.balance.totalCreditsRefund,
                        totalCreditsCancellation: data.balance.totalCreditsCancellation,
                        totalChargeback: data.balance.totalChargeback,
                        totalChargebackReversals: data.balance.totalChargebackReversals,
                        totalWager: data.balance.totalWager,
                        averageWagering: data.balance.averageWagering,
                        averageDeposit: data.balance.averageDeposit,
                        maxBalance: data.balance.maxBalance,
                        totalWagering: data.depositWagering.totalWagering,
                        wageringCompleted: data.depositWagering.totalWagering - data.depositWagering.wageringRequired,
                        wageringRequired: data.depositWagering.wageringRequired,
                    }

                    self.gameBalance {
                        product: data.gameBalance.product,
                        balance: data.gameBalance.balance,
                        bonusBalance: data.gameBalance.bonusBalance,
                        bettingBalance: data.gameBalance.bettingBalance,
                        totalBonus: data.gameBalance.totalBonus
                    }