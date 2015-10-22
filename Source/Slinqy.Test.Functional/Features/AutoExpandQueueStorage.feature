Feature: AutoExpandQueueStorage
	As a Queue Writer
	I want my queue storage capacity to expand when it nears capacity
	So that I can always store my messages

Scenario: Queue Storage Expands When Utilization Reaches Threshold
	Given a Queue with Storage Utilization Scale Up Threshold set
	When the Queue Storage Utilization reaches the Scale Up Threshold
	Then the Queue Storage Capacity expands