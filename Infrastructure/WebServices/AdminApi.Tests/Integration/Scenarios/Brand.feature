@Integration

Feature: Brand
    As a user, I can add, view, edit, activate, deactivate brand and countries assigned to it

#CommonTests

Scenario: Can not execute permission protected brand methods
    Given I am logged in and have access token
    Then I am forbidden to execute permission protected brand methods with insufficient permissions

Scenario: Can not execute brand methods with invalid token
    Given I am not logged in and I do not have valid token
    Then I am unauthorized to execute brand methods with invalid token

Scenario: Can not not execute brand methods using GET
    Given I am logged in and have access token
    Then I am not allowed to execute brand methods using GET

Scenario: Can not not execute brand methods using POST
    Given I am logged in and have access token
    Then I am not allowed to execute brand methods using POST

#End of CommonTests

#BrandController

#Brand/GetUserBrands
Scenario: Can get user brands
    Given I am logged in and have access token
    Then Available brands are visible to me

#Brand/GetAddData
Scenario: Can get brand add data
    Given I am logged in and have access token
    Then Required data to add new brand is visible to me

#Brand/GetEditData
Scenario: Can get brand edit data
    Given I am logged in and have access token
    When New activated brand is created
    Then Required data to edit that brand is visible to me

Scenario: Can not get brand edit data with invalid brand
    Given New activated brand is created
    And New user with Update permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to get brand edit data

#Brand/GetViewData
Scenario: Can get brand view data
    Given I am logged in and have access token
    When New activated brand is created
    Then Required brand data is visible to me

Scenario: Can not get brand view data with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to get brand view data

#Brand/Add
Scenario: Can add new brand
    Given I am logged in and have access token
    Then New brand is successfully added

#Brand/Edit
Scenario: Can edit brand data
    Given I am logged in and have access token
    When New deactivated brand is created
    Then Brand data is successfully edited

Scenario: Can not edit brand data with invalid brand
    Given New activated brand is created
    And New user with Update permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to edit brand

#Brand/GetCountries
Scenario: Can get brand countries
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand countries are visible to me

Scenario: Can not get brand countries with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to get brand countries

#Brand/Activate
Scenario: Can activate brand
    Given I am logged in and have access token
    When New deactivated brand is created
    Then Brand is successfully activated

Scenario: Can not activate brand with invalid brand
    Given New deactivated brand is created
    And New user with Activate permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to activate brand

#Brand/Deactivate
Scenario: Can deactivate brand
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand is successfully deactivated

Scenario: Can not deactivate brand with invalid brand
    Given New activated brand is created
    And New user with Deactivate permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden to deactivate brand

#Brand/Brands
Scenario: Can get brands list
    Given I am logged in and have access token
    When New activated brand is created
    Then Brands are visible to me

#End of BrandController

#BrandCountryController

#BrandCountry/GetAssignData
Scenario: Get brands countries assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand country assign data is visible to me

Scenario: Can not get brands countries assign data with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden get brands countries assign data

#BrandCountry/Assign
Scenario: Assign country to the brand
    Given I am logged in and have access token
    When New activated brand is created
        And New country is created
    Then Brand country is successfully added

Scenario: Can not assign country to the brand with invalid brand
    Given New activated brand is created
    And New user with Create permission in SupportedCountries module is created
    And I am logged in and have access token
    When New country is created
    Then I am forbidden to assign country to the brand

#End of BrandCountryController

#BrandCultureController

#BrandCulture/GetAssignData
Scenario: Get brands cultures assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand culture assign data is visible to me

Scenario: Can not get brands cultures assign data with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden get brands cultures assign data

#BrandCulture/Assign
Scenario: Assign culture to the brand
    Given I am logged in and have access token
    When New activated brand is created
        And New culture is created
    Then Brand culture is successfully added

Scenario: Can not assign culture to the brand with invalid brand
    Given New activated brand is created
    And New user with Create permission in SupportedLanguages module is created
    And I am logged in and have access token
    When New culture is created
    Then I am forbidden to assign culture to the brand

#End of BrandCultureController

#BrandCurrencyController

#BrandCurrency/GetBrandCurrencies
Scenario: Get brand currencies
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand currencies are visible to me

Scenario: Can not get brand currencies with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden get brand currencies

#BrandCurrency/GetBrandCurrenciesWithNames
Scenario: Get brand currencies with names
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand currencies with names are visible to me

Scenario: Can not get brand currencies with names with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden get brand currencies with names

#BrandCurrency/GetAssignData
Scenario: Get brand currency assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand currency assign data is visible to me

Scenario: Can not get brand currency assign data with invalid brand
    Given New activated brand is created
    And New user with View permission in BrandManager module is created
    And I am logged in and have access token
    Then I am forbidden get brand currency assign data

#BrandCurrency/Assign
Scenario: Assign currency to the brand
    Given I am logged in and have access token
    When New activated brand is created
        And New currency is created
    Then Brand currency is successfully added

Scenario: Can not assign currency to the brand with invalid brand
    Given New activated brand is created
    And New user with Create permission in SupportedCurrencies module is created
    And I am logged in and have access token
    Then I am forbidden to assign currency to the brand

#End of BrandCurrencyController

Scenario: Get brand product assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product assign data is visible to me

Scenario: Assign product to the brand
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product is successfully assigned

Scenario: Get brand product bet levels
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product bet levels are visible to me

Scenario: Create new content translation
    Given I am logged in and have access token
    When New culture is created
    Then New content translation is successfully created
