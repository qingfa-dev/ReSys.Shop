#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0013-analytics.png", width: 100%),
  caption: [Use Case Diagram for UC-0013],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0013*], [*Analytics Dashboard*],
    [Actor], [Administrator],
    [Description], [View high-level sales KPIs in the Dashboard to monitor business performance.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Admin has Dashboard permissions.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin accesses Dashboard Home.],
      [2], [System queries Sales Summary (Revenue, Order Count).],
      [3], [System queries Inventory Summary (Low Stock).],
      [4], [Displays KPIs, Charts, and Recent Activity.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Real-time metrics displayed.],
    ),

    [Related Use Cases], [UC-0012 (Inventory)],
  ),
  caption: [UC-0013: Analytics Dashboard],
)

The Analytics Dashboard offers a high-level overview of business health, aggregating key performance indicators (KPIs) such as Total Revenue, Order Volume, and Low Stock Warnings. It synthesizes data from the Order, Catalog, and Inventory services into a cohesive visual report, allowing administrators to make data-driven decisions regarding restocking and marketing.
