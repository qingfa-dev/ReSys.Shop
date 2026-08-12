# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.x     | :white_check_mark: (pre-release) |

## Reporting a Vulnerability

ReSys.Shop is currently in pre-release development. If you discover a security vulnerability, please report it via email to the maintainers rather than opening a public issue.

**Disclosure process:**
1. Email the details to the project maintainers
2. Include a description of the vulnerability, steps to reproduce, and affected versions
3. Allow reasonable time for the issue to be addressed before public disclosure
4. The maintainers will acknowledge receipt within 5 business days and provide an estimated timeline

## Security Update Process

Security patches are released as part of the regular release cycle. Critical vulnerabilities may warrant an out-of-cycle patch.

## Known Security Considerations

- JWT secrets for non-Development environments must be provisioned via user-secrets or environment variables. The `appsettings.Development.json` file contains development-only keys.
- CORS allows credentials in the Development environment only. Ensure production CORS is locked down to the deployed origin.
- File upload malware scanning is disabled by default (`MalwareScanner.Enabled = false`). Enable ClamAV integration in production environments.
- Anti-forgery protection is enabled (`AntiForgery.IsEnabled = true` with `X-CSRF-TOKEN` header). Ensure it remains enabled in production.
- Rate limiting is configured for auth endpoints only. Consider adding policies for cart operations and API search endpoints before production deployment.
- The `.env.template` files contain `REPLACE_ME_*` placeholders — never commit real secrets to this repository.
