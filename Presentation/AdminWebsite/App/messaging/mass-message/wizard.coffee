define [
    'komapping', 
    'nav', 
    'i18next', 
    'config',
    './recipients', 
    './content',  
    './send-dialog'
    'wizard'], (
    mapping, 
    nav, 
    i18N, 
    config,
    Recipients, 
    Content,
    SendDialog) ->
    class TemplateWizardModel
    
        constructor: ->
            @Id = ko.observable()
            @serverErrors = ko.observable()

            @steps = [
                {index: 0, name: "Recipients"}
                {index: 1, name: "Content"}]
                
            @initialStep = @steps[0]
            @allowedSteps = ko.observableArray [@steps[0]]
            @wizardSelector = ".template-wizard"
            
            wizardCount = @getCurrentWizardCount()
            
            @tabs = 
                for i in [0..@steps.length - 1]
                        tabId: "tab#{wizardCount * 10 + i}"
                        stepNumber: i + 1
                        stepName: i18N.t "messaging.massMessage.wizardSteps.#{i}"
                
            @prevBtnClass = "wizard-prev#{wizardCount}"
            @nextBtnClass = "wizard-next#{wizardCount}"
            @closeBtnText = ko.observable()
            @isNextBtnVisible = ko.observable yes
            @isPrevBtnVisible = ko.observable yes
            
        close: -> nav.close()
        
        submit: (tab, navigation, index) =>
            if not @Recipients.isValid()
                @Recipients.errors.showAllMessages()
                return no
            @serverErrors null               
            @showNext ko.utils.arrayFirst @steps, (step) -> step.index is index
            @Content.Id(@Recipients.Id())
            @Content.setLanguages(@Recipients.Languages())
            yes
            
        send: =>
            isValid = true
            $.each @Content.Languages(), (index, contentMessage) =>
                contentMessageIsValid =  contentMessage.isValid()
                if not contentMessageIsValid
                    isValid = false
                    contentMessage.errors.showAllMessages()
            if isValid
                content = []
                $.each @Content.Languages(), (index, contentMessage) =>
                    content.push
                        languageCode: contentMessage.languageCode()
                        onSite: contentMessage.onSite()
                        onSiteSubject: contentMessage.onSiteSubject()
                        onSiteContent: contentMessage.onSiteContent()
                objectToSend = 
                    id: @Content.Id()
                    content: content
                $.ajax
                    type: "POST"
                    url: config.adminApi("MassMessage/Send")
                    data: ko.toJSON(objectToSend)
                    dataType: "json"
                    contentType: "application/json"
                    traditional: true
                .done (data) =>
                    if data.isSent
                        sendDialog = new SendDialog(@close);
                        sendDialog.show();

        activate: (input) =>
            $.get config.adminApi("MassMessage/GetNewData")
                .done (response) =>
                    @Recipients = new Recipients response.licensees
                    @Content = new Content()

        bindingComplete: =>
            @element = $(@wizardSelector).last().bootstrapWizard
                tabClass: "nav nav-pills nav-justified"
                previousSelector: ".#{@prevBtnClass}"
                nextSelector: ".#{@nextBtnClass}"
                onNext: @submit
                onTabClick: (tab, navigation, currentIndex, index) =>
                    result = ko.utils.arrayFilter @allowedSteps(), (step) -> step.index is index
                    returnValue = result.length > 0
                    if currentIndex is 1 and index is 0
                        @backToRecipients()
                    returnValue
                onPrevious: @backToRecipients
                onTabShow: @toggleButtonsVisibility
            @disable step for step in @steps
            @enable step for step in @allowedSteps()
            @show @initialStep

        backToRecipients: =>
            @disable @steps[1]
            @allowedSteps.removeAll()
            @allowedSteps.push @steps[0]

        show: (step) => @element.bootstrapWizard "show", step.index
        
        showNext: (step) =>
            @allowedSteps.push step
            @enable step
            @show step
        
        toggleButtonsVisibility: (tab, navigation, index) =>
            @closeBtnText i18N.t "common.close"
            @isNextBtnVisible yes
            @isPrevBtnVisible yes
            step = @steps[index]
            if step is @steps[0]
                @isPrevBtnVisible no
            if step is @steps[1]
                @isNextBtnVisible no


        enable: (step) => @element.bootstrapWizard "enable", step.index
        
        disable: (step) => @element.bootstrapWizard "disable", step.index
        
        getCurrentWizardCount: =>
            wizardCount = 0
            wizardTabs = $ "#{@wizardSelector} a[data-toggle='tab']"
            if wizardTabs.length > 0
                hrefs = ($(tab).attr('href') for tab in wizardTabs)
                tabNumbers = (parseInt(href.slice(4)) for href in hrefs)
                maxTabNumber = Math.max tabNumbers...
                wizardCount = Math.floor(maxTabNumber / 10) + 1
            wizardCount