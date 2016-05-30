@Integration

Feature: Admin
    As a user, I can manage country, culture, currency

# Country

#Scenario: Get countries list
#    Given I am logged in and have access token
#    Then Available countries are visible to me

Scenario: Get country by code
    Given I am logged in and have access token
    When New country is created for admin
    Then Country by code is visible to me

Scenario: Save country data
    Given I am logged in and have access token
    Then Country data is successfully saved

Scenario: Delete country
    Given I am logged in and have access token
    When New country is created for admin
    Then Country is successfully deleted

# Culture

#Scenario: Get cultures list
#    Given I am logged in and have access token
#    Then Available cultures are visible to me

Scenario: Get culture by code
    Given I am logged in and have access token
    When New culture is created for admin
    Then Culture by code is visible to me

Scenario: Activate culture
    Given I am logged in and have access token
    When New culture is created for admin
    Then Culture is successfully activated

Scenario: Deactivate culture
    Given I am logged in and have access token
    When New culture is created for admin
    Then Culture is successfully deactivated

Scenario: Save culture data
    Given I am logged in and have access token
    Then Culture data is successfully saved

# Currency

#Scenario: Get currencies list
#    Given I am logged in and have access token
#    Then Available currencies are visible to me

Scenario: Get currency by code
    Given I am logged in and have access token
    When New currency is created
    Then Currency by code is visible to me

Scenario: Activate currency
    Given I am logged in and have access token
    When New currency is created
    Then Currency is successfully activated

Scenario: Deactivate currency
    Given I am logged in and have access token
    When New currency is created
    Then Currency is successfully deactivated

Scenario: Save currency data
    Given I am logged in and have access token
    Then Currency data is successfully saved

# Admin manager

Scenario: Create user
    Given I am logged in and have access token
    When New activated brand is created
    Then New user is successfully created

Scenario: Update user
    Given I am logged in and have access token
    When New activated brand is created
    Then User is successfully updated

Scenario: Reset user password
    Given I am logged in and have access token
    When New activated brand is created
    Then User password is successfully reset

Scenario: Activate user
    Given I am logged in and have access token
    When New activated brand is created
    And New deactivated user is created
    Then User is successfully activated

Scenario: Deactivate user
    Given I am logged in and have access token
    When New activated brand is created
    And New activated user is created
    Then User is successfully deactivated

Scenario: Get edit user data
    Given I am logged in and have access token
    When New activated brand is created
    And New activated user is created
    Then User edit data is visible to me

Scenario: Get licensee data
    Given I am logged in and have access token
    When New licensee is created
    Then Licensee data is visible to me

Scenario: Save brand filter selection
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand filter selection is successfully saved

Scenario: Save licensee filter selection
    Given I am logged in and have access token
    When New licensee is created
    Then Licensee filter selection is successfully saved

# Role manager

Scenario: Create role
    Given I am logged in and have access token
    When New licensee is created
    Then New role is successfully created

Scenario: Get role
    Given I am logged in and have access token
    When New licensee is created
    And New role is created
    Then Role is visible to me

Scenario: Get role edit data
    Given I am logged in and have access token
    When New licensee is created
    And New role is created
    Then Role edit data is visible to me

#Scenario: Update role
#    Given I am logged in and have access token
#    When New licensee is created
#    Then Role is successfully updated

Scenario: Get role manager licensee data
    Given I am logged in and have access token
    When New licensee is created
    Then Role manager licensee data is visible to me

# Identification document settings

#Scenario: Create identification setting
#    Given I am logged in and have access token
#    When New activated brand is created
#    Then New identification settings is successfully created
#
#Scenario: Get identification setting edit data
#    Given I am logged in and have access token
#    When New activated brand is created
#    And New identification settings is created
#    Then Identification settings edit data is visible to me
#
#Scenario: Update identification settings
#    Given I am logged in and have access token
#    When New activated brand is created
#    And New identification settings is created
#    Then Identification settings is successfully updated
#
#Scenario: Get identification document settings licensee brands data
#    Given I am logged in and have access token
#    When New licensee is created
#    Then Identification document settings licensee brands data is visible to me

# Admin ip regulations controller

Scenario: Is ip address unique in backend ip regulations
    Given I am logged in and have access token
    Then Submitted ip address is unique

Scenario: Is ip address batch unique in backend ip regulations
    Given I am logged in and have access token
    Then Submitted ip address batch is unique

Scenario: Get admin ip regulation edit data
    Given I am logged in and have access token
    When New admin ip regulation is created
    Then Admin ip regulation edit data is visible to me

Scenario: Create admin ip regulation
    Given I am logged in and have access token
    Then New admin ip regulation is successfully created

Scenario: Update admin ip regulation
    Given I am logged in and have access token
    When New admin ip regulation is created
    Then Admin ip regulation is successfully updated

Scenario: Delete admin ip regulation
    Given I am logged in and have access token
    When New admin ip regulation is created
    Then Admin ip regulation is successfully deleted

# Brand ip regulations controller

Scenario: Is ip address unique in brand ip regulations
    Given I am logged in and have access token
    Then Submitted brand ip address is unique

Scenario: Is ip address batch unique in brand ip regulations
    Given I am logged in and have access token
    Then Submitted brand ip address batch is unique

Scenario: Get licensee brands in brand ip regulations
    Given I am logged in and have access token
    When New licensee is created
    Then Licensee brands are visible to me

Scenario: Get brand ip regulation edit data
    Given I am logged in and have access token
    When New activated brand is created
    And New brand ip regulation is created
    Then Brand ip regulation edit data is visible to me

Scenario: Create brand ip regulation
    Given I am logged in and have access token
    When New activated brand is created
    Then New brand ip regulation is successfully created

Scenario: Update brand ip regulation
    Given I am logged in and have access token
    When New activated brand is created
    And New brand ip regulation is created
    Then Brand ip regulation is successfully updated

Scenario: Delete brand ip regulation
    Given I am logged in and have access token
    When New activated brand is created
    And New brand ip regulation is created
    Then Brand ip regulation is successfully deleted

# Common

@ignore
Scenario: Can not execute permission protected admin methods
    Given I am logged in and have access token
    Then I can not execute protected admin methods with insufficient permissions