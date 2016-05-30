define 	["nav", "i18next", "security/security", "./activate-dialog", "shell", "bonus/bonusCommon", "config", "controls/grid"],
(nav, i18N, security, activateModal, shell, common, config) ->	
	class ViewModel
		constructor: ->
			@shell = shell
			@config = config
			@bonusId = ko.observable null
			@isActivateEnabled = ko.observable false
			@isDeactivateEnabled = ko.observable false
			@search = ko.observable ""
			@selectedBrands = ko.computed -> shell.selectedBrandsIds()
			@isEditEnabled = ko.computed => @bonusId() and @isActivateEnabled()
		typeFormatter: -> common.typeFormatter @Type
		issuanceModeFormatter: -> common.issuanceModeFormatter @Mode
		statusFormatter: -> i18N.t "bonus.bonusStatuses.#{@IsActive}"
		isAddBtnVisible: ko.computed ->
			security.isOperationAllowed security.permissions.create, security.categories.bonusManager
		isEditBtnVisible: ko.computed ->
			security.isOperationAllowed security.permissions.update, security.categories.bonusManager
		isViewBtnVisible: ko.computed ->
			security.isOperationAllowed security.permissions.view, security.categories.bonusManager
		isActivateBtnVisible: ko.computed ->
			security.isOperationAllowed security.permissions.activate, security.categories.bonusManager
		isDeactivateBtnVisible: ko.computed ->
			security.isOperationAllowed security.permissions.deactivate, security.categories.bonusManager
		openAddBonusTab: ->
			nav.open
				path: "bonus/bonus-manager/add-edit-bonus"
				title: i18N.t "bonus.bonusManager.new"
		openEditBonusTab: ->
			if @bonusId()
				nav.open
					path: "bonus/bonus-manager/add-edit-bonus"
					title: i18N.t "bonus.bonusManager.edit"
					data: id: @bonusId()
		openViewBonusTab: ->
			if @bonusId()
				nav.open
					path: "bonus/bonus-manager/view-bonus"
					title: i18N.t "bonus.bonusManager.view"
					data: id: @bonusId()
		showModalDialog: (isActive) ->
			modal = new activateModal @bonusId()
			modal.show(isActive)
		reloadGrid: -> 	$('#bonus-grid').trigger "reload"
		compositionComplete: =>
			$("#bonus-grid").on "gridLoad selectionChange", (e, row) =>
				@bonusId row.id
				#check that bonus has template
				if row.data.BrandId isnt ""
					@isActivateEnabled row.data.IsActive is "Inactive" and @bonusId()?
					@isDeactivateEnabled row.data.IsActive is "Active" and @bonusId()?
				else
					@isActivateEnabled no
					@isDeactivateEnabled no
			$("#bonus-search").submit =>
				@search $('#bonus-name-search').val()
				false
			$(document).on "bonuses_changed", @reloadGrid
		detached: => $(document).off "bonuses_changed", @reloadGrid