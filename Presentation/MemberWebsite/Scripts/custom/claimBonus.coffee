class ClaimBonusMemberModel extends FormBase
    constructor: ->
        super

        @claimedBonus = ko.observable()

    selectBonus: (bonusId) =>
        if (@claimedBonus() != bonusId)
            @claimedBonus bonusId
        else
            @claimedBonus undefined
            
    claim: () =>
        $.postJson '/api/ClaimBonusReward', RedemptionId: @claimedBonus()
        .done (response) =>
            redirect "/Home/ClaimBonus"

model = new ClaimBonusMemberModel
model.errors = ko.validation.group(model);
ko.applyBindings model, $("#claim-bonus-page")[0]
$("#claim-bonus-page").i18n();