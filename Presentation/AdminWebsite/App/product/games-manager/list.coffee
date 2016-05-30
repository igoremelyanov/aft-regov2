define 	["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid", "config"],
(nav, app, i18N, security, shell, common, config) ->	
	class ViewModel
		constructor: ->
			@shell = shell
			@gameId = ko.observable()
			@canBeEditedDeleted = ko.observable false			
			@gameManagementEnabled = ko.observable config.gameManagementEnabled			
			@compositionComplete = =>
				$("#games-grid").on "gridLoad selectionChange", (e, row) =>
					@gameId row.id
			@isAddBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.create, security.categories.gameManager
			@isEditBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.update, security.categories.gameManager
			@isDeleteBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.delete, security.categories.gameManager
		add: ->
			nav.open
				path: 'product/games-manager/edit'
				title: i18N.t("app:gameIntegration.games.new")
		edit: ->
			if @gameId()
				nav.open
					path: 'product/games-manager/edit'
					title: i18N.t("app:gameIntegration.games.edit")
					data: id: @gameId()
		deleteGame: ->
			if @gameId()
				app.showMessage i18N.t('gameIntegration.games.confirDelete'),
					i18N.t('gameIntegration.games.deleteTitle'),
					[ text: i18N.t('common.booleanToYesNo.true'), value: yes
					text: i18N.t('common.booleanToYesNo.false'), value: no ],
					false,
					style: width: "350px"
				.then (confirmed) =>
					if confirmed
						$.post "/games/delete", id: @gameId()
						.done (data) =>
							if data.success
								$('#games-grid').trigger "reload"
								app.showMessage i18N.t("gameIntegration.games.deleteSuccessful"), i18N.t("messageTemplates.dialogs.successful"), [i18N.t("common.close")]
							else
								app.showMessage data.Message, i18N.t("common.error"), [i18N.t("common.close")]
