Feature: Receive Queue Messages
	As a Queue Receiver
	I want to be able to always receive messages from the queue
	So that I can process them

Scenario: All queued messages can be read from a scaled out queue
	Given a Queue whose storage has scaled out
	When the Queue Receiver is restored
	Then the all the messages can be received