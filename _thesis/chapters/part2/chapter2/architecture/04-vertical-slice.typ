==== Vertical Slice Organization

In VSA, a "Slice" is equivalent to a "Use Case". Files are physically grouped by their functional purpose in the `src/libs/ReSys.Core/Features` directory.

#figure(
  raw(
    lang: "text",
    block: true,
    "
src/ReSys.Core/Features/
├── Catalog/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs      // Input DTO
│   │   ├── CreateProductHandler.cs      // Logic
│   │   └── CreateProductValidator.cs    // Rules
│   └── GetProduct/
│       ├── GetProductQuery.cs
│       └── GetProductHandler.cs
├── Orders/
│   ├── PlaceOrder/
│   │   ├── PlaceOrderCommand.cs
│   │   └── PlaceOrderHandler.cs
│   └── ShipOrder/
│       └── ...
└── Identity/
    └── ...
  ",
  ),
  caption: [Vertical Slice Architecture: Physical organization of code by Feature rather than technical layer.],
)

A typical slice (e.g., `PlaceOrder`) contains:
- *Request/Response DTOs:* Defines the public API contract for input and output, decoupled from internal entities.
- *Command/Query:* A simplified object defining the intent (e.g., `PlaceOrderCommand`).
- *Handler:* The isolated "function" that executes the logic. It takes the Command/Query and dependencies (DB, Services) and returns a Result.
- *Validator:* `FluentValidation` rules that run *before* the handler, ensuring only valid data enters the domain.
