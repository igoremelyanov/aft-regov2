class @ClaimBonus extends FormBase
    constructor: (@id) ->
        super
        
        @code = ko.observable('').extend
            validatable: yes
        @hasErrors = ko.observable(false)
        
        @submitBonusCode = ->
            $.postJson '/api/ClaimBonusCode',
                code : @code
                .success (response) =>
                    if response.hasError
                        $.each response.errors, (propName)=>
                            observable = @[propName]
                            #observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                            observable.error = 'test'
                            observable.__valid__ no    
                    else
                        popupAlert('', '')