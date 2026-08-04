#import "../../../../../template/ctu-styles.typ": context-callout, pseudocode
===== Atomic Stock Reservation (Concurrency Control)
#context-callout(title: "Architectural Choice: Serializable Isolation")[
  While `Read Committed` is the default isolation level in PostgreSQL, the inventory module explicitly elevates to `Serializable` during stock reservation. This prevents "Phantom Reads" and "Write Skew," ensuring that the quantity checked at step 2 remains valid throughout the transaction until commit.
]

/*
#pseudocode(title: "Algorithm 3: Atomic Stock Reservation (Pessimistic Locking)")[
  *Input:* productId, quantityRequested \
  *Output:* ReservationResult (Success/Failure) \
  \
  1. *Begin:* Start a new `Serializable` database transaction \
  2. *Lock:* `SELECT * FROM InventoryItems` \
    `WHERE ProductId = productId` \
    `FOR UPDATE` (Acquire row-level exclusive lock) \
  3. *Check:* If `QuantityAvailable < quantityRequested`: \
    a. Rollback Transaction \
    b. Return Failure("Insufficient Stock") \
  4. *Deduct:* `QuantityAvailable -= quantityRequested` \
  5. *Reserve:* `QuantityReserved += quantityRequested` \
  6. *Commit:* Finish transaction and release lock \
  7. *Return:* Success
]
*/

1. Start Transaction.
2. Lock `InventoryItem` row (Database-level row lock).
3. Check `QuantityAvailable > RequestedQuantity`.
4. Update `QuantityReserved += RequestedQuantity`.
5. Commit Transaction.

If two customers attempt to buy the last item simultaneously, the database lock ensures they are processed sequentially, and the second request fails gracefully.

The atomic stock reservation process is critical for preventing overselling. When the `PlaceOrderCommand` is dispatched, the system initiates a *Serializable* database transaction. This isolation level ensures that the check-and-reserve logic is strictly atomic. The handler first executes a "Select for Update" query on the `InventoryItem` record, effectively acquiring an exclusive row-level lock. Only after the lock is secured does the logic verify availability; if sufficient, the `QuantityReserved` is incremented, and the transaction is committed. Any competing transaction attempting to lock the same row is forced into a WAIT state by the RDBMS until the first transaction completes, guaranteeing absolute inventory consistency under high concurrent load.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0017-reservations.png", width: 65%),
  caption: [Atomic Stock Reservation Sequence: Verifying availability and locking inventory rows within a Serializable transaction.],
) <fig:sq-0017>

From a User Experience perspective, this backend locking mechanism is visualized via a "Stock Hold Timer". Once the system successfully acquires the reservation (lock), the user is presented with a countdown (e.g., "Items reserved for 10:00"), strictly informing them of the guaranteed window to complete the purchase. This transparency reduces anxiety and enforces the fairness of the "First-Come-First-Serve" event model.


// TODO: [Implementation] Add Transaction Log snippet.
// #figure(
//   figure-placeholder("Log Output: Stock Reservation Transaction Logs (UC-0018)"),
//   caption: [Transaction Log Excerpt: Evidence of Serializable isolation level preventing overselling during concurrent access.],
// )

*Concurrency Verification:*
- *UI Feedback (Timer):* From a User Experience perspective, this backend locking mechanism is visualized via a "Stock Hold Timer". Once the system successfully acquires the reservation (lock), the user is presented with a countdown (e.g., "Items reserved for 10:00"), strictly informing them of the guaranteed window to complete the purchase.
- *Sequence Flow:* Referring to sequence @fig:sq-0017, the transaction logs would show a strict interleaving of `BEGIN` $\to$ `SELECT FOR UPDATE` $\to$ `COMMIT`. If a second request arrives at $T+1$, the database engine blocks the `SELECT` until the first lock is released, creating a linearized history of stock deductions that is free of race conditions.
