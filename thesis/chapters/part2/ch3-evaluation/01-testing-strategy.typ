== Testing Strategy

The ReSys.Shop platform was subjected to a multi-level testing strategy covering three distinct layers: unit tests for isolated logic, integration tests for component interactions, and end-to-end tests for critical user workflows. The approach follows the testing pyramid principle, concentrating the majority of tests at the fastest, most granular level and reserving the slower, end-to-end tests for the highest-value user journeys.

=== Unit Testing

Unit tests form the foundation of the verification strategy:

+ The .NET backend uses xUnit v3 to test handler logic, domain invariants, and validation rules in isolation.
+ Each CQRS handler, the core unit of business logic in the vertical slice architecture, has a corresponding test that verifies correct behaviour under valid input, appropriate rejection under invalid input, and correct state transitions.
+ Domain invariants such as the order state machine transitions and the inventory non-negative stock constraint are enforced through tests that execute without any external dependencies.
+ The Python machine learning sidecar uses pytest to validate the embedding generation pipeline, ensuring each supported model produces vectors of the expected dimensionality and that the preprocessing pipeline normalises input images to model-expected formats.

=== Integration Testing

Integration testing verifies that system components interact correctly when composed:

+ Testcontainers provisions ephemeral PostgreSQL and Redis instances for each test run, ensuring database queries, including vector similarity search with pgvector, are tested against real infrastructure.
+ Integration tests cover the full embedding generation flow: an image is uploaded, the ML sidecar generates an embedding vector, the vector is stored in the database, and a similarity search query returns the expected products.
+ Cross-service communication between the .NET backend and the Python sidecar is validated, establishing that the HTTP contract between the two services is honoured.

=== End-to-End Testing

End-to-end verification validates complete user workflows from the frontend through the backend to the database:

+ The key user flows — visual search, checkout, and admin product management — were verified manually using documented HTTP test files that simulate the sequence of API calls a frontend client makes during a real user session.
+ Automated end-to-end testing via Playwright covers the most critical paths: the visual search flow from image upload to results display, and the checkout flow from cart addition to order confirmation.
+ These tests run against a fully deployed containerized environment, giving confidence that the system operates correctly when all services are composed.