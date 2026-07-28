=== System Use Cases

The System actor represents automated background processes that maintain data consistency, generate embeddings, and perform scheduled operations. Coordination relies on *Hangfire* @hangfire-docs for job scheduling and *Redis* @redis-docs for persistence.

#include "system/system.typ"
