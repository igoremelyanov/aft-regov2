class @PendingDeposit extends FormBase
    constructor: (@id) ->
        super
        
        @fullName = ko.observable('test').extend
            validatable: yes
        @idFront = ko.observable('').extend
            validatable: yes
        @idBack = ko.observable('').extend
            validatable: yes
        @transferMethod = ko.observable('').extend
            validatable: yes
        @transferBank = ko.observable('').extend
            validatable: yes
        @depositReceipt = ko.observable('').extend
            validatable: yes
        @depositAmount = ko.observable('').extend
            validatable: yes
        @remark = ko.observable('').extend
            validatable: yes
        
        @submitDepositConfirmation = ->
            alert "confirm deposit"