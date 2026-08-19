# Thesis Rewrite — Part 2, Chapter 2, Sections 2.1–2.2

This section of the chapter needed fewer edits than Chapter 1, the requirements traceability and use case counts were genuinely solid. Two real fixes, both in §2.1, plus one optional clarification you can take or leave.

All edits trace back to `thesis-review-part2-chapter2-sections1-2.md` and the master fix list.

---

## Edit 1 — §2.1 opening, requirement/module count (p.30)

**BEFORE:**
> The platform delivers **88 functional requirements across nine business modules**, each enforcing domain invariants expressed through entity validation rules and application-layer checks. Five non-functional quality dimensions define performance, security, modularity, observability, and reliability targets that shaped architectural decisions throughout design. Feature classification distinguishes three core research contributions, detailed in Sections 2.3 and 2.4, from four supporting infrastructure modules that provide the realistic evaluation context described in Section 3.2.
> - Functional Requirements. Traceable per module: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, Location, **and Dashboard**.

**AFTER:**
> The platform delivers **87 functional requirements across eight business modules**, each enforcing domain invariants expressed through entity validation rules and application-layer checks. Five non-functional quality dimensions define performance, security, modularity, observability, and reliability targets that shaped architectural decisions throughout design. Feature classification distinguishes three core research contributions, detailed in Sections 2.3 and 2.4, from four supporting infrastructure modules that provide the realistic evaluation context described in Section 3.2.
> - Functional Requirements. Traceable per module: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, and Location.

**Why:** I counted every requirement ID directly from Tables 10–17 (CAT 22, IDN 16, INV 12, ORD 14, PAY 10, SHP 6, PRF 3, LOC 4 = 87), and there's no `DSH-FR-XX` requirement anywhere in the thesis. Dashboard is a real feature (it shows up later with one API endpoint in Table 50), it just never got a functional-requirements table of its own, so it doesn't belong in a sentence specifically counting *functional requirements* by module. If you'd rather keep Dashboard in the module list, the alternative fix is to add its missing requirements table instead of removing it here, in which case the count would need to go up rather than down. Either fix works; just pick one and make the number match.

---

## Edit 2 — Table 19, Model Benchmark System row (§2.1.3, p.40)

**BEFORE:**
> Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across **11 embedding models**, providing model selection guidelines for deployment.

**AFTER:**
> Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across **four representative embedding models, selected from six supported by the framework**, providing model selection guidelines for deployment.

**Why:** same "eleven models" correction applied throughout the thesis, Table 55's actual registry has six entries, and four of those six are the ones formally benchmarked with full accuracy/latency tables. This is the version of the sentence framed as a "contribution," so it's worth being precise here specifically: a reader evaluating your thesis's claimed contribution should see the real scope of what was benchmarked.

---

## Optional — §2.2.1, "Support" actors not mentioned in the actor count (p.40)

This one's a clarification, not a correction, your original text isn't wrong, just slightly incomplete. Low priority, include only if you're already touching this paragraph for other reasons.

**Current text (roughly, System Actors intro):**
> Three actors interact with the platform: Customer, Administrator, and System.

**Optional addition:**
> Three actors interact with the platform: Customer, Administrator, and System. Individual use cases may additionally reference supporting external systems, such as the ML embedding sidecar, the payment gateway, or the OAuth provider, that participate in a use case without initiating it.

**Why:** several use case tables in §2.2.3–2.2.5 list a "Support" field naming external systems (ML Service, Payment Gateway, Email Service, Google OAuth) that aren't among the three primary actors. This is standard UML practice (secondary actors), not an error, but a one-sentence heads-up here means a reader won't be briefly confused when a "Support" row shows up two pages later in a use case table.

---

## What wasn't touched

Everything else in §2.1 and §2.2 held up well under review and needed no changes:
- §2.1.2 (Non-Functional Requirements) — internally consistent (JWT 15-minute expiry, inventory reservation timeout, rate limits all match their counterparts elsewhere in the thesis).
- §2.2.1–2.2.5 (System Actors through System Use Cases) — the "26 use cases" claim is exactly right (verified by direct count: 15 admin + 9 storefront + 2 system), and all 73 functional-requirement references inside the use case specs correctly resolve to real requirements, no orphan citations anywhere across roughly 50 pages of use case tables. That's genuinely careful work and didn't need touching.
- All 26 individual use case specifications (UC-ADM-* and UC-STR-* and UC-SYS-*) — sampled several in the original review, no internal contradictions found, no AI-writing flags.

---

Ready for §2.3 (System Architecture and Design) next whenever you want to continue.
