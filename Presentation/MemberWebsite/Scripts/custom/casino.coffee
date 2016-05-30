class Casino
    
    # Start from page 1 to load more game
    page : 1
        
    constructor: ->
        self = this

        # Load more game      
        @loadMore = ->
            
            alert 'wait for api load more games'
            return false  
               
            # Load more game URL
            $.get 'api/url'
           
                # on done
                .done (response) =>
                    console.log 'games load'
                    self.page = self.page + 1
        
                # on fail
                .fail (jqXHR) =>
                    @onLoadFail JSON.parse jqXHR.responseText
    
        # fail message
        @onLoadFail = (response) =>
            message = ''
                
            if IsJsonString response.message
                error = JSON.parse response.message;
                message = i18n.t error.text, error.variables;
            else
                message = i18n.t response.message

# init the object page
model = new Casino
    
# apply the object page with the DOM node
ko.applyBindings model, $("#casino-wrapper")[0]