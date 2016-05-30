﻿define (require) ->
    require "../Scripts/ace-editor/ace"
    i18n = require "i18next"

    class AceEditor
        constructor: (editorId, editorObservable, isEditable = true, isSingleLine = false)->
            editor = ace.edit editorId
            editor.setTheme "ace/theme/chrome"
            editor.getSession().setUseWorker false
            editor.setShowPrintMargin false;
            editor.setFontSize 14
            editor.setOptions
                highlightActiveLine: false
            defaultValue = editorObservable()
            if defaultValue
                editor.setValue defaultValue, -1
            if isEditable
                editor.getSession().on "change", =>
                    editorObservable editor.getValue()
                editor.on "paste", (e)=>
                    pasteLineCount = e.text.split(/\r\n|\r|\n/).length;
                    maxPasteLineCount = 10000;
                    if (pasteLineCount > maxPasteLineCount)
                        e.text = ""
                        alert i18n.t "app:common.pasteExceedsLineLimit"
            else
                editor.setReadOnly true
            if isSingleLine
                editor.setOptions
                    maxLines: 1
                editor.on "paste", (e) ->
                    e.text = e.text.replace(/[\r\n]+/g, " ")
                editor.commands.bindKey "Enter|Shift-Enter", "null"
            else
                editor.getSession().setMode "ace/mode/html"