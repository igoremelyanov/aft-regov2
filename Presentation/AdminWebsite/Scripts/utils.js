// A custom knockout binding for numeric input (decimal value)
ko.bindingHandlers['numeric'] = {
    init: function (element, valueAccessor) {
        bindNumericElement(element, false);
    }
};

// A custom knockout binding for numeric input (integer value)
ko.bindingHandlers['numericInt'] = {
    init: function (element, valueAccessor) {
        bindNumericElement(element, true);
    }
};

ko.bindingHandlers['negativeInt'] = {
    init: function (element) {
        bindIntegerElement(element);
    }
};

ko.bindingHandlers.datetimepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var options = allBindingsAccessor().datepickerOptions || {};
        $(element).datetimepicker(options).on("changeDate", function (evntObj) {
            var observable = valueAccessor();
            if (evntObj.date !== undefined) {
                observable(moment(evntObj.date).format("YYYY/MM/DD"));
            }
        });
    }
};

ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var options = allBindingsAccessor().datepickerOptions || {};
        if (!options.hasOwnProperty('format'))
            options.format = 'yyyy/mm/dd';
        if (!options.hasOwnProperty('autoclose'))
            options.autoclose = true;
        $(element).datepicker(options)
        //open the date picker by clicking on the calendar icon
        .prev().on(ace.click_event, function () {
            $(this).next().focus();
        });
        var observable = valueAccessor();
        var dateString = observable();
        if (dateString) {
            var date = moment(dateString, "YYYY/MM/DD").toDate();
            $(element).datepicker("setDate", date);
        }
    }
};

ko.bindingHandlers.date2 = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor();
        var allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);

        // Date formats: http://momentjs.com/docs/#/displaying/format/
        var pattern = allBindings.format || 'DD/MM/YYYY';

        var output = "-";
        if (valueUnwrapped !== null && valueUnwrapped !== undefined && valueUnwrapped.length > 0) {
            output = moment(valueUnwrapped).format(pattern);
        }

        if ($(element).is("input") === true) {
            $(element).val(output);
        } else {
            $(element).text(output);
        }
    }
};

ko.bindingHandlers.numberText = {
    update: function (element, valueAccessor, allBindingsAccessor) {
        var defaults = ko.bindingHandlers.numberText.defaults,
            aba = allBindingsAccessor,
            unwrap = ko.utils.unwrapObservable,
            value = unwrap(valueAccessor()) || valueAccessor(),
            result = '',
            numarray;

        var separator = unwrap(aba().separator) || defaults.separator,
            decimal = unwrap(aba().decimal) || defaults.decimal,
            precision = unwrap(aba().precision) || defaults.precision,
            symbol = unwrap(aba().symbol) || defaults.symbol,
            after = unwrap(aba().after) || defaults.after;

        value = parseFloat(value) || 0;

        if (precision > 0)
            value = value.toFixed(precision)

        numarray = value.toString().split('.');

        for (var i = 0; i < numarray.length; i++) {
            if (i == 0) {
                result += numarray[i].replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1' + separator);
            } else {
                result += decimal + numarray[i];
            }
        }

        result = (after) ? result += symbol : symbol + result;

        ko.bindingHandlers.text.update(element, function () { return result; });
    },
    defaults: {
        separator: ',',
        decimal: '.',
        precision: 0,
        symbol: '',
        after: false
    }
};

function bindIntegerElement(element) {
    $(element).on("keydown", function (event) {
        var val = $(event.target).val();

        // Allow: backspace, delete, tab, escape, and enter
        if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
            // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) ||
            // Allow: home, end, left, right
            (event.keyCode >= 35 && event.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        else if (event.keyCode == 45 || event.keyCode == 189) { // minus
            return;
        }
        else {
            // Ensure that it is a number and stop the keypress
            if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                event.preventDefault();
            }
        }
    });
}

function bindNumericElement(element, isInteger) {

    $(element).on("keydown", function (event) {
        // Allow: backspace, delete, tab, escape, and enter
        if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
            // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) ||
            // Allow: . 
            (!isInteger && (event.keyCode == 190 || event.keyCode == 110) && this.value.indexOf(".") == -1) ||
            // Allow: home, end, left, right
            (event.keyCode >= 35 && event.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        else {
            // Ensure that it is a number and stop the keypress
            if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                event.preventDefault();
            }
        }
    });
}
function setError(ob, error) {
    ob.error = error;
    ob.__valid__(false);
}
//This function is to solve issue with posting json to server-side with array of json objects 
function postJson(value) {
    var result = {};

    var buildResult = function (object, prefix) {
        for (var key in object) {

            var postKey = isFinite(key)
                ? (prefix != "" ? prefix : "") + "[" + key + "]"
                : (prefix != "" ? prefix + "." : "") + key;

            switch (typeof (object[key])) {
                case "number": case "string": case "boolean":
                    result[postKey] = object[key];
                    break;

                case "object":
                    buildResult(object[key], postKey != "" ? postKey : key);
            }
        }
    };

    buildResult(value, "");

    return result;
};

ko.extenders.formatDecimal = function (target, precision) {
    //create a writeable computed observable to intercept writes to our observable
    var result = ko.computed({
        read: target, //always return the original observables value
        write: function (newValue) {
            if (newValue == undefined) {
                return;
            }
            newValue = newValue.toString().trim();
            if (newValue.length === 0) {
                target(newValue);
                target.notifySubscribers(newValue);
            } else {
                var val = parseFloat(+newValue);
                if (val < 0.01)
                    val = 0;
                val = val.toString();
                var valueToWrite = val.indexOf(".") === -1 ? val : val.substr(0, val.indexOf(".") + precision + 1);
                target(valueToWrite);
                target.notifySubscribers(valueToWrite);
            }
        }
        //Using { notify: 'always' } causes the textbox to refresh (erasing rejected values) even if the computed property has not changed value.
    }).extend({ notify: "always" });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};

ko.extenders.formatNegativeDecimal = function (target, precision) {
    //create a writable computed observable to intercept writes to our observable
    var result = ko.computed({
        read: target,  //always return the original observables value
        write: function (newValue) {
            if (newValue == '') {
                target(newValue);
                return;
            }

            var current = target(),
                roundingMultiplier = Math.pow(10, precision),
                newValueAsNum = isNaN(newValue) ? 0 : parseFloat(+newValue),
                valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

            //only write if it changed
            if (valueToWrite !== current) {
                target(valueToWrite);
            } else {
                //if the rounded value is the same, but a different value was written, force a notification for the current field
                if (newValue !== current) {
                    target.notifySubscribers(valueToWrite);
                }
            }
        }
    }).extend({ notify: 'always' });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};

ko.extenders.formatInt = function (target, settings) {
    //create a writeable computed observable to intercept writes to our observable
    var result = ko.computed({
        read: target,
        write: function (newValue) {
            if (!settings.allowEmpty && (newValue === undefined || newValue === null || newValue.length === 0))
                newValue = 0;

            newValue = newValue.toString().trim();
            if (newValue.length === 0) {
                target(newValue);
                target.notifySubscribers(newValue);
            } else {
                var val = parseInt(+newValue);

                if (val < 0.01 && !settings.allowNegative)
                    val = 0;

                if (isNaN(val))
                    val = 0;

                val = val.toString();
                target(val);
                target.notifySubscribers(val);
            }
        }
        //Using { notify: 'always' } causes the textbox to refresh (erasing rejected values) even if the computed property has not changed value.
    }).extend({ notify: "always" });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};