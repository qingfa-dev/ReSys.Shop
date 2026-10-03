===== Inventory Management: UC-ADM-STK, UC-ADM-LOC

*Stock list.* Table grouped by product showing each variant with SKU, size/colour, on-hand, reserved, available (computed), and low-stock indicator (red badge when below threshold). Filterable by location, category, and low-stock-only (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-inventory-stock.png", width: 100%),
  caption: [Inventory stock: On Hand/Reserved/Available per variant, low-stock badge.],
) <fig-admin-inventory-stock>

*Stock movements.* Append-only audit log: timestamp, product/variant, movement type (Receiving, Selling, Returning, Transferring), quantity delta, before/after quantities, reason, operator. Paginated and filterable (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-inventory-movements.png", width: 100%),
  caption: [Stock movements: audit log with +/- quantity deltas, reasons, and filters.],
) <fig-admin-inventory-movements>

*Stock locations.* Management table with location name, address, active toggle, item count. Restock form: product/variant selection, quantity, unit cost, reason. Transfer form: source/destination locations, variant, quantity; lifecycle progresses Created to In-Transit to Received (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-stock-locations.png", width: 100%),
  caption: [Stock locations: table with name, address, active badge, item count.],
) <fig-admin-stock-locations>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-inventory-restock.png", width: 100%),
  caption: [Restock dialog: variant, quantity, unit cost, reason, and notes.],
) <fig-admin-inventory-restock>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-inventory-transfer.png", width: 100%),
  caption: [Transfer dialog: source/destination locations, variant, quantity.],
) <fig-admin-inventory-transfer>
