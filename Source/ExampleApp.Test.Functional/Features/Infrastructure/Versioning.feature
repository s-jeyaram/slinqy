Feature: Version
	As a User
	I want to know what version I'm using
	So that I can understand how the system should act

Scenario: Deployed Version Matches Test Version
	Given I navigate to the Home page
	Then the Application Version matches the Test Version