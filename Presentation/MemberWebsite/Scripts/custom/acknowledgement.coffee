class Acknowledgement
    constructor: ->
        @self = this
        @getText = ko.observable('u<br/>hu')
            
    getMainText: (date)=>
        i18n.t 'acknowledgment.main', { date: date }
            

model = new Acknowledgement()
        
ko.applyBindings model, document.getElementById "acknowledgement-wrapper"