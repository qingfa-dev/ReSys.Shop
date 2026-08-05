===== User and Role Administration: UC-ADM-USR, UC-ADM-ROL

*User management.* Paginated table: avatar, name, email, registration date, enabled toggle, roles (compact badges), last login. Filters: status, role, keyword search. Create/edit form: full name, email, password (create only), enabled toggle, role multi-select (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-user-list.png", width: 100%),
//   caption: [Users: table (50 per page) with avatar (32px initials fallback), Name, Email, Registered, Status toggle, Roles badges (Admin purple, Customer blue), Last Login. Toolbar: search, status/role filters, "Add User" button.],
// ) <fig-admin-user-list>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-user-edit.png", width: 100%),
//   caption: [User edit dialog: Full Name, Email (read-only), Enabled toggle, Roles multi-select checkboxes (Admin, Manager, Customer: Customer checked). Save/Cancel.],
// ) <fig-admin-user-edit>

*Role management.* Role table: name, description, user count, creation date. Expanding a role shows permission assignments in an expandable tree grouped by domain, each with `domain:category:action` checkbox toggles (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-role-list.png", width: 100%),
//   caption: [Roles: table (Admin: 12 users, Manager: 5, Support: 8, Customer: 1,200). Each: name, description, user count badge, Edit/Delete.],
// ) <fig-admin-role-list>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-role-permissions.png", width: 100%),
//   caption: [Role editor: left domain list (Catalog expanded, others collapsed). Right: Catalog permissions tree (Products: Create/Read/Update/Delete all checked; Variants: all checked; Images: Upload/Read/Delete checked; Taxonomies: Read/Update checked). "Save Permissions" button.],
// ) <fig-admin-role-permissions>

===== Shipping Configuration: UC-ADM-SHP

Shipping methods table: carrier name, delivery estimate, active toggle, rates count. Each method has a rates table per geographic zone and weight/value tier. Add-rate dialog: zone dropdown, weight range, value range, rate amount (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-shipping-methods.png", width: 100%),
//   caption: [Shipping methods: table (Standard 3-5 days, Express 1-2 days, Next-Day, International 7-14 days). Each: name, carrier, delivery estimate, active toggle, rates count badge.],
// ) <fig-admin-shipping-methods>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-shipping-rates.png", width: 100%),
//   caption: [Rates (Standard Delivery): table (Domestic: 0-2kg 30,000 VND, 2-5kg 50,000 VND; Southeast Asia: 0-2kg 150,000 VND). Each: zone, weight range, value range, rate, active toggle.],
// ) <fig-admin-shipping-rates>

===== Reference Data: UC-ADM-REF

Country table with ISO codes and active toggles. Selecting a country displays its states with ISO 3166-2 codes in a linked panel (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-reference-data.png", width: 100%),
//   caption: [Reference data: left country list (Vietnam, USA, Japan, Korea, Singapore with ISO codes, active toggles). Right: Vietnam's states (Ho Chi Minh City SG, Hanoi HN, Da Nang DN) with ISO 3166-2 codes.],
// ) <fig-admin-reference-data>
