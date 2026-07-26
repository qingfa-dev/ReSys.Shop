==== Shipping Method Configuration

// Diagram placeholder: Shipping Configuration use case diagram

*UC-ADM-SHP-01 — Manage shipping methods.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, activate, deactivate, or remove shipping methods. Configure carrier and applicable zones per method. \
*Postcondition:* Shipping method available for customer selection at checkout if active and zone-applicable. \
*Related FR:* SHP-FR-01, SHP-FR-04.

#v(0.5cm)
*UC-ADM-SHP-02 — Manage shipping rates.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, or remove shipping rates per method. Define rate tiers by weight, cart value, and geographic zone. \
*Postcondition:* Shipping rates applied during storefront checkout calculation for matching carts. \
*Related FR:* SHP-FR-02, SHP-FR-05.

==== Reference Data Management

// Diagram placeholder: Reference Data use case diagram

*UC-ADM-LOC-01 — Manage countries.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, or remove country records with ISO codes. Set active status to control availability. \
*Postcondition:* Country data updated. Active countries available in address forms and shipping zone configuration. \
*Related FR:* LOC-FR-01, LOC-FR-03.

#v(0.5cm)
*UC-ADM-LOC-02 — Manage states.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, or remove state records with ISO codes, linked to parent country. Set active status per state. \
*Postcondition:* State data updated. Active states available for address validation within their parent country. \
*Related FR:* LOC-FR-02, LOC-FR-04.
