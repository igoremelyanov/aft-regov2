@Integration

Feature: Player
	As a user I can manage player related functionality

Scenario: Get add player data
    Given I am logged in and have access token
    Then Required data to add new player is visible to me

Scenario: Get add player brands
    Given I am logged in and have access token
    When New licensee is created
    Then Required data to add player brands is visible to me

Scenario: Get add player brand data
    Given I am logged in and have access token
    When New brand is created
    Then Required data to add player brand data is visible to me

Scenario: Get payment levels
    Given I am logged in and have access token
    When New brand is created
    Then Payment levels are visible to me

Scenario: Get vip levels
    Given I am logged in and have access token
    When New brand is created
    Then Vip levels are visible to me

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Change vip level
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    And New vip level is created
#    Then Vip level is successfully changed

Scenario: Change payment level
     Given I am logged in and have access token
     When New brand is created
     And New player is created
     And New payment level is created
     Then Payment level is successfully changed

Scenario: Change players payment level
     Given I am logged in and have access token
     When New brand is created
     And New player is created
     And New player 2 is created
     And New player 3 is created
     And New payment level is created
     Then Players Payment level is successfully changed

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Send new password
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then New password is sent to player

Scenario: Add new player
    Given I am logged in and have access token
    Then New player is created

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Get player for bank account
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Required player data is visible to me

Scenario: Get bank account
    Given I am logged in and have access token
    When New bank account is created
    Then Required brank account data is visible to me

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Get current bank account
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    And New player bank account is created
#    Then Current bank account for player is visible to me

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Save bank account
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    And New bank account is created
#    Then Player bank account is successfully added

Scenario: Set current bank account
    Given I am logged in and have access token
    When New bank account is created
    Then Current bank account is successfully set

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Edit log remark
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    And New activity log is created
#    Then Activity log remark is successfully edited

# TODO: Throws excepion: The provider for the source IQueryable doesn't implement IDbAsyncQueryProvider. Only providers that implement IDbAsyncQueryProvider can be used for Entity Framework asynchronous operations.
#Scenario: Get player balances
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Player balances are visible to me

Scenario: Get transaction types
    Given I am logged in and have access token
    Then Transaction types are visible to me

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Get identification document edit data
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Identification document edit data is visible to me

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Edit player data
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Player data is successfully edited

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Submit exemption
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Exemption is successfully submitted

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Set player status
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Player status is successfully set

#TODO: Uncomment, when Anthon merges ApplicationSeeder class changes
#Scenario: Resend activation email
#    Given I am logged in and have access token
#    When New brand is created
#    And New player is created
#    Then Activation email is successfully resent

Scenario: Can not execute permission protected player methods
    Given I am logged in and have access token
    Then I can not execute protected player methods with insufficient permissions