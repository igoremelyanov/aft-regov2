class @GameHistory extends FormBase
    constructor: (@id) ->
        super
        
        @game = ko.observable('').extend
            validatable: yes
        @from = ko.observable('')
        @to = ko.observable('')
        @hasErrors = ko.observable(false)
        
        @submitFilter = ->
            $.postJson '/api/GameFilter',
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
                