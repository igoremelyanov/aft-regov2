define (require) ->
    i18n = require "i18next"
    aceEditor = require "ace-editor"
    class ContentMessage
        constructor: (id, languageCode, languageName)->
            @languageCode = ko.observable languageCode
            @languageName = ko.observable languageName
            
            @onSite = ko.observable false
            
            @onSiteSubject = ko.observable().extend
                required:
                    params: true
                    onlyIf: @onSite

            @onSiteContent = ko.observable().extend
                required:
                    params: true
                    onlyIf: @onSite
                    
            @hasMessage = ko.observable().extend
                validation:
                    params: true
                    validator: (val) =>
                        @onSite() is true
                    message: i18n.t "messaging.massMessage.validation.NoMessageTypeSelected"
            
            editorPrefix = id + "-" + languageCode + "-"
            @onSiteSubjectId = ko.observable editorPrefix + "on-site-subject-editor"
            @onSiteContentId = ko.observable editorPrefix + "on-site-content-editor"
            
            ko.validation.group @
            
        loadEditors: =>
            new aceEditor @onSiteSubjectId(), @onSiteSubject, true, true
            new aceEditor @onSiteContentId(), @onSiteContent, true, false
