@Integration

Feature: Game
	As a user I can use game related functionality

Scenario: Validate 200 status code for Game Controller GET Requests API
	Given Access Token for player is available
	When I try to send GET request to Game Controller API <end point>
	Then I should see Status Code 200 Successful for all of the calls

Scenario: Validate 400 status code for Game Controller GET methods 
	Given Access Token for player is available
	When I try to send request to Game Controller with validation errors in the request
	Then I should see Status Code 400 for all of the calls

Scenario: Validate 401 status code for Game Controller API
	Given Login with invalid credentials
	When I try to send GET request to Game Controller end point
	Then I should see Status Code Unauthorised Request 400 