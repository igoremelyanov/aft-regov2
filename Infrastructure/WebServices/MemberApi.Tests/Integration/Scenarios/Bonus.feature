@Integration

Feature: Bonus
	As a software tester I want Bonus Controller GET/POST requests refactored  
	So that proper status code are shown

Scenario: Validate 200 status code for Bonus Controller GET Requests API
	Given User with valid credentials
	When I try to send GET request to Bonus  Controller API <end point>
	Then I should see Status Code 200 Successful and Response schema validated

Scenario: Validate 201 status code for Bonus  Controller POST Requests API
	Given User with valid credentials
	When I try to send POST request to Bonus Controller  API <end point>
	Then I should see Status Code 201 Created and Response schema validated

Scenario: Validate 400 status code for Bonus Controller API
	Given User with valid credentials
	When I try to send request with invalid parameters to Bonus Controller  API <end point>
	Then I should see Status Code 400 Bad Request

Scenario: Validate 401 status code for Bonus Controller API
	Given User with invalid login password combination tries to use the system
	When I try to send request to Bonus Controller  API <end point>
	Then I should see Status Code 401 Unauthorised

Scenario: Validate 404 status code for Bonus Controller API
	Given User with valid credentials
	When I try to send request to Bonus Controller API which has invalid URI
	Then I should see Status Code 404 Not Found

Scenario: Validate 500 status code for Bonus Controller
	Given User with valid credentials
	When I try to send request to Bonus Controller and service is not available
	Then I should see Status for not available service