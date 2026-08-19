===== System Processes: UC-SYS-EMB, UC-SYS-MNT

Background automation is monitored through the Hangfire dashboard accessible at `/hangfire`. The overview displays total succeeded/failed jobs, recurring schedules, and queue depths. Recurring jobs: cart expiry (every 20 minutes, releases inventory for carts inactive 7 days), embedding retries (exponential back-off: 1-2-4-8 minutes, max 3 retries, permanent failures flagged), index maintenance (nightly: analyses HNSW state, rebuilds when thresholds exceeded), reservation expiry (every 15 minutes: releases holds exceeding timeout), payment webhook processing (triggered by Stripe events: validates HMAC, checks idempotency, updates state). The monitoring interface is illustrated below.

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-dashboard-overview.png", width: 100%),
//   caption: [Hangfire dashboard: metrics bar (Succeeded 12,450, Failed 3 red badge, Recurring Jobs 5, Queues 3). Recurring Jobs table: Cart Expiry (20 min), Embedding Retries, Index Maintenance (nightly), Reservation Expiry (15 min), Webhook Processing. Each shows cron/interval, last/next execution, success count.],
// ) <fig-hangfire-overview>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-job-detail.png", width: 100%),
//   caption: [Job detail (Cart Expiry): history table with Job ID, Created, State, Duration. "Last 100 executions" duration chart. Failed job entry highlighted red with "Retry" button.],
// ) <fig-hangfire-job-detail>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-queues.png", width: 100%),
//   caption: [Queues: 3 panels (default, embedding, maintenance) each with enqueued, processing, scheduled counts. Embedding queue: 2 enqueued (failed retries), 0 processing.],
// ) <fig-hangfire-queues>
