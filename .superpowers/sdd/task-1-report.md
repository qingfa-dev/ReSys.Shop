# Task 1: Remove Direct Identity → Profile Reference in ConfirmEmail

## Status: ✓ Complete

## Changes

### 1. Created: `Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs`
- `CreateUserProfileCommand` record implementing `ICommand<CreateUserProfileResult>`
- `CreateUserProfileResult(Guid ProfileId)` response type

### 2. Modified: `Module/Profile/Features/Store/Profiles/Create/CreateProfile.cs`
- Added `CreateUserProfileCommandHandler` that wraps existing `CreateProfile.CommandHandler`
- Uses `Shared.Application.Contracts.Profile` import (avoided full-qualified name due to `Shared` namespace collision with `Module.Profile.Features.Store.Profiles.Shared`)

### 3. Modified: `Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs`
- Removed `using Module.Profile.Domain;` and `using Module.Profile.Features.Store.Profiles.Create;`
- Added `using Shared.Application.Contracts.Profile;`
- Replaced direct `CreateProfile.Command` call with `CreateUserProfileCommand` via `IMediator`
- Replaced `UserProfileLoggers.Management.*` with `UserLoggers.Profiles.*` (already in Shared)

## Verification

- **Build**: `dotnet build` — succeeds (0 warnings, 0 errors)
- **Architecture test**: `dotnet test --filter-class "ModuleIsolationTests"` — 3/4 pass, 1 failure is pre-existing `Catalog→Inventory` + `Ordering→Inventory` violations (unrelated)

## Notes

- `UserLoggers.Profiles` (in `Shared.Security.Identity.Domain.Users`) was used instead of `UserProfileLoggers.Management` (in `Module.Profile.Domain`) to avoid any direct module reference
- The `CreateUserProfileCommandHandler` delegates to the existing `CreateProfile.CommandHandler`, so no logic changes
- Error propagation uses implicit `List<Error>` → `Result<T>` conversion
