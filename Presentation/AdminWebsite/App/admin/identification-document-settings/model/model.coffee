define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    config = require "config"

    class IdentificationSettingModel
        constructor: () ->
            @remarks = ko.observable('')
            .extend
                maxLength: 200
                
            @licensees = ko.observableArray()
            @brands = ko.observableArray()
            
            @licensee = ko.observable()
                .extend
                    required: true
                    
            @licensee.subscribe (val) =>
                @loadBrands val.id
            
            @brand = ko.observable()
                .extend
                    required: true
                    
            @brand.subscribe (val) =>
                @loadPaymentMethods val.id
            
            @transactionType = ko.observable()
                .extend
                        required: true
                        
            @transactionTypes = ko.observableArray()
            
            @paymentMethod = ko.observable()
                .extend
                    required: true
            @paymentMethods = ko.observableArray()
            @paymentMethodTitle = ko.computed =>
                if @paymentMethod() && @paymentMethods()
                    method = _.find @paymentMethods(), (m)=>
                        m.id == @paymentMethod()
                    if method
                        method.name
                    else "-"
                else
                    '-'
            @idFront = ko.observable()
            @idBack = ko.observable()
            @creditCardFront = ko.observable()
            @creditCardBack = ko.observable()
            @poa = ko.observable()
            @dcf = ko.observable()
            
            @setting = ko.observable()
            
        load: (id)->
            @id = ko.observable(id)
            url = config.adminApi("IdentificationDocumentSettings/GetEditData")
            
            if @id()
                url = url + "?id=" + @id()
                
            $.get url
                .done (data) =>
                    @licensees data.licensees
                    @transactionTypes data.transactionTypes
                
                    if (data.licensees.length == 0)
                        return
                    
                    if @id() && data.setting
                        @setting data.setting
                        @idFront data.setting.idFront
                        @idBack data.setting.idBack
                        @creditCardFront data.setting.creditCardFront
                        @creditCardBack data.setting.creditCardBack
                        @poa data.setting.poa
                        @dcf data.setting.dcf
                        @remarks data.setting.remark
                        @paymentMethod data.setting.paymentGatewayBankAccountId
                        @transactionType data.setting.transactionType
                        
                        @licensee _.find @licensees(), (l)->
                            l.id == data.setting.licenseeId
                           
                        g = 0
                    else
                        @licensee @licensees()[0]
        
        loadBrands: (licenseeId) ->
            if (licenseeId?)
                $.get config.adminApi("IdentificationDocumentSettings/GetLicenseeBrands?licenseeId=" + licenseeId)
                    .done (data) =>
                        @brands data.brands
                    
                        if (data.brands.length == 0)
                            return
                        
                        if @id()
                            @brand _.find @brands(), (b)=>
                                b.id == @setting().brandId
                        else
                            @brand @brands()[0]
                            
        loadPaymentMethods: (brandId) ->
            if (brandId?)
                $.get config.adminApi("IdentificationDocumentSettings/GetPaymentMethods?brandId=" + brandId)
                    .done (data) =>
                        @paymentMethods data.paymentMethods
                    
                        if (data.paymentMethods.length == 0)
                            return
                        
                        if @id()
                            @paymentMethod _.find @paymentMethods(), (b)=>
                                b.id == @paymentMethods().id
                        else
                            @paymentMethod @paymentMethods()[0]
                    
        clear: () ->
            @remarks ''
            @remarks.isModified no
            @id ''
            #@licensees ''
            #@brands ''
            @licensee ''
            @brand ''
            @transactionType ''
            @paymentMethod ''
            @idFront ''
            @idBack ''
            @creditCardFront no
            @creditCardBack no
            @poa no
            @dcf no
                    
        getModelToSave: () ->
            obj = {
                LicenseeId: @licensee().id,
                BrandId: @brand().id,
                TransactionType: @transactionType(),
                PaymentGatewayBankAccountId: @paymentMethod(),
                IdFront: @idFront(),
                IdBack: @idBack(),
                CreditCardFront: @creditCardFront(),
                CreditCardBack: @creditCardBack(),
                POA: @poa(),
                DCF: @dcf(),
                Remark: @remarks()
            }
            
            if @id()
                obj.Id = @id()
            
            return obj
            
    model = new IdentificationSettingModel()
    model.load()
    model.errors = ko.validation.group model
    model