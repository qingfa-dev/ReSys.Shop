=== Architectural Style and Rationale

The application adopts a *Distributed Vertical Slice Architecture*. This choice addresses the specific dual nature of the project: a transactional e-commerce core and a computational AI engine.

+ *Distributed Services:*
  The system is split into two primary backend services to separate concerns based on technology strengths:
  - *Core API (.NET 10):* Handles high-concurrency user requests, business rules, and data integrity. .NET was selected for its robust type system and performance.
  - *ML Service (Python 3.12):* Handles tensor operations and model inference. Python was chosen for its rich ecosystem of AI libraries (PyTorch, Transformers).
  This separation allows the ML service to be scaled independently (e.g., on GPU nodes) without duplicating the transactional overhead of the core application.

+ *Vertical Slices (Backend):*
  Unlike traditional "Layered Architecture" (Controller $->$ Service $->$ Repository), which groups code by technical concern, this system groups code by *Feature* (e.g., "Add to Cart", "Update Profile").
  - *Rationale:* In e-commerce, changes are almost always vertical (e.g., adding a discount field affects the API, logic, and database). VSA keeps all these related files together, reducing "context switching" and the risk of ripple effects across unrelated features.
