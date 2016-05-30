@Integration

Feature: Bonus
    As a user I can manage bonuses and bonus templates

Scenario: Can not execute permission protected bonus methods
    Given I am logged in and have access token
    Then I am forbidden to execute permission protected bonus methods with insufficient permissions

Scenario: Can not execute bonus methods with invalid token
    Given I am not logged in and I do not have valid token
    Then I am unauthorized to execute bonus methods with invalid token

Scenario: Can not execute bonus methods using GET
    Given I am logged in and have access token
    Then I am not allowed to execute bonus methods using GET

Scenario: Can not execute bonus methods using POST
    Given I am logged in and have access token
    Then I am not allowed to execute bonus methods using POST