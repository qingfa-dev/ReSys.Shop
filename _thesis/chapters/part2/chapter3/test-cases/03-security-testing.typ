=== Security Compliance Checks

#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Scenario ID*], [*SC-003*],
    [*Scenario Name*], [*Administrative Access Control*],
    [*Source Code*], [`src/.../tests/api/unit/test_security.py`],
    [*Objective*], [Verify that standard users cannot access administrative functions.],
    [*Test Steps*],
    [
      1. Login as a Standard Customer.
      2. Attempt to access the Admin Dashboard URL.
      3. Attempt to call the 'Promote User' API.
    ],

    [*Expected Result*],
    [
      - Access Denied (HTTP 403 Forbidden).
      - User redirected to Home Page or Error Page.
    ],

    [*Actual Result*], [Request blocked by Role Guard. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [Evaluation Scenario SC-003: RBAC Enforcement],
  kind: table,
)
