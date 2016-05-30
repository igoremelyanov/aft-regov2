class RegisterStep3
    constructor: ->
        @bonuscode = ko.observable().extend({ validator: true })
        @selectedBonusId = ko.observable()
        @depositRequestInProgress = ko.observable no
        
    submitBonus: () =>
        console.log "submit bonus"
    validateBonusCode: () =>
        @bonuscode.setError false
        @validate "/api/ValidateFirstDepositBonus", {depositAmount: $("#deposit-amount").text(), bonusCode: @bonuscode()}, (response) =>
            redirect '/home/registerstep3?bonusCode=' + response.bonus.code
    selectBonus: (data, event) =>
        bonusId = $(event.currentTarget).find('#bonus-id').text()
        if !$(event.currentTarget).hasClass('disable') 
            if ($(event.currentTarget).hasClass('selected')) 
                $(event.currentTarget).removeClass('selected')
                @selectedBonusId ""
            else
                $('#bonusList .col-sm-3').removeClass('selected')
                $(event.currentTarget).addClass('selected')
                @selectedBonusId bonusId
    submitRequest: ()=>
        if @depositRequestInProgress()
            return
        onlineDeposit = @createOnlineDepositObject @selectedBonusId()
        onlineDeposit.submitOnlineDeposit()
        @depositRequestInProgress yes
    skipBonus: ()=>
        if @depositRequestInProgress()
            return
        onlineDeposit = @createOnlineDepositObject ''
        onlineDeposit.submitOnlineDeposit()
        @depositRequestInProgress yes
    createOnlineDepositObject: (bonusId) =>
        onlineDeposit = new OnlineDepositModel('/home/registerstep4','/home/registerstep2')
        onlineDeposit.onlineAmount $("#deposit-amount").text()
        onlineDeposit.onlineDepositBonusId bonusId
    validate: (url, data, onSuccess) =>
        $.postJson url, data
            .done (response) => 
                onSuccess(response)
            .fail (failResponse) => 
                @onErrorHandler failResponse
        
    onErrorHandler: (failResponse) =>
        response = JSON.parse failResponse.responseText
        for error in response.errors
            fieldname = error.fieldName
            field = @[fieldname.toLowerCase()]
            if(field)
                field.setError true


    model = new RegisterStep3()
    ko.applyBindings model, document.getElementById("register-wrapper")
    $('#bonusList .col-sm-3').first().click()

usaFormatNumber = (n) ->
    n = n.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,")
    return n.replace '.00', ''
    
cleanNumber = (n) ->
    n = n.replace(',', '').replace('.00', '')
    Math.floor n
    
ko.extenders.validator = (target) ->
    target.hasError = ko.observable false
    target.messages = ko.observableArray []

    target.setError = (val) =>
        target.hasError(val);
        if not val 
            target.messages [] ;
    target.subscribe (newVal, oldVal)=>
        target.setError no
    target

