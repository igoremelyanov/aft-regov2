define (require) ->
    gameManagementEnabled = false
    gameManagementEnabled: gameManagementEnabled
    adminApiClientId: "local"
    adminApi: (path = "") ->
        adminApiUrl + path