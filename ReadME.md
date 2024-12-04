# Welcome to Chatier

`Chatier` is a simple and efficient messenger application designed to facilitate seamless communication.

## Requirements

1. Users should be able to send messages to each other.
2. Users must be notified when they receive a message.
3. Users can create groups.
4. Users must be notified when they are added to a group.
5. If a user doesn't read a notification within x minutes, they should be notified by an external service.
6. __Nice to have__: Persistence, resiliency, and scalability.

### Before starting

Before starting, please take some time and imagine how you would implement this service. 

Consider the following:

1. What patterns are you going to use?
2. How is your architecture going to look like?
3. What about the development and testing process?
4. How are you going to run and deploy it?

## Solution

My implementation is based on `Orleans`. 

This framework helps you design your large service as set of small `grains` and use general `OOP` approach. 

### Stack

1. .NET 9
2. Orleans
3. MSTests

### How to run

The main flow is implemented as a functional test.

1. Open the test project.
2. Run the tests.
3. Observe and learn the flow.


## Conclusion 

Feel free to ask if you need any further improvements or have additional questions!
