# Slinqy Agent

An autonomous component that performs scaling actions.

## Design

The Slinqy Agent is single threaded and only one instance of the agent should be active at any one time.  
This way only one single thread is evaluating the state of the queue shards and attempting to make changes at a time, avoiding conflicts and odd errors.

When a Slinqy queue is created, a management queue is created along with the queues first shard. Only one message will be enqueued\processed at a time.
This way many instances of the Slinqy Agent can be running for redundancy but only one instance/thread will be active at any given time.