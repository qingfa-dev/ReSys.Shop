# Thesis Rewrite — Part 2, Chapter 2, Section 2.3

Three real fixes in this section, all small and localized: a one-word contradiction, two broken cross-references, and a permission-format mismatch between a template and its own examples.

All edits trace back to `thesis-review-part2-chapter2-section3.md` and the master fix list.

---

## Edit 1 — §2.3.1, "nine" contradicts "eight" three lines earlier (p.77)

**BEFORE:**
> ReSys.Shop comprises three services, a Vue 3 frontend, a .NET 10 backend [19], and a Python FastAPI ML sidecar [23], and eight bounded contexts using Domain-Driven Design with MediatR dispatch between modules.
>
> [Table 46]
>
> Internally, the backend is **partitioned into nine bounded contexts**, each owning a dedicated database schema. Table 47 lists each context, its aggregate root, and key domain entities.

**AFTER:**
> ReSys.Shop comprises three services, a Vue 3 frontend, a .NET 10 backend [19], and a Python FastAPI ML sidecar [23], and eight bounded contexts using Domain-Driven Design with MediatR dispatch between modules.
>
> [Table 46]
>
> Internally, the backend is **partitioned into eight bounded contexts**, each owning a dedicated database schema. Table 47 lists each context, its aggregate root, and key domain entities.

**Why:** the sentence three lines above already says "eight," Table 47 immediately below lists exactly 8 rows (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location), and §2.3.2 repeats "eight" three more times. "Nine" here is a one-word slip surrounded on all sides by the correct number, easy fix, but worth catching since it's the kind of thing a reader notices within the same paragraph.

---

## Edit 2 — §2.3.4.3, phantom "Section 2.1.5" reference (p.89)

**BEFORE:**
> The platform defaults to HNSW indexing [12] using cosine distance to meet the sub-second CBIR latency target (NFR-01a), with IVFFlat as a fallback for local environments **(see Section 2.1.5 for index detail)**.

**AFTER:**
> The platform defaults to HNSW indexing [12] using cosine distance to meet the sub-second CBIR latency target (NFR-01a), with IVFFlat as a fallback for local environments **(see §1.4.3–1.4.4 for the HNSW/IVFFlat algorithm comparison)**.

**Why:** Section 2.1 only goes up to §2.1.3, there's no 2.1.4 or 2.1.5 anywhere in the thesis. The content this was almost certainly meant to point to, the HNSW and IVFFlat explanations, actually lives in §1.4.3 and §1.4.4.

---

## Edit 3 — §2.4.3.2, second phantom "Section 2.1.5" reference (p.101)

This one technically sits in §2.4 rather than §2.3, but it's the same broken reference caught during the §2.3 review, so it's included here for completeness.

**BEFORE:**
> The production HNSW index uses cosine distance for sub-second CBIR queries **(see Section 2.3.4 for index detail and Section 2.1.5 for ANN algorithm comparison)**.

**AFTER:**
> The production HNSW index uses cosine distance for sub-second CBIR queries **(see §2.3.4 for index detail and §1.4.3–1.4.4 for the ANN algorithm comparison)**.

**Why:** same fix as Edit 2, applied to the second occurrence.

---

## Edit 4 — §2.3.6.2, permission format template doesn't match its own examples (p.93)

**BEFORE:**
> Operations enforce granular resource-action claims. Ten FeatureMetadata files define the permission registry across modules, each mapping to the PermissionContext static catalogue. Permissions use the format **Domain.Category.Resource.Action**:
> ```
> catalog.products.create
> catalog.products.update
> catalog.variants.delete
> identity.roles.manage
> ordering.orders.approve
> payment.intents.capture
> inventory.stock.transfer
> ```

**AFTER:**
> Operations enforce granular resource-action claims. Ten FeatureMetadata files define the permission registry across modules, each mapping to the PermissionContext static catalogue. Permissions use the format **domain.resource.action**:
> ```
> catalog.products.create
> catalog.products.update
> catalog.variants.delete
> identity.roles.manage
> ordering.orders.approve
> payment.intents.capture
> inventory.stock.transfer
> ```

**Why:** the stated template names four segments (Domain, Category, Resource, Action), but every one of the seven examples immediately below it is a three-segment string (e.g., `catalog.products.create` = domain.resource.action). The template should describe what the examples actually show.

**A broader cleanup this points to:** five other places in the thesis describe this same permission system using a colon-separated, three-part format instead, `domain:category:action` (Chapter 1 §1.5.4; Chapter 2 NFR-02b in §2.1.2; §2.2.1's actor description; §2.3.2's DDD section; and §2.4.5's admin UI description). Since the actual code examples here use dots, not colons, and three segments, not four, it's worth a global find-and-replace changing all five `domain:category:action` mentions to `domain.resource.action` so the whole thesis describes the permission format the same way. That's a five-location, cross-chapter cleanup rather than a single edit, so it's flagged here rather than rewritten line-by-line.

---

## Note — PostgreSQL version (no edit needed in this section)

§2.3.1 (Table 46), §2.3.3.2, and §2.3.4 all correctly say **PostgreSQL 17**, consistent with seven other mentions across the thesis. Nothing to fix here, this section is right. The one outlier is Chapter 3's Table 66, which says "PostgreSQL 16," that's the one that needs correcting, not anything in §2.3. Flagging it here just so it's not missed when you get to the Chapter 3 rewrite.

---

## What wasn't touched

§2.3.2 (Domain-Driven Design), §2.3.3 (C4 Architecture, aside from Edit 3's cross-reference), §2.3.5 (API Design), and the rest of §2.3.6 (Security Design) all held up under review. The 262-endpoint count and 11 inter-module DTO count were both verified as exact sums from their respective tables, no changes needed there.

---

Ready for §2.4 (Implementation) next whenever you want to continue.
