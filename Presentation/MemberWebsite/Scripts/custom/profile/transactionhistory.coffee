class @TransactionHistory extends FormBase
    constructor: (@id) ->
        super
        
        @startdate = ko.observable('').extend({ validatable: yes })
        @enddate = ko.observable('').extend({ validatable: yes })
        @hasErrors = ko.observable(false)
        
        @submitFilter = ->
            $.postJson '/api/TransactionFilter',
                game: @amount
                from: @from
                to: @to
             .success (response) =>
                if response.hasError
                    $.each response.errors, (propName)=>
                        observable = @[propName]
                        #observable.error = i18n.t 'resetPassword.' + response.errors[propName]
                        observable.error = 'test'
                        observable.__valid__ no    
                else
                    popupAlert('', '')
                