Feature: Send Queue Messages
	As a Queue Sender
	I want to be able to always send messages to the queue
	So that I can be sure my messages will eventually be handled

Scenario: Queue Writer can send a single Queue Message
	Given a Queue
	When a message is sent
	Then the message can be received