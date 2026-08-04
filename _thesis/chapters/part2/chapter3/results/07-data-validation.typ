=== Data Validation Assessment

To ensure system robustness and data integrity, a series of *Negative Test Cases* were executed to verify that the system correctly rejects invalid inputs and enforces the non-functional constraints defined in Chapter 2.

#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Scenario ID*], [*TC-004*],
    [*Scenario Name*], [*Data Validation & Integrity Enforcement*],
    [*Objective*], [Verify that the system gracefully handles and rejects invalid data inputs.],

    [*Test Steps*],
    [
      1. Upload a 15MB Image File (Limit: 10MB).
      2. Upload a `.exe` file as Product Image.
      3. Create a Product with an existing SKU.
    ],

    [*Expected Result*],
    [
      1. HTTP 413 Payload Too Large.
      2. HTTP 415 Unsupported Media Type.
      3. HTTP 409 Conflict (Duplicate SKU).
    ],

    [*Actual Result*],
    [
      1. Rejected (413). Client receives 'File too large' error.
      2. Rejected (400). Validator: 'Invalid file extension'.
      3. Rejected (409). Database constraint violation caught.
    ],

    [*Status*], [*PASS*],
  ),
  caption: [Evaluation Scenario TC-004: Negative Testing for Data Constraints.],
  kind: table,
) <tbl:data-validation>

The successful execution of TC-004 confirms that the *Application Layer* (FluentValidation) and *Database Layer* (Unique Constraints) are correctly aligned to prevent data corruption and denial-of-service vectors.
