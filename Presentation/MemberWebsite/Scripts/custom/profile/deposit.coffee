class Deposit extends FormBase
    constructor: ->
        @amount = ko.observable()
        @bonuses = ko.observableArray()
        @amount.subscribe (value) =>
            if value
                @checkForBonuses value
        @checkForBonuses = (value) =>
            data = {amount: value}
            $.post '/api/qualifiedbonuses', data
                    .done (response) => 
                        @bonuses []
                        @bonuses response
                        console.log response
         @selectBonus = (data, event) =>
            if !$(event.currentTarget).hasClass('disable') 
                if ($(event.currentTarget).hasClass('selected')) 
                    $(event.currentTarget).removeClass('selected')
                else
                    $('#bonusList .col-sm-3').removeClass('selected')
                    $(event.currentTarget).addClass('selected')
    model = new Deposit()
    ko.applyBindings model, $("#profile-wrapper")[0]