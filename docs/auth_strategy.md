## Security Decisions For Zapas

### Decisions

- Zapas validates JWT bearer tokens issued by an external identity provider.
- Session endpoints require authenticated users.
- Upload endpoints require the `CanUploadSession` policy.
- Admin-only operations use the `CanDeleteSession` policy.
- Session reads must enforce owner access or admin access.
- FIT uploads have file size and file extension validation.
- Parser failures return controlled client errors.
- Secrets are stored outside source control.
- CORS allows only known frontend origins.

### Tradeoffs

- Zapas validates access tokens but does not implement a full identity provider.
- Roles are simple, but ownership checks require resource-based authorization or owner-filtered queries.
- File extension checks are useful but not sufficient on their own.
- Parser exception details are logged internally but not returned to clients.
