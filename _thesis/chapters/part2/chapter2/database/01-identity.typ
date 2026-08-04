=== Identity Context

The *Identity Context* serves as the security and profile management foundation for the entire e-commerce ecosystem. It is designed to be compliant with modern identity standards (ASP.NET Identity Core) while supporting extensibility for e-commerce specific requirements like address management and soft-banning.

==== User Aggregate
The `User` entity serves as the *Aggregate Root* for this context. Unlike simple credential stores, this entity acts as the central hub for customer customer-centric data.

- *Authentication:* It stores secure password hashes and security stamps to manage session validity (e.g., invalidating all tokens when a password changes).
- *Profile Management:* It segregates standard identity fields (Email, Phone) from e-commerce profile data (Avatar, Active Status).
- *Auditability:* It includes `ConcurrencyStamp` to strictly enforce optimistic concurrency control during profile updates.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key. Serves as the immutable reference for Orders and Carts.],
    [2], [UserName], [VARCHAR(256)], [Unique login identifier. Often mirrors Email but allows separation.],
    [3], [Email], [VARCHAR(256)], [Primary communication channel. Indexed for fast lookup.],
    [4], [FirstName], [VARCHAR(100)], [Customer's given name for personalization.],
    [5], [LastName], [VARCHAR(100)], [Customer's surname.],
    [6], [ProfileImagePath], [TEXT], [URI pointer to the user's avatar in object storage.],
    [7], [IsActive], [BOOL], [Soft-delete mechanism. Allows banning users without destroying relational integrity.],
    [8], [SecurityStamp], [VARCHAR], [Random value updated on credential changes to invalidate old cookies.],
    [9], [ConcurrencyStamp], [UUID], [Prevents lost updates during concurrent profile edits.],
    [10], [PasswordHash], [TEXT], [Argon2id or PBKDF2 hash of the user's password.],
    [11], [PhoneNumber], [VARCHAR(20)], [Optional contact number for SMS notifications.],
    [12], [TwoFactorEnabled], [BOOL], [Flag indicating if 2FA is required for login.],
    [13], [CreatedAt], [TIMESTAMP], [Account creation timestamp for cohort analysis.],
    [14], [LastSignInAt], [TIMESTAMP], [Last successful authentication timestamp.],
    [15], [SignInCount], [INT], [Total login count, used for engagement metrics.],
  ),
  caption: [Users table],
)

==== Logistics & Profile Data
To support the "Ship-to" and "Bill-to" requirements of e-commerce, the system extends the identity model with `UserAddresses`. This allows a single user to maintain a rolodex of locations (Home, Work, Gift Recipient).

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [UserId], [UUID], [Foreign Key linking to the User Aggregate.],
    [3], [Street], [VARCHAR(200)], [Primary address line.],
    [4], [City], [VARCHAR(100)], [City or municipality.],
    [5], [State], [VARCHAR(100)], [State, Province, or Region.],
    [6], [ZipCode], [VARCHAR(20)], [Postal or ZIP code for shipping calculation.],
    [7], [CountryCode], [VARCHAR(2)], [ISO 3166-1 alpha-2 country code (e.g. 'US', 'VN').],
    [8], [IsDefault], [BOOL], [Marker for pre-filling checkout forms.],
    [9], [Type], [INT], [Discriminator: 0=Shipping, 1=Billing.],
  ),
  caption: [UserAddresses table],
)

==== Authorization & RBAC
The system implements a *Role-Based Access Control (RBAC)* model supplemented by granular *Claims*.

- *Roles:* Define high-level groups like "Administrator" or "Customer".
- *Claims:* Define specific permissions like `catalog.manage` or `users.view`. This allows for fine-grained security policies where a "Content Manager" might update products but not delete users.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [Name], [VARCHAR(100)], [Human-readable role name (e.g. 'Administrator').],
    [3], [NormalizedName], [VARCHAR(100)], [Uppercase normalized name for consistent indexing.],
  ),
  caption: [Roles table],
)

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [UserId], [UUID], [Foreign Key to User.],
    [2], [RoleId], [UUID], [Foreign Key to Role.],
  ),
  caption: [UserRoles table (Many-to-Many Bridge)],
)

==== External Identity & Sessions
To support modern authentication flows (OAuth2, OIDC) and mobile apps, the schema includes support for external login providers and refresh tokens.

- *UserLogins:* Links local accounts to providers like Google or Facebook.
- *UserTokens:* Stores ephemeral tokens for API access or password reset flows.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [LoginProvider], [VARCHAR(100)], [The provider name (e.g. 'Google').],
    [2], [ProviderKey], [VARCHAR(100)], [The unique user ID from the provider system.],
    [3], [UserId], [UUID], [Foreign Key linking to the local account.],
  ),
  caption: [UserLogins table],
)

