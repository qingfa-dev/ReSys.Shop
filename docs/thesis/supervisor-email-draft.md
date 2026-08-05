To: [Supervisor Name]
Subject: ReSys.Shop Thesis — Guidance needed on 3 security/compliance scope decisions

Dear [Supervisor Name],

I am finalizing the design documentation for my ReSys.Shop thesis and have completed the full draft of 11 chapters covering problem analysis, requirements, system architecture, domain analysis, database design, API design, detailed design, security, deployment, testing, and evaluation.

To ensure the scope aligns with examiner expectations, I need your guidance on three specific areas where I have intentionally deferred depth pending your advice:

---

### 1. Accessibility (WCAG) and GDPR — Should these appear as explicit NFRs?

**Context**: Chapter 2 (Requirements) currently documents 10 non-functional requirements covering modularity, error handling, build strictness, testability, observability, rate limiting, security headers, file upload security, caching, and background jobs.

**Question**: Should I add explicit accessibility (WCAG 2.1) and GDPR/privacy NFRs to Chapter 2? The system does have soft deletion (`IsDeleted` flags), user consent flows for cookies, and PII handling via ASP.NET Identity — but I have not formalized these as documented requirements.

**Options**:
- a) Add a brief §2.2.x on GDPR (data retention, right to erasure, lawful basis) and §2.2.y on accessibility (keyboard navigation, screen reader support) — approximately 1 page each.
- b) Mention GDPR and accessibility as out-of-scope constraints in §1.4 (Scope and Delimitations) with a sentence explaining that the thesis focuses on architecture rather than compliance.
- c) Ignore entirely unless the examiner specifically raises it.

**My preference**: (a) if the examiner values compliance awareness; (b) if the thesis is strictly architecture-focused.

---

### 2. Security Threat Model — How deep should Chapter 8 go?

**Context**: Chapter 8 (Security Design) currently contains a layered controls table covering authentication (JWT + rotation), authorization (permission-based), input validation (FluentValidation + anti-forgery), rate limiting, file upload guards, security headers, and secrets management. It also references the STRIDE categories informally.

**Question**: Should I add a formal STRIDE-per-element threat model (e.g., mapping Spoofing/Tampering/Repudiation/Info Disclosure/DoS/Elevation to the Order, PaymentIntent, and JWT Token elements)? This would add approximately 2–3 pages.

**Options**:
- a) Add a formal STRIDE table for the 3 most critical elements (Order, PaymentIntent, JWT) — adds ~2 pages, demonstrates structured security thinking.
- b) Keep the current layered controls approach ( Chapter 8 as-is) — sufficient for a SE thesis where security is a supporting concern, not the primary contribution.
- c) Add a lightweight STRIDE summary (½ page) without the full element-by-element breakdown.

**My preference**: (b) for draft, (a) only if you think the examiner is security-focused.

---

### 3. GDPR Privacy-by-Design — Data retention and right to erasure documentation?

**Context**: The system uses soft deletion (`IsDeleted`, `DeletedAtUtc`, `DeletedBy`) on all business entities rather than hard deletion. User profiles, addresses, and order history are retained even after a user "deletes" their account. This is a deliberate design choice for audit trails in an e-commerce context.

**Question**: Should I document the GDPR implications of this design? Specifically:
- Data retention periods (e.g., orders retained for 7 years for tax purposes; PII anonymized after account deletion)
- Lawful basis for processing (contract for orders, consent for marketing)
- Right to erasure — explain why soft deletion is used and under what conditions hard erasure would occur

**Options**:
- a) Add a 1-page "Privacy Impact Assessment" subsection in Chapter 8 (Security Design) explaining the soft-deletion rationale, retention policy, and erasure procedure.
- b) Add a brief paragraph in §1.4 (Scope) noting that full GDPR compliance is deferred as an operational/legal concern outside the architectural scope.
- c) Omit entirely — the thesis contribution is software architecture, not legal compliance.

**My preference**: (a) if the examiner is interdisciplinary (e.g., includes a law/ethics panel member); (b) or (c) otherwise.

---

### 4. User Study / Usability Evaluation — Is this expected?

**Context**: The thesis contribution is architectural: modular monolith, vertical slices, explicit error handling (`Result<T>`), and CBIR integration. The frontends (Vue 3 Admin + Storefront) exist as proof-of-concept API clients.

**Question**: Should I include any form of user evaluation (e.g., System Usability Scale questionnaire with 5–10 participants, or task-based testing for the checkout flow)?

**Options**:
- a) Skip user evaluation entirely — the contribution is architectural, not HCI.
- b) Add a lightweight SUS appendix (1 page) with 5 volunteer participants — minimal effort, signals awareness of usability.
- c) Add a full usability section in Chapter 11 (Evaluation) with task-based metrics — significant scope expansion.

**My preference**: (a) unless the examiner has explicitly signaled interest in UX.

---

### What I need from you

Could you indicate your preference (a / b / c) for each of the four questions above? If any of these are mandatory for passing the thesis in your view, please let me know and I will prioritize them immediately.

All other thesis documentation decisions have been resolved based on standard SE thesis conventions (IEEE 830, ISO 42010, C4 Model). I am currently at the draft stage and plan to run benchmarks (test coverage, ML metrics) before final submission.

Thank you for your guidance.

Best regards,
[Your Name]
