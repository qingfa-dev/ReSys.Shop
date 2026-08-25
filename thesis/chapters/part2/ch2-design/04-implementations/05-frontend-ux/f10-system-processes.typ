===== System Processes: UC-SYS-EMB, UC-SYS-MNT

Background automation is monitored through the Hangfire dashboard accessible at #emph[/jobs]. The overview displays total succeeded/failed jobs, recurring schedules, and queue depths. Recurring jobs: cart expiry (hourly, releases inventory for carts inactive 7 days), reservation expiry (every minute: releases holds exceeding the 15-minute timeout); failed embedding generations retry up to 3 times; payment webhook processing is triggered by Stripe events (validates HMAC, checks idempotency, updates state). The monitoring interface is illustrated below.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-dashboard-overview.png", width: 100%),
  caption: [Hangfire dashboard overview: job metrics with realtime and history graphs.],
) <fig-hangfire-overview>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-queues.png", width: 100%),
  caption: [Queues: queue depths, next jobs, and per-state job counts.],
) <fig-hangfire-queues>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/hangfire-job-detail.png", width: 100%),
  caption: [Job detail: method, parameters table, state history timeline.],
) <fig-hangfire-job-detail>
