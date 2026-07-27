#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0007-address-book.png", width: 100%),
  caption: [Use Case Diagram for UC-0007],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0007*], [*Address Book*],
    [Actor], [Customer],
    [Description], [Manage delivery addresses.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [User is Authenticated.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer views Addresses.],
      [2], [Adds or Edits Address.],
      [3], [System validates and saves.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Address saved.],
    ),

    [Related Use Cases], [UC-0002 (Checkout)],
  ),
  caption: [UC-0007: Address Book],
)

The Address Book allows customers to manage their shipping destinations efficiently. By supporting Create, Read, Update, and Delete (CRUD) operations for saved addresses, the system streamlines the checkout process (UC-0002), allowing users to select pre-validated locations rather than re-entering details for every purchase.
