===== User and Role Administration: UC-ADM-USR, UC-ADM-ROL

*User management.* Paginated table: avatar, name, email, registration date, enabled toggle, roles (compact badges), last login. Filters: status, role, keyword search. Create/edit form: full name, email, password (create only), enabled toggle, role multi-select (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-user-list.png", width: 100%),
  caption: [User list with status toggle and roles; search and filter toolbar.],
) <fig-admin-user-list>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-user-edit.png", width: 100%),
  caption: [User edit form: name, email, enabled toggle, role checkboxes.],
) <fig-admin-user-edit>

*Role management.* Role table: name, description, user count, creation date. Expanding a role shows permission assignments in an expandable tree grouped by domain, each with #emph[domain.category.resource.action] checkbox toggles (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-role-list.png", width: 100%),
  caption: [Roles: table with name, description, user count badge, Edit/Delete.],
) <fig-admin-role-list>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-role-permissions.png", width: 100%),
  caption: [Role editor: permissions tree with domain.category.resource.action toggles.],
) <fig-admin-role-permissions>

===== Shipping Configuration: UC-ADM-SHP

Shipping methods table: carrier name, delivery estimate, active toggle, rates count. Each method has a rates table per geographic zone and weight/value tier. Add-rate dialog: zone dropdown, weight range, value range, rate amount (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-shipping-methods.png", width: 100%),
  caption: [Shipping methods: carrier, delivery estimate, active toggle, rates count.],
) <fig-admin-shipping-methods>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-shipping-rates.png", width: 100%),
  caption: [Rates: table with zone, weight range, value range, rate, active toggle.],
) <fig-admin-shipping-rates>
