# ReSys.Shop API Tests

HTTP API test files for the ReSys.Shop backend, organized by module and
concern (Admin vs Store/Storefront). Compatible with the
[VS Code REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
and [JetBrains HTTP Client](https://www.jetbrains.com/help/idea/http-client-in-product-code-editor.html).

## Structure

```
ApiTests/
  _shared/                  # Shared variables and auth helpers
    variables.http          # Base URL, tokens, common IDs
    auth-helpers.http       # Login/register/refresh request snippets
    README.md
  Identity/
    Admin/                  # 7 .http files (users, roles, permissions, auth)
    Store/                  # 5 .http files (register, login, sessions, emails, passwords)
  Profile/
    Admin/                  # 1 .http file (profiles)
    Store/                  # 5 .http files (profiles, addresses, wishlists, notifications, preferences)
  Location/
    Admin/                  # 2 .http files (countries, states)
    Store/                  # 2 .http files (countries, states)
  Catalog/
    Admin/                  # 12 .http files (option types, taxonomies, products, variants, images, etc.)
    Storefront/             # 7 .http files (products, search, collections, taxons, digitals, image search)
  README.md                 # This file
```

**Total: 41 endpoint files covering all 4 modules.**

## Getting Started

1. **Start the API** — Run the ReSys.Shop API on `http://localhost:5000`
   (or configure `@baseUrl` in `_shared/variables.http`).

2. **Seed the database** — Ensure test data exists (countries, users,
   products, etc.).

3. **Configure variables** — Update the placeholder GUIDs in
   `_shared/variables.http` to match your seeded IDs.

4. **Obtain auth tokens** — Run the login requests in
   `_shared/auth-helpers.http` to set `@adminToken` and `@storeToken`.

5. **Run individual tests** — Open any `.http` file and click
   "Send Request" above each `###` block.

6. **Run all tests** — Open `run-all.http` and send requests sequentially.

## Conventions

- `###` separates individual request scenarios
- `@name` labels a request for response extraction
- `{{variable}}` references shared variables
- Comments above each scenario describe the expected status code
- Every file includes success, error (400/404/409), and auth (401) scenarios

## Environment Configuration

Create an `http-client.env.json` in this directory:

```json
{
  "dev": {
    "baseUrl": "http://localhost:5000",
    "adminToken": "",
    "storeToken": ""
  }
}
```

Or use `http-client.private.env.json` for secrets (git-ignored by convention).
