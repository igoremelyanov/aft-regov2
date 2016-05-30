# CoffeeScript
define ['./bonusTier'], (BonusTier) ->
    class RewardTier
        constructor: (args...) ->
            @CurrencyCode = ''
            @BonusTiers = ko.observableArray()
            @removeBtnIsEnabled = ko.computed => @BonusTiers().length > 1
            @RewardAmountLimit = ko.observable 0
            
            @vRewardAmountLimit = ko.computed
                read: () => if @RewardAmountLimit() is 0 then '' else @RewardAmountLimit()
                write: @RewardAmountLimit                
                                                                                                                
            if args.length is 1
                @CurrencyCode = args[0].CurrencyCode
                @BonusTiers.push new BonusTier(tier) for tier in args[0].BonusTiers          
                @vRewardAmountLimit args[0].RewardAmountLimit
        
        addBonusTier: => @BonusTiers.push new BonusTier()
        removeBonusTier: (tier) => @BonusTiers.remove tier