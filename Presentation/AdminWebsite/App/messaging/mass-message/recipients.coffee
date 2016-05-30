define [
    'i18next',
    'config',
    'datePicker'], (
    i18n,
    config) ->
    class Recipients
        constructor: (availableLicensees) ->
            @config = config
            @Id = ko.observable()
            @searchGridSelector = "#mass-message-search-grid"
            @recipientsGridSelector = "#mass-message-recipients-grid"
            @allowChangeBrand = ko.observable true
            @availableLicensees = ko.observableArray(availableLicensees)
            @selectedLicensee = ko.observable()

            @availableBrands = ko.computed =>
                if @selectedLicensee() then @selectedLicensee().brands else []

            @selectedBrand = ko.observable()

            @availablePaymentLevels = ko.computed =>
                if @selectedBrand() then @selectedBrand().paymentLevels else []

            @availableVipLevels = ko.computed =>
                if @selectedBrand() then @selectedBrand().vipLevels else []

            @availableStatuses = ko.observableArray([
                name: i18n.t "common.active"
                value: 'Active'
            ,
                name: i18n.t "common.inactive"
                value: 'Inactive'
            ])
            
            @registrationDateFrom = ko.observable()
            
            @registrationDateFrom.subscribe (value) =>
                reload = @validateRegistrationDateChange true
                if reload then @reload(@searchGridSelector)
            
            @registrationDateTo = ko.observable()
            
            @registrationDateTo.subscribe (value) =>
                reload = @validateRegistrationDateChange false
                if reload then @reload(@searchGridSelector)

            @BrandId = ko.computed =>
                if @selectedBrand() then @selectedBrand().id else null

            @BrandId.subscribe =>
                @reload(@searchGridSelector)

            @SearchTerm = ko.observable ""
            
            @SearchTerm.extend rateLimit:
                timeout: 1000, 
                method: "notifyWhenChangesStop"
                
            @SearchTerm.subscribe (value) =>
                @reload(@searchGridSelector)

            @PaymentLevelId = ko.observable()
            
            @PaymentLevelId.subscribe =>
                @reload(@searchGridSelector)

            @VipLevelId = ko.observable()
            
            @VipLevelId.subscribe =>
                @reload(@searchGridSelector)

            @Status = ko.observable()
            
            @Status.subscribe =>
                @reload(@searchGridSelector)

            @RegistrationDateFrom = ko.observable ""
            
            @RegistrationDateTo = ko.observable ""
            
            @allowChangeBrand = ko.observable yes

            @all = ko.observable i18n.t("common.all")
            
            @HasRecipients = ko.observable(false).extend
                validation:
                    validator: (val) =>
                        val is true
                    message: i18n.t "messaging.massMessage.recipientRequired"
                    
            @Languages = ko.observableArray()
            
            @compositionComplete = =>
                $ =>
                    searchGridLoading = false
                    recipientsGridLoading = false
                    $(@searchGridSelector).on "selectionChange", (e, row) =>
                        if searchGridLoading 
                            return
                        updateType = if row.isSelected then "SelectSingle" else "UnselectSingle"
                        playerId = row.id
                        @updateRecipients updateType, playerId, [@recipientsGridSelector]
                    $(@searchGridSelector).on "selectAllChange", (e, data) =>
                        updateType = if data.allSelected then "SearchResultSelectAll" else "SearchResultUnselectAll"
                        @updateRecipients updateType, null, [@recipientsGridSelector]
                    $(@recipientsGridSelector).on "selectionChange", (e, row) =>
                        if !recipientsGridLoading
                            @updateRecipients "UnselectSingle", row.id, [@searchGridSelector, @recipientsGridSelector]
                    $(@recipientsGridSelector).on "selectAllChange", (e, data) =>
                        if !recipientsGridLoading
                            @updateRecipients "RecipientsListUnselectAll", null, [@searchGridSelector, @recipientsGridSelector]
                    $(@searchGridSelector).on "gridLoad", (e, data) =>
                        searchGridLoading = true
                        grid = $(@searchGridSelector)
                        table = grid.find '.ui-jqgrid-btable'
                        tableData = data.tableData
                        for rowData in tableData.rows
                            if rowData.cell.Selected
                                table.jqGrid 'setSelection', rowData.id, true
                        searchGridLoading = false

                    $(@recipientsGridSelector).on "gridLoad", (e, data) =>
                        recipientsGridLoading = true
                        grid = $(@recipientsGridSelector)
                        checkbox = grid.find 'th input[type="checkbox"]'
                        checkbox.click()
                        recipientsGridLoading = false
                
                ko.validation.group @

        validateRegistrationDateChange: (fromDateChanged) =>
            reload = true
            fromField = $ "input[data-bind*='datepicker: registrationDateFrom']"
            fromDate = fromField.datepicker 'getDate'
            toField = $ "input[data-bind*='datepicker: registrationDateTo']"
            toDate = toField.datepicker 'getDate'
            if fromDateChanged and fromDate
                @RegistrationDateFrom @getDateAsUtc(fromDate)
            if !fromDateChanged and toDate
                @RegistrationDateTo @getDateAsUtc(toDate)
            if toDate and fromDate
                if fromDate > toDate
                    reload = false
                    if fromDateChanged
                        toField.datepicker 'setDate', fromDate
                    else
                        fromField.datepicker 'setDate', toDate
            reload

        reload: (gridSelector) =>
            $(gridSelector).trigger "reload"

        getDateAsUtc: (date) =>
            date.getFullYear() + "-" + 
            @pad(date.getMonth() + 1) + "-" + 
            @pad(date.getDate()) + 
            'T00:00:00-00:00Z';
        
        pad: (number) ->
            if number < 10 then "0" + number else number
            
        updateRecipients: (updateType, playerId, gridsToUpdate) =>
            @allowChangeBrand false
            data = 
                Id: @Id()
                UpdateRecipientsType: updateType
                PlayerId: playerId
                SearchPlayersRequest:
                    BrandId: @BrandId()
                    SearchTerm: @SearchTerm()
                    PaymentLevelId: @PaymentLevelId()
                    VipLevelId: @VipLevelId()
                    PlayerStatus: @Status()
                    RegistrationDateFrom: @RegistrationDateFrom()
                    RegistrationDateTo: @RegistrationDateTo()
            $.ajax
                type: "POST"
                url: config.adminApi "MassMessage/UpdateRecipients"
                data: ko.toJSON(data)
                dataType: "json"
                contentType: "application/json"
            .done (response) =>
                @Id response.id
                @HasRecipients response.hasRecipients
                ko.mapping.fromJS response.languages, {}, @Languages
                #@Languages response.languages
                $.each gridsToUpdate, (index, value) =>
                    @reload value