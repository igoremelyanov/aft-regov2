@Integration

Feature: Player
	As a user I can use player related functionality

Scenario: send online deposit request
	Given I am logged in and have access token for player
	Then Online deposit form data is visible to me
	When I submit online deposit request
	Then Online deposit request is submitted successfully
	When I pay on payment gateway
	Then Online deposit is processed successfully
	When I query the online deposit
	Then Online deposit is approved

Scenario: Validate 200 status code for Player Controller GET Requests API
	Given I am logged in and have access token for player
	When I try to send GET request to Player Controller API <end point>
	Then I should see Status Code 200 Successful for each <end point>

Scenario: Validate 400 status code for Player Controller API
	Given I am logged in and have access token for player
	When I try to send request with invalid parameters to Player Controller  API <end point>
	Then As a response I should see Status Code 400 Bad Request 

Scenario: Validate 401 status code for Player Controller API
	Given User with invalid login password combination
	When I try to send GET request to Player Controller end point
	Then I should see Status Code 401 Unauthorised Request 

Scenario: Validate 500 status code for Player Controller API
	Given I am logged in and have access token for player
	When I try to send GET request and logical server error occurs
	Then I should see Status Code 500 indicating internal server error

Scenario: Validate 201 status for Player Controller API
	Given I am logged in and have access token for player
	When I try to send POST request to Player controller end point
	Then I should see status code 201 and uri which points to the updated entity