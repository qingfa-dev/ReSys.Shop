# Label Quick Reference

A condensed lookup table for all labels defined in the Code Commenting Standard v3.0.
Claude reads this file when it needs to quickly identify the correct label without loading the full standard.

## CAT-1 Validation
| Label | One-line trigger |
|-------|-----------------|
| `Validate:` | Format or business-rule requirement |
| `Check:` | Null, existence, or permission check |
| `Guard:` | Defensive boundary; name the threat |
| `Verify:` | Post-condition or system state check |
| `Assert:` | Dev-time invariant; stripped in prod |

## CAT-2 Object Operations
| Label | One-line trigger |
|-------|-----------------|
| `Create:` | New domain object or value type |
| `Assign:` | Set a property; explain why |
| `Update:` | Modify existing object state |
| `Add:` | Insert into a collection |
| `Remove:` | Delete from collection / soft-delete |
| `Clone:` | Independent copy (specify deep/shallow) |
| `Merge:` | Combine sources with conflict resolution |
| `Initialize:` | Populate defaults during setup |
| `Reset:` | Return to known default state |

## CAT-3 Processing Logic
| Label | One-line trigger |
|-------|-----------------|
| `Compute:` | Derive a calculated value; state formula |
| `Transform:` | Convert representation; name both sides |
| `Parse:` | Extract structure from raw input |
| `Format:` | Structure for display/serialisation/logging |
| `Filter:` | Remove elements by predicate |
| `Generate:` | Produce IDs, hashes, tokens |
| `Normalize:` | Standardise format or encoding |
| `Aggregate:` | Combine into summary metric |
| `Sort:` | Order by business-meaningful criterion |
| `Explain:` | Algorithmic/mathematical reasoning |

## CAT-4 Events and Business Rules
| Label | One-line trigger |
|-------|-----------------|
| `Enforce:` | Business invariant that must not be violated |
| `Raise:` | Internal domain event |
| `Trigger:` | External workflow, saga, background process |
| `Notify:` | Dispatch notification to user/system |
| `Handle:` | Process inbound domain/integration event |
| `Subscribe:` | Register listener for events |
| `Policy:` | Reference named compliance/business policy |

## CAT-5 Flow Control
| Label | One-line trigger |
|-------|-----------------|
| `Await:` | Async wait; document timeout/cancellation |
| `Retry:` | Re-attempt with defined policy |
| `Skip:` | Bypass path under well-defined condition |
| `Fallback:` | Alternative when primary fails |
| `Batch:` | Group operations for efficiency/compliance |
| `Throttle:` | Limit rate; state the limit |
| `Defer:` | Postpone to queue/scheduler |
| `Continue:` | Next loop iteration (readability) |
| `Break:` | Exit loop/pipeline early |
| `Circuit:` | Circuit breaker; name threshold and state |

## CAT-6 Resource Management
| Label | One-line trigger |
|-------|-----------------|
| `Acquire:` | Obtain scarce resource |
| `Release:` | Explicitly free resource |
| `Lock:` | Exclusive access to shared resource |
| `Cache:` | Store value; include TTL + invalidation |
| `Purge:` | Remove stale/expired entries |
| `Pool:` | Borrow/return from pool |
| `Dispose:` | Release managed/unmanaged resources |

## CAT-7 Error Handling
| Label | One-line trigger |
|-------|-----------------|
| `Catch:` | Handle specific named exception |
| `Recover:` | Restore valid system state |
| `Compensate:` | Roll back side-effects (saga) |
| `Degrade:` | Safe subset of functionality |
| `Escalate:` | Re-throw to higher-level handler |
| `Suppress:` | Swallow non-critical; ALWAYS explain why |

## CAT-8 Integration
| Label | One-line trigger |
|-------|-----------------|
| `Call:` | External service invocation; name system + version |
| `Send:` | Transmit command/document |
| `Receive:` | Ingest from external source |
| `Publish:` | Broadcast event to message bus |
| `Map:` | Convert between data models at boundary |
| `Serialize:` | To transmittable format; state format |
| `Deserialize:` | Reconstruct typed object from raw |
| `Webhook:` | Handle + validate inbound webhook |

## CAT-9 Observability
| Label | One-line trigger |
|-------|-----------------|
| `Log:` | Business event or diagnostic fact |
| `Trace:` | Distributed trace span |
| `Monitor:` | Metric counter, gauge, histogram |
| `Audit:` | Tamper-evident compliance record |
| `Profile:` | Bracket performance-sensitive section |
| `Debug:` | TEMP diagnostic; remove before merge |

## CAT-10 AI / Agent Annotations
| Label | Format | When |
|-------|--------|------|
| `Contract:` | `pre=COND, post=COND, throws=EX` | Formal function contract |
| `Invariant:` | `COND [; COND]` | Class/struct invariant |
| `Assume:` | `COND — REASON` | Non-enforced assumption |
| `AgentHint:` | `INSTRUCTION [; do NOT: X]` | Guidance for AI editors |
| `AgentSkip:` | `REASON` | Block agents must not touch |
| `Boundary:` | `LAYER → LAYER — REASON` | Architectural boundary |
| `Context:` | `DESCRIPTION — see REF` | Background + link to source |

## Temporal Markers
| Marker | Format |
|--------|--------|
| `TODO` | `TODO(owner, YYYY-MM-DD): reason — TICKET` |
| `FIXME` | `FIXME(owner, YYYY-MM-DD): defect — TICKET` |
| `HACK` | `HACK(owner, YYYY-MM-DD): workaround — TICKET` |
| `TEMP` | `TEMP: reason` |
| `DEADLINE` | `DEADLINE(YYYY-MM-DD): reason — TICKET` |
| `DEPRECATED` | `DEPRECATED(YYYY-MM-DD): use X instead` |
| `BREAKING` | `BREAKING(YYYY-MM-DD): what changes — semver note` |
| `PERF` | `PERF(owner, YYYY-MM-DD): measurement — target — TICKET` |