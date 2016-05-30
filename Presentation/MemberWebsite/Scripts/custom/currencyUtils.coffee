isValidCurrency = (val) ->
    unformatted = numeral().unformat(val)
    if unformatted < 0
        return no
    
    if unformatted.toString() == val.toString()
        return yes
    
    reformattedValue = numeral(unformatted).format('0,0')
    val == reformattedValue
    
doesValueContainsLetters = (val) ->
    /[a-z]/.test val.toLowerCase()

ko.bindingHandlers['currency'] =
    update: (element, valueAccessor) ->
        value = valueAccessor()
        valueUnwrapped = ko.utils.unwrapObservable(value)
        
        if valueUnwrapped == "" || valueUnwrapped == undefined
            return
        
        unformatted = numeral().unformat(valueUnwrapped)
        
        if isValidCurrency valueUnwrapped
            output = numeral(unformatted).format('0,0')
            
            value(output);
            if ($(element).is('input') == true)
                $(element).val(output)
            else
                $(element).text(output)
            
            return
        
ko.validation.rules['isValidAmount'] = {
    validator: (val, otherVal) =>
        if isValidCurrency val
            return yes
        return no
    message: 'Please enter valid amount.'
};      

ko.validation.rules['notNegativeAmount'] = {
    validator: (val, otherVal) =>
        if isValidCurrency val
            return yes
        
        valueUnwrapped = ko.utils.unwrapObservable(val)
        
        if valueUnwrapped == "" || valueUnwrapped == undefined
            return
        
        unformatted = numeral().unformat(valueUnwrapped)
        
        if isNaN(unformatted)
            return yes
        
        return unformatted > 0
        
    message: 'Amount should be greater then zero.'
};      
            
ko.validation.rules['mustNotContainLetters'] = {
    validator: (val, otherVal) =>
        if isValidCurrency val
            return yes
    
        valueUnwrapped = ko.utils.unwrapObservable(val)
        
        if doesValueContainsLetters valueUnwrapped
            return no
            
        return yes
    message: 'Amount shouldn\'t contain letters.'
};
ko.validation.registerExtenders();