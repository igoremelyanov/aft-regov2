define (require) ->
    ContentMessage = require "./content-message"
    i18n = require "i18next"
    class Content
        constructor: ->
            @Id = ko.observable()
            @Languages = ko.observableArray()
            
        setLanguages: (languages) =>
            @Languages.removeAll()
            for language in languages
                contentMessage = new ContentMessage @Id(), language.code(), language.name()
                @Languages.push contentMessage
                contentMessage.loadEditors()
        