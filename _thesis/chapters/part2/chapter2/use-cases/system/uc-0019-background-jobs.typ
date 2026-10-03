#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/system/uc-0019-background-jobs.png", width: 100%),
  caption: [Use Case Diagram for UC-0019],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0019*], [*Background Job Processing*],
    [Actor], [System],
    [Description], [Orchestrate asynchronous tasks (Email, Maintenance).],
    [Trigger], [Schedule or Event.],
    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Job Scheduler triggers task.],
      [2], [Task executes logic (e.g., Send Order Email).],
      [3], [On Failure: Retry with exponential backoff.],
      [4], [On Success: Mark complete.],
    ),

    [Related Use Cases], [UC-0006 (Order Status)],
  ),
  caption: [UC-0019: Background Job Processing],
)

The Background Job Processing system handles long-running or scheduled tasks that should not block user interactions. Using a durable queue (Hangfire), it reliably executes dependencies such as sending order confirmation emails, clearing expired carts, and generating daily reports. It includes built-in retry logic to handle transient failures, ensuring system robustness.
