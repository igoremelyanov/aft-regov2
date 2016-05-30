    ko.validation.init({
        registerExtenders: true
    }, true);

    class @OnlineDepositModel
        constructor: (successReturnUrl, failReturnUrl, failCallback)->         
            @successReturnUrl = successReturnUrl
            @failReturnUrl = failReturnUrl
            @orderId = ko.observable()
            @onlineAmount = ko.observable("")
                               
            @onlineDepositRequestInProgress = ko.observable no
            @checkOnlineDepositInProgress = ko.observable no
            @onlineDepositSuccess = ko.observable location.hash is "#onlineDeposit/success"

            @onlineDepositErrors = ko.observableArray []                           
                                                                                                            
            @playerDepositedAmount = ko.observable 0
            @bonusAmount = ko.observable 0
            @totalDepositedAmount = ko.observable 0
            @onlineDepositBonusCode = ko.observable()
            @onlineDepositBonusId = ko.observable()
        submitOnlineDeposit: () =>
            @onlineDepositSuccess off
            @onlineDepositErrors []
            @onlineDepositRequestInProgress yes
          
            $.postJson '/api/onlineDeposit',                
                Amount: @onlineAmount()     
                BonusCode: @onlineDepositBonusCode()
                BonusId: @onlineDepositBonusId()           
            .done (response) =>                       
                @orderId(response.depositRequestResult.redirectParams.orderId);
                
                popWidth = screen.width / 1.5;
                popHeight = screen.height / 1.5;
                x = screen.width / 2 - popWidth / 2;
                y = screen.height / 2 - popHeight / 2;   
                popupTitle = 'OnlineDeposit';
                targetUrl = getLozalizedPath '/Home/OnlineDepositForm'
                @popupPayWin = window.open(targetUrl, 
                popupTitle, 'height=' + popHeight + ',width=' + popWidth + ',left=' + x + ',top=' + y + ',location=0');                   
                counter = 0;
                pageLoadedTimer = setInterval(
                    () =>
                        counter++;
                        if @popupPayWin.isPopwinReady 
                            clearInterval(pageLoadedTimer);                            
                            popupWin = @popupPayWin.document;
                            $('#payRedirectForm', popupWin).attr("action", response.depositRequestResult.redirectUrl);
                            $('#method', popupWin).val(response.depositRequestResult.redirectParams.method);
                            $('#channel', popupWin).val(response.depositRequestResult.redirectParams.channel);
                            $('#merchantId', popupWin).val(response.depositRequestResult.redirectParams.merchantId);
                            $('#orderId', popupWin).val(response.depositRequestResult.redirectParams.orderId);
                            $('#amount', popupWin).val(response.depositRequestResult.redirectParams.amount);
                            $('#currency', popupWin).val(response.depositRequestResult.redirectParams.currency);
                            $('#language', popupWin).val(response.depositRequestResult.redirectParams.language);
                            $('#returnUrl', popupWin).val(response.depositRequestResult.redirectParams.returnUrl);
                            $('#notifyUrl', popupWin).val(response.depositRequestResult.redirectParams.notifyUrl);
                            $('#signature', popupWin).val(response.depositRequestResult.redirectParams.signature);
                            $('#payRedirectForm', popupWin).submit();       
                            @startCheckStatus()                
                        else
                            if (counter >= 20)
                                clearInterval(pageLoadedTimer);                                                
                , 300);                        
            .fail (jqXHR) =>
                @fail JSON.parse jqXHR.responseText
                @onlineDepositRequestInProgress no
            .always =>
            
        startCheckStatus: () =>                             
             pageClosedtimer = setInterval(
                () => 
                    if (@popupPayWin.closed) 
                        clearInterval(pageClosedtimer);    
                        @checkStatus()
                , 1000);   
                
        fail: (response) ->
            message = ''
                
            if IsJsonString response.message
                error = JSON.parse(response.message);
                message = i18n.t(error.text, error.variables);
            else
                message = i18n.t(response.message)
                
            if response.unexpected || response.message
                    @onlineDepositErrors.push i18n.t("app:payment.deposit.depositFailed") + message
            else
                @onlineDepositErrors.push error for error in response.errors
                if response.errors.length is 0 and response.message
                    @onlineDepositErrors.push i18n.t("app:payment.deposit.depositFailed") + message
        checkStatus:() =>
            if(@checkOnlineDepositInProgress() == no)
                @checkOnlineDepositInProgress yes
                $.postJson '/api/checkOnlineDepositStatus',                
                    TransactionNumber: @orderId()
                .done (response) =>
                    if(response.depositStatus.isPaid)                        
                        @onlineDepositSuccess yes
                        @onlineDepositRequestInProgress no                            
                        @playerDepositedAmount response.depositStatus.amount
                        @bonusAmount response.depositStatus.bonus;
                        @totalDepositedAmount response.depositStatus.totalAmount
                        if @successReturnUrl
                            redirect @successReturnUrl
                        else
                            location.hash="#onlineDepositResult"
                    else
                        @onlineDepositSuccess no
                        @onlineDepositRequestInProgress no                                                                                         
                        if @successReturnUrl
                            redirect @failReturnUrl
                        else
                            location.hash="#onlineDepositResult"               
                .fail (jqXHR) =>
                    @fail JSON.parse jqXHR.responseText     
                .always =>
                    @checkOnlineDepositInProgress no
