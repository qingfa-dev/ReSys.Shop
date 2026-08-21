===== System Processes: UC-SYS-EMB, UC-SYS-MNT

Background automation is monitored through the Hangfire dashboard accessible at `/jobs`. The overview displays total succeeded/failed jobs, recurring schedules, and queue depths. Recurring jobs: cart expiry (every 20 minutes, releases inventory for carts inactive 7 days), embedding retries (exponential back-off: 1-2-4-8 minutes, max 3 retries, permanent failures flagged), index maintenance (nightly: analyses HNSW state, rebuilds when thresholds exceeded), reservation expiry (every 15 minutes: releases holds exceeding timeout), payment webhook processing (triggered by Stripe events: validates HMAC, checks idempotency, updates state). The monitoring interface is illustrated below.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-dashboard-overview.png", width: 100%),
  caption: [Hangfire dashboard overview: metrics bar (Jobs, Retries, Recurring Jobs, Servers), realtime and history graphs.],
) <fig-hangfire-overview>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-queues.png", width: 100%),
  caption: [Queues: table with Queue, Length, Fetched, and next jobs per queue, plus per-state job counts (Enqueued, Scheduled, Processing, Succeeded, Failed, Deleted, Awaiting).],
) <fig-hangfire-queues>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-job-detail.png", width: 100%),
  caption: [Job detail: job method and state at top, parameters table, state history timeline.],
) <fig-hangfire-job-detail>
