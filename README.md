# Dissertation Project Microservice Backend

For the React-based frontend code, please refer to the other GitHub repository:
https://github.com/Mayron/Dissertation-Project-React-Frontend/blob/master/README.md

# About

This dissertation investigates solutions for overcoming data consistency challenges when using the Database-per-Service pattern across a distributed microservice architecture (MSA). It compares previous concurrency control protocols used to fulfil global transactions and discusses how such protocols can be used at the application level to improve scalability and performance. As part of this project's evaluation, timestamp-based concurrency control using saga orchestration reduced the number of data inconsistency anomalies.

Event sourcing was implemented with the Akka.Net actor framework to preserve the domain model's consistency for improved fault-tolerance. This dissertation shows how the actor model can implement message-driven inter-process communication between microservices, where each actor reacts to incoming messages. It also discusses how complex stateful classes, such as saga orchestrators implemented using the finite-state machine model, can be persisted between restarts to preserve data consistency.

The implementation of the MSA created as part of this research attempts to maintain data consistency while also resulting in a reactive system, as defined by the reactive manifesto. Therefore, the responsiveness of message types and the quality of the user experience are both evaluated. Finally, a qualitative evaluation of each implemented design pattern examines how they promote designing for change and software engineering best practices.
