# CoffeeScript
define ->
    class BonusTier
        constructor: (args...) ->
            @From = ko.observable 0
            @Reward = ko.observable 0
            @MaxAmount = ko.observable 0
            @NotificationPercentThreshold = ko.observable 0

            if args.length is 1
                @From args[0].From
                @Reward args[0].Reward
                @MaxAmount args[0].MaxAmount
                @NotificationPercentThreshold args[0].NotificationPercentThreshold
            
            @vFrom = ko.computed
                read: () => if @From() is 0 then '' else @From()
                write: @From
                
            @vReward = ko.computed
                read: () => if @Reward() is 0 then '' else @Reward()
                write: @Reward                  
            
            @vMaxAmount = ko.computed
                read: () => if @MaxAmount() is 0 then '' else @MaxAmount()
                write: @MaxAmount
                
            @vNotificationPercentThreshold = ko.computed
                read: () => if @NotificationPercentThreshold() is 0 then '' else @NotificationPercentThreshold()
                write: @NotificationPercentThreshold