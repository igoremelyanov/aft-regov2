define (require) ->
    require "controls/grid"
    nav = require "nav"
    confirmation = require "player-manager/info/confirm-dialog/confirm-dialog"
    config = require "config"
    efu = require "EntityFormUtil"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            @config = require "config"
            @playerId = ko.observable()
            @selectedRowId = ko.observable()
            @metadata = ko.observable([])
            
            @form = new efu.Form @
            
            @form.makeField "deviceIdExactScore", ko.observable()
            @form.makeField "firstNameExactScore", ko.observable()
            @form.makeField "lastNameExactScore", ko.observable()
            @form.makeField "fullNameExactScore", ko.observable()
            @form.makeField "usernameExactScore", ko.observable()
            @form.makeField "addressExactScore", ko.observable()
            @form.makeField "signUpIpExactScore", ko.observable()
            @form.makeField "mobilePhoneExactScore", ko.observable()
            @form.makeField "dateOfBirthExactScore", ko.observable()
            @form.makeField "emailAddressExactScore", ko.observable()
            @form.makeField "zipCodeExactScore", ko.observable()
            
            @form.makeField "deviceIdFuzzyScore", ko.observable()
            @form.makeField "firstNameFuzzyScore", ko.observable()
            @form.makeField "lastNameFuzzyScore", ko.observable()
            @form.makeField "fullNameFuzzyScore", ko.observable()
            @form.makeField "usernameFuzzyScore", ko.observable()
            @form.makeField "addressFuzzyScore", ko.observable()
            @form.makeField "signUpIpFuzzyScore", ko.observable()
            @form.makeField "mobilePhoneFuzzyScore", ko.observable()
            @form.makeField "dateOfBirthFuzzyScore", ko.observable()
            @form.makeField "emailAddressFuzzyScore", ko.observable()
            @form.makeField "zipCodeFuzzyScore", ko.observable()
            
            fieldsList = ["deviceIdExactScore",
            "firstNameExactScore",
            "lastNameExactScore",
            "fullNameExactScore",
            "usernameExactScore",
            "addressExactScore",
            "signUpIpExactScore",
            "mobilePhoneExactScore",
            "dateOfBirthExactScore",
            "emailAddressExactScore",
            "zipCodeExactScore",
            
            "deviceIdFuzzyScore",
            "firstNameFuzzyScore",
            "lastNameFuzzyScore",
            "fullNameFuzzyScore",
            "usernameFuzzyScore",
            "addressFuzzyScore",
            "signUpIpFuzzyScore",
            "mobilePhoneFuzzyScore",
            "dateOfBirthFuzzyScore",
            "emailAddressFuzzyScore",
            "zipCodeFuzzyScore"]
            
            efu.publishIds @, "duplicate-search-", fieldsList
            efu.addCommonMembers @
            @form.publishIsReadOnly fieldsList
            
            @getFormData = () =>
                ko.toJSON({
                    deviceIdExactScore: parseInt if @fields.deviceIdExactScore() then @fields.deviceIdExactScore() else 0
                    firstNameExactScore: parseInt if @fields.firstNameExactScore() then @fields.firstNameExactScore() else 0
                    lastNameExactScore: parseInt if @fields.lastNameExactScore() then @fields.lastNameExactScore() else 0
                    usernameExactScore: parseInt if @fields.usernameExactScore() then @fields.usernameExactScore() else 0
                    addressExactScore: parseInt if @fields.addressExactScore() then @fields.addressExactScore() else 0
                    signUpIpExactScore: parseInt if @fields.signUpIpExactScore() then @fields.signUpIpExactScore() else 0
                    mobilePhoneExactScore: parseInt if @fields.mobilePhoneExactScore() then @fields.mobilePhoneExactScore() else 0
                    dateOfBirthExactScore: parseInt if @fields.dateOfBirthExactScore() then @fields.dateOfBirthExactScore() else 0
                    emailAddressExactScore: parseInt if @fields.emailAddressExactScore() then @fields.emailAddressExactScore() else 0
                    zipCodeExactScore: parseInt if @fields.zipCodeExactScore() then @fields.zipCodeExactScore() else 0
                })
            
            @totalExactScore = ko.computed ()=>
                score = 0
                
                deviceIdExactScore = parseInt @fields.deviceIdExactScore()
                if not isNaN deviceIdExactScore
                    score += deviceIdExactScore
                    
                firstNameExactScore = parseInt @fields.firstNameExactScore()
                if not isNaN firstNameExactScore
                    score += firstNameExactScore
                    
                lastNameExactScore = parseInt @fields.lastNameExactScore()
                if not isNaN lastNameExactScore
                    score += lastNameExactScore
                    
                fullNameExactScore = parseInt @fields.fullNameExactScore()
                if not isNaN fullNameExactScore
                    score += fullNameExactScore
                    
                usernameExactScore = parseInt @fields.usernameExactScore()
                if not isNaN usernameExactScore
                    score += usernameExactScore
                    
                addressExactScore = parseInt @fields.addressExactScore()
                if not isNaN addressExactScore
                    score += addressExactScore
                    
                signUpIpExactScore = parseInt @fields.signUpIpExactScore()
                if not isNaN signUpIpExactScore
                    score += signUpIpExactScore
                    
                mobilePhoneExactScore = parseInt @fields.mobilePhoneExactScore()
                if not isNaN mobilePhoneExactScore
                    score += mobilePhoneExactScore
                    
                dateOfBirthExactScore = parseInt @fields.dateOfBirthExactScore()
                if not isNaN dateOfBirthExactScore
                    score += dateOfBirthExactScore
                    
                emailAddressExactScore = parseInt @fields.emailAddressExactScore()
                if not isNaN emailAddressExactScore
                    score += emailAddressExactScore
                    
                zipCodeExactScore = parseInt @fields.zipCodeExactScore()
                if not isNaN zipCodeExactScore
                    score += zipCodeExactScore
                    
                score
                
            @totalFuzzyScore = ko.computed ()=>
                score = 0
                
                deviceIdFuzzyScore = parseInt @fields.deviceIdFuzzyScore()
                if not isNaN deviceIdFuzzyScore
                    score += deviceIdFuzzyScore
                    
                firstNameFuzzyScore = parseInt @fields.firstNameFuzzyScore()
                if not isNaN firstNameFuzzyScore
                    score += firstNameFuzzyScore
                    
                lastNameFuzzyScore = parseInt @fields.lastNameFuzzyScore()
                if not isNaN lastNameFuzzyScore
                    score += lastNameFuzzyScore
                    
                fullNameFuzzyScore = parseInt @fields.fullNameFuzzyScore()
                if not isNaN fullNameFuzzyScore
                    score += fullNameFuzzyScore
                    
                usernameFuzzyScore = parseInt @fields.usernameFuzzyScore()
                if not isNaN usernameFuzzyScore
                    score += usernameFuzzyScore
                    
                addressFuzzyScore = parseInt @fields.addressFuzzyScore()
                if not isNaN addressFuzzyScore
                    score += addressFuzzyScore
                    
                signUpIpFuzzyScore = parseInt @fields.signUpIpFuzzyScore()
                if not isNaN signUpIpFuzzyScore
                    score += signUpIpFuzzyScore
                    
                mobilePhoneFuzzyScore = parseInt @fields.mobilePhoneFuzzyScore()
                if not isNaN mobilePhoneFuzzyScore
                    score += mobilePhoneFuzzyScore
                    
                dateOfBirthFuzzyScore = parseInt @fields.dateOfBirthFuzzyScore()
                if not isNaN dateOfBirthFuzzyScore
                    score += dateOfBirthFuzzyScore
                    
                emailAddressFuzzyScore = parseInt @fields.emailAddressFuzzyScore()
                if not isNaN emailAddressFuzzyScore
                    score += emailAddressFuzzyScore
                    
                zipCodeFuzzyScore = parseInt @fields.zipCodeFuzzyScore()
                if not isNaN zipCodeFuzzyScore
                    score += zipCodeFuzzyScore
                    
                score

        activate: (data) ->
            @playerId data.playerId
            
            $.get "DuplicateMechanism/GetConfiguration", playerId:  @playerId()
                .done (response) =>
                    if response.configuration
                        @fields.deviceIdExactScore response.configuration.deviceIdExactScore
                        @fields.firstNameExactScore response.configuration.firstNameExactScore
                        @fields.lastNameExactScore response.configuration.lastNameExactScore
                        @fields.fullNameExactScore response.configuration.fullNameExactScore
                        @fields.usernameExactScore response.configuration.usernameExactScore
                        @fields.addressExactScore response.configuration.addressExactScore
                        @fields.signUpIpExactScore response.configuration.signUpIpExactScore
                        @fields.mobilePhoneExactScore response.configuration.mobilePhoneExactScore
                        @fields.dateOfBirthExactScore response.configuration.dateOfBirthExactScore
                        @fields.emailAddressExactScore response.configuration.emailAddressExactScore
                        @fields.zipCodeExactScore response.configuration.zipCodeExactScore

                        @fields.deviceIdFuzzyScore response.configuration.deviceIdFuzzyScore
                        @fields.firstNameFuzzyScore response.configuration.firstNameFuzzyScore
                        @fields.lastNameFuzzyScore response.configuration.lastNameFuzzyScore
                        @fields.fullNameFuzzyScore response.configuration.fullNameFuzzyScore
                        @fields.usernameFuzzyScore response.configuration.usernameFuzzyScore
                        @fields.addressFuzzyScore response.configuration.addressFuzzyScore
                        @fields.signUpIpFuzzyScore response.configuration.signUpIpFuzzyScore
                        @fields.mobilePhoneFuzzyScore response.configuration.mobilePhoneFuzzyScore
                        @fields.dateOfBirthFuzzyScore response.configuration.dateOfBirthFuzzyScore
                        @fields.emailAddressFuzzyScore response.configuration.emailAddressFuzzyScore
                        @fields.zipCodeFuzzyScore response.configuration.zipCodeFuzzyScore
            
        attached: (view) ->
            self = this
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $(view).on "click", ".jqgrow", ->
                self.selectedRowId $(@).attr "id"
                table = $grid.find('.ui-jqgrid-btable')
                data = table.jqGrid('getRowData', self.selectedRowId())

        setMetadata: (data) =>
            arr = @metadata()
            arr.push { id: data[0], data: JSON.parse(data[1]) } # 1 index of metadata field
            @metadata arr
            
        colorClass: (fieldName, value, id) =>
            c = _.findLast @metadata(), (item) =>
                item.id == id
            
            if _.include c.data, fieldName
                '<span class="red">' + value + '</span>'
            else
                value
