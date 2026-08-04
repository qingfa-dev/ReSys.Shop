=== Redis Caching

Redis 7 operates alongside the .NET *HybridCache* abstraction in a two-tier arrangement @redis-docs.

- *L1: in-process.* Frequently accessed data (taxonomy trees, front-page product lists) is held in application memory with sub-millisecond read latency. Cache entries expire on a configurable sliding window, typically five minutes.

- *L2: Redis.* The shared tier synchronises cache across application instances. Redis also backs Hangfire job queues and guest session storage. On cache miss at L1, the value is retrieved from Redis and promoted to L1, ensuring subsequent hits stay in-process.
