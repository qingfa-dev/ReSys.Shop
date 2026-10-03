# Shared API Test Utilities

## Purpose

This directory contains shared configuration, authentication helpers, and
reusable snippets used across all module `.http` test files.

## Files

| File | Purpose |
|------|---------|
| `variables.http` | Global variables: `@baseUrl`, `@adminToken`, `@storeToken`, common GUIDs |
| `auth-helpers.http` | Reusable login/register/refresh requests for obtaining bearer tokens |

## Usage

1. **Set your base URL** — Edit `variables.http` or use an
   `http-client.env.json` file to override `@baseUrl`.

2. **Obtain tokens** — Run `auth-helpers.http` requests to populate
   `@adminToken` and `@storeToken`.

3. **Replace GUIDs** — The placeholder GUIDs in `variables.http` must be
   replaced with actual IDs from your seeded database.

### Environment File Example (`http-client.env.json`)

```json
{
  "dev": {
    "baseUrl": "http://localhost:5000",
    "adminToken": "",
    "storeToken": ""
  }
}
```

Place this file in the `ApiTests/` root or your user home directory.
See [VS Code REST Client docs](https://github.com/Huachao/vscode-restclient#environment-variables)
for details.
