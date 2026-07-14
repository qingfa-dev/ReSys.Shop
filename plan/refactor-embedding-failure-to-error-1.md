---
goal: Rename Failure → Error in embedding service Result pattern to match .NET Shared.Application.Models
version: 1.0
date_created: 2026-07-14
owner: ReSys.Shop Platform
status: Planned
tags: [refactor, embedding, naming, dotnet-alignment]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The embedding service's `Result` pattern uses `Failure`/`FailureType` names inherited from the sidecar's `BuildingBlocks` pattern. The current .NET codebase (`service/Api/src/Shared/Application/Models/`) uses `Error`/`ErrorType` instead. Rename all Python occurrences to match, reducing cognitive overhead when crossing the .NET/Python boundary.

---

## 1. Requirements & Constraints

- **REQ-001**: `Failure` class → `Error` (matching .NET `readonly partial struct Error`)
- **REQ-002**: `FailureType` enum → `ErrorType` (matching .NET `ErrorType` constant class)
- **REQ-003**: `Result.failures` field → `Result.errors` (matching .NET `List<Error> Errors`)
- **REQ-004**: `Result.failure()` and `ValueResult.failure_value()` factory method names stay as-is (matching .NET `Result.Failure(Error)` method name pattern)
- **REQ-005**: Parameter names in factory methods change from `failure` to `error`
- **REQ-006**: `Failure.bad_request()` / `.not_found()` etc. → `Error.bad_request()` / `.not_found()`
- **REQ-007**: File `failure.py` → `error.py`
- **REQ-008**: No behavior change — only naming; all 129 tests must still pass
- **REQ-009**: `uv run ruff check .` must pass with zero errors
- **CON-001**: The `*.Errors` namespace classes (`InferenceResults.Errors`, `ImageResults.Errors`, `RegistryResults.Errors`) are NOT renamed — they already match .NET naming and are pure factory namespaces
- **CON-002**: The `FailureType.None_` value needs careful handling during rename (trailing underscore)
- **CON-003**: `pyproject.toml` `tool.setuptools.packages` entry `embedding.schemas.results` does not need updating (the package dir stays `results/`)

---

## 2. Implementation Steps

### Implementation Phase 1: Rename source definition

- GOAL-001: Rename `Failure` → `Error` and `FailureType` → `ErrorType` in the definition file

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename `failure.py` → `error.py`; rename all class/enum names, factory classmethods, field names | |  |
| TASK-002 | Update `results/result.py` imports, type hints, field name `failures` → `errors`, parameter names | |  |
| TASK-003 | Update `results/__init__.py` re-exports | |  |

**TASK-001**: Rename `failure.py` → `error.py` with class renames

Files:
- Rename: `service/Embedding/src/schemas/results/failure.py` → `service/Embedding/src/schemas/results/error.py`

```bash
mv service/Embedding/src/schemas/results/failure.py service/Embedding/src/schemas/results/error.py
```

Edit `service/Embedding/src/schemas/results/error.py`:
```python
# Rename FailureType → ErrorType
class ErrorType(IntEnum):
    None_ = 0
    Validation = 1
    Conflict = 2
    NotFound = 3
    BadRequest = 4
    InternalError = 5
    Unauthorized = 6
    Forbidden = 7
    Unexpected = 8

# Rename Failure → Error
class Error(BaseModel):
    # field 'type: FailureType' → 'type: ErrorType'
    type: ErrorType = Field(...)
    code: str = Field(...)
    description: str = Field(...)
    status_code: int = Field(default=400, ...)

    # All classmethod names stay the same, but return type annotation changes from "Failure" to "Error"
    @classmethod
    def validation(cls, code: str, description: str) -> "Error": ...   # was "Failure"
    @classmethod
    def conflict(cls, code: str, description: str) -> "Error": ...
    @classmethod
    def not_found(cls, code: str, description: str) -> "Error": ...
    @classmethod
    def bad_request(cls, code: str, description: str) -> "Error": ...
    @classmethod
    def internal_error(cls, code: str, description: str) -> "Error": ...
    @classmethod
    def unauthorized(cls, code: str, description: str) -> "Error": ...
    @classmethod
    def forbidden(cls, code: str, description: str) -> "Error": ...
```

Validation:
```bash
cd service/Embedding && uv run ruff check --fix src/schemas/results/error.py
```

**TASK-002**: Update `result.py`

Edit `service/Embedding/src/schemas/results/result.py`:

```python
# Changed import
from embedding.schemas.results.error import Error  # was "from ...failure import Failure"

class Result(BaseModel):
    # Changed field name and type
    errors: List[Error] = Field(default=[], ...)  # was "failures: List[Failure]"

    @classmethod
    def ok(cls, status_code: int = 200, message: Optional[str] = None) -> "Result":
        return cls(isSuccess=True, statusCode=status_code, message=message, errors=[])

    @classmethod
    def failure(cls, error: Union[Error, List[Error]], message: Optional[str] = None) -> "Result":
        # Parameter renamed from "failure" to "error"
        errors = [error] if isinstance(error, Error) else error  # was "if isinstance(failure, Failure)"
        sc = errors[0].status_code if errors else 400  # was "failures[0].status_code"
        return cls(isSuccess=False, statusCode=sc, message=message, errors=errors)

class ValueResult(Result, Generic[T]):
    @classmethod
    def ok_value(cls, value: T, status_code: int = 200, message: Optional[str] = None) -> "ValueResult[T]":
        return cls(isSuccess=True, statusCode=status_code, message=message, errors=[], value=value)

    @classmethod
    def failure_value(cls, error: Union[Error, List[Error]], message: Optional[str] = None) -> "ValueResult[T]":
        errors = [error] if isinstance(error, Error) else error
        sc = errors[0].status_code if errors else 400
        return cls(isSuccess=False, statusCode=sc, message=message, errors=errors, value=None)
```

**TASK-003**: Update `results/__init__.py`

Edit `service/Embedding/src/schemas/results/__init__.py`:

```python
from embedding.schemas.results.error import Error, ErrorType  # was "from ...failure import Failure, FailureType"
from embedding.schemas.results.result import Result, ValueResult

__all__ = [
    "Error",      # was "Failure"
    "ErrorType",  # was "FailureType"
    "Result",
    "ValueResult",
]
```

Commit:
```bash
git add service/Embedding/src/schemas/results/
git commit -m "refactor(embedding): rename Failure/ FailureType to Error/ ErrorType in result module"
```

### Implementation Phase 2: Update schema namespace factories

- GOAL-002: Update all `schemas/inferences/`, `schemas/images/`, `schemas/registries/`, and `schemas/__init__.py` imports

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Update `schemas/inferences/__init__.py` imports and `Failure.*` → `Error.*` calls | |  |
| TASK-005 | Update `schemas/images/__init__.py` imports and `Failure.*` → `Error.*` calls | |  |
| TASK-006 | Update `schemas/registries/__init__.py` imports and `Failure.*` → `Error.*` calls | |  |
| TASK-007 | Update `schemas/__init__.py` exports (add `Error`, `ErrorType`; remove `Failure`, `FailureType`) | |  |

**TASK-004**: Update `inferences/__init__.py`

Edit `service/Embedding/src/schemas/inferences/__init__.py`:

```python
# Changed import
from embedding.schemas.results.error import Error  # was "from ...failure import Failure"

class InferenceResults:
    class Errors:
        @staticmethod
        def ModelNotFound(model_name: str) -> Error:  # was "-> Failure"
            return Error.not_found("Model.NotFound", ...)  # was "Failure.not_found"

        @staticmethod
        def OnnxNotFound(path_or_message: str) -> Error:
            return Error.not_found("Model.NotFound", ...)

        @staticmethod
        def LoadError(model_name: str, detail: str) -> Error:
            return Error.internal_error("Model.LoadError", ...)

        @staticmethod
        def InferenceFailed(model_name: str, detail: str) -> Error:
            return Error.internal_error("Inference.Error", ...)

        @staticmethod
        def DeviceError(model_name: str, device: str, detail: str) -> Error:
            return Error.internal_error("Inference.DeviceError", ...)
```

**TASK-005**: Update `images/__init__.py`

Edit `service/Embedding/src/schemas/images/__init__.py`:

```python
from embedding.schemas.results.error import Error

class ImageResults:
    class Errors:
        @staticmethod
        def LoadError(detail: str) -> Error:
            return Error.bad_request("Image.LoadError", detail)

        @staticmethod
        def UnsupportedType(type_name: str) -> Error:
            return Error.bad_request("Image.InputError", ...)
```

**TASK-006**: Update `registries/__init__.py`

Edit `service/Embedding/src/schemas/registries/__init__.py`:

```python
from embedding.schemas.results.error import Error

class RegistryResults:
    class Errors:
        @staticmethod
        def NotRegistered(skill_name: str) -> Error:
            return Error.internal_error("Registry.Error", ...)
```

**TASK-007**: Update `schemas/__init__.py`

Edit `service/Embedding/src/schemas/__init__.py`:

```python
# Changed imports
from embedding.schemas.results import Error, ErrorType, Result, ValueResult  # was "Failure, FailureType"

__all__ = [
    "Result",
    "ValueResult",
    "Error",       # was "Failure"
    "ErrorType",   # was "FailureType"
    "InferenceResults",
    "ImageResults",
    "RegistryResults",
    "EmbeddingRequest",
    "EmbeddingResponse",
    "ModelMetadata",
]
```

Commit:
```bash
git add service/Embedding/src/schemas/
git commit -m "refactor(embedding): update all schema imports from Failure to Error"
```

### Implementation Phase 3: Update consumers

- GOAL-003: Update all non-schema files that import/reference `Failure` or `FailureType`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Update `api/middleware/exception_handlers.py` | |  |
| TASK-009 | Search all remaining source files for `Failure` references | |  |
| TASK-010 | Run `ruff check --fix` across entire `src/` | |  |

**TASK-008**: Update `exception_handlers.py`

Edit `service/Embedding/src/api/middleware/exception_handlers.py`:

```python
# Changed import
from embedding.schemas import Error, ErrorType, Result  # was "Failure, FailureType, Result"

async def global_exception_handler(request, exc):
    failure = Error.internal_error(...)  # was "Failure.internal_error"
    return create_error_response(Result.failure(failure))

async def http_exception_handler(request, exc):
    if exc.status_code == 404:
        ftype = ErrorType.NotFound  # was "FailureType.NotFound"
    elif exc.status_code == 401:
        ftype = ErrorType.Unauthorized
    elif exc.status_code == 403:
        ftype = ErrorType.Forbidden
    else:
        ftype = ErrorType.Unexpected

    failure = Error(  # was "Failure("
        type=ftype,
        code=code,
        description=str(exc.detail),
        status_code=exc.status_code
    )
    return create_error_response(Result.failure(failure))

async def validation_exception_handler(request, exc):
    failures = []
    for error in exc.errors():
        loc = ".".join(str(x) for x in error["loc"])
        msg = error["msg"]
        failures.append(Error.validation(...))  # was "Failure.validation"
    return create_error_response(Result.failure(failures))
```

**TASK-009**: Verify no remaining `Failure` references

```bash
cd service/Embedding
grep -rn "Failure" src/ --include="*.py" || echo "OK: No remaining Failure references"
grep -rn "from.*failure" src/ --include="*.py" || echo "OK: No imports from failure module"
```

**TASK-010**: Auto-fix any remaining import/lint issues

```bash
cd service/Embedding && uv run ruff check --fix src/
cd service/Embedding && uv run ruff check src/
# Expected: 0 errors
```

Commit:
```bash
git add service/Embedding/src/api/ service/Embedding/src/models/ service/Embedding/src/services/
git commit -m "refactor(embedding): update all consumers from Failure to Error"
```

### Implementation Phase 4: Update test files

- GOAL-004: Update all test files that reference `Failure`/`FailureType`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Search test files for `Failure`/`failure` references and update | |  |
| TASK-012 | Run tests and fix any failures | |  |

**TASK-011**: Update test files

```bash
cd service/Embedding
# Find all test files mentioning Failure
grep -rn "Failure" tests/ --include="*.py"
grep -rn "failure" tests/ --include="*.py"

# For each match:
# - `from embedding.schemas.results.failure import Failure, FailureType` → `from embedding.schemas.results.error import Error, ErrorType`
# - `from embedding.schemas import Failure` → `from embedding.schemas import Error`
# - `Failure(...)` → `Error(...)`
# - `FailureType.` → `ErrorType.`
# - `.failures` → `.errors` (only on Result/ValueResult instances)
# - `isinstance(failure, Failure)` → `isinstance(error, Error)`
# - `Result.failure(failure)` → `Result.failure(error)` (parameter name only)
# - `ValueResult.failure_value(failure)` → `ValueResult.failure_value(error)`
```

Run the mechanical rename:
```bash
cd service/Embedding

# Replace import module paths
find tests/ -name "*.py" -exec sed -i 's/from embedding\.schemas\.results\.failure import/from embedding.schemas.results.error import/g' {} +
find tests/ -name "*.py" -exec sed -i 's/import embedding\.schemas\.results\.failure/import embedding.schemas.results.error/g' {} +

# Replace class references (not method names)
find tests/ -name "*.py" -exec sed -i 's/\bFailureType\b/ErrorType/g' {} +
find tests/ -name "*.py" -exec sed -i 's/\bclass Failure\b/class Error/g' {} +
find tests/ -name "*.py" -exec sed -i 's/\bFailure(\(/Error(\(/g' {} +
find tests/ -name "*.py" -exec sed -i 's/\bisinstance(\(.*\), Failure)/isinstance(\1, Error)/g' {} +

# Replace field accesses (.failures → .errors)
find tests/ -name "*.py" -exec sed -i 's/\.failures\b/.errors/g' {} +

# Replace parameter names (failure → error in function signatures)
find tests/ -name "*.py" -exec sed -i 's/\bfailure:\s*Union\[Error/error: Union[Error/g' {} +
find tests/ -name "*.py" -exec sed -i 's/=\s*failure\b/= error/g' {} +
```

**TASK-012**: Verify tests pass

```bash
cd service/Embedding && uv run pytest
# Expected: 129/129 passing (same as before rename)
```

If failures, fix each one manually. Common issues:
- Import path still referencing old `failure.py` module
- `Failure` still appearing in string annotations (`-> "Failure"`)
- `.failures` still appearing in assertions
- `FailureType` still used in comparison assertions

Commit:
```bash
git add service/Embedding/tests/
git commit -m "refactor(embedding): update all test Failure references to Error"
```

### Implementation Phase 5: Verification

- GOAL-005: Run full lint + test suite, verify name consistency

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Run ruff check across entire `service/Embedding/` | |  |
| TASK-014 | Run full test suite | |  |
| TASK-015 | Verify no remaining `Failure` type references (except method names `failure()`, `failure_value()`) | |  |

**TASK-013**: Ruff check
```bash
cd service/Embedding && uv run ruff check .
# Expected: 0 errors
```

**TASK-014**: Test suite
```bash
cd service/Embedding && uv run pytest -v
# Expected: 129/129 passing
```

**TASK-015**: Final grep for leftovers
```bash
cd service/Embedding

# These should have ZERO matches (the type is completely renamed):
echo "=== Checking for class/type references ==="
grep -rn "class Failure" src/ --include="*.py" || echo "OK: No Failure class"
grep -rn "class FailureType" src/ --include="*.py" || echo "OK: No FailureType class"
grep -rn ": Failure" src/ --include="*.py" || echo "OK: No ': Failure' type hints"
grep -rn "import Failure" src/ --include="*.py" || echo "OK: No Failure imports"
grep -rn "import FailureType" src/ --include="*.py" || echo "OK: No FailureType imports"

# These should have matches (the method names are preserved):
echo "=== Checking for method references ==="
grep -rn "Result\.failure\|ValueResult\.failure_value" src/ --include="*.py"
# Expected: matches in result.py, exception_handlers.py, etc.

# Test files should also be clean:
grep -rn "class Failure" tests/ --include="*.py" || echo "OK: No Failure class in tests"
grep -rn "import Failure" tests/ --include="*.py" || echo "OK: No Failure imports in tests"
```

Final commit if verification passes:
```bash
git add -A && git commit -m "chore(embedding): final verification — no remaining Failure type references"
```

---

## 3. Alternatives

- **ALT-001**: Keep `Failure` name and only rename the `failures` field to `errors` — rejected because the type name inconsistency would be confusing (`errors: List[Failure]`)
- **ALT-002**: Rename `Result.failure()` factory to `Result.error()` too — rejected because .NET uses `Result.Failure(Error)` with the same pattern (method name stays "Failure", parameter type is `Error`)
- **ALT-003**: Keep both `Failure` and `Error` as aliases — rejected as YAGNI; creates two names for the same concept
- **ALT-004**: Use `FailureType` enum name without renaming — rejected because it must match `ErrorType` from .NET `Shared.Application.Models.Errors.ErrorType`

---

## 4. Dependencies

- **DEP-001**: `service/Embedding/` must have passed `ruff check .` before starting (no pre-existing lint issues)
- **DEP-002**: `service/Embedding/` tests must be passing (129/129 baseline)
- **DEP-003**: All impacted files must be on the same branch

---

## 5. Files

| File | Action | Details |
|------|--------|---------|
| `service/Embedding/src/schemas/results/failure.py` | Rename | → `error.py` |
| `service/Embedding/src/schemas/results/error.py` | Modify | Rename `FailureType` → `ErrorType`, `Failure` → `Error` |
| `service/Embedding/src/schemas/results/result.py` | Modify | `failures` → `errors` field, `Failure` → `Error` imports/type hints |
| `service/Embedding/src/schemas/results/__init__.py` | Modify | Exports `Error`/`ErrorType` instead of `Failure`/`FailureType` |
| `service/Embedding/src/schemas/__init__.py` | Modify | Same export rename |
| `service/Embedding/src/schemas/inferences/__init__.py` | Modify | `Failure.*` → `Error.*` in factory methods |
| `service/Embedding/src/schemas/images/__init__.py` | Modify | Same |
| `service/Embedding/src/schemas/registries/__init__.py` | Modify | Same |
| `service/Embedding/src/api/middleware/exception_handlers.py` | Modify | `Failure`/`FailureType` → `Error`/`ErrorType` |
| `service/Embedding/tests/` | Modify | All test files referencing `Failure`/`FailureType` |

---

## 6. Testing

- **TEST-001**: All existing 129 tests must pass with zero changes to test logic (only import/type-name changes)
- **TEST-002**: `uv run ruff check .` must report zero errors
- **TEST-003**: `grep -rn "Failure" src/ --include="*.py"` must only match the `Result.failure()` and `ValueResult.failure_value()` method names (not the class name)
- **TEST-004**: `grep -rn "class Failure" src/ --include="*.py"` must return zero matches
- **TEST-005**: `grep -rn "import Failure" src/ --include="*.py"` must return zero matches

---

## 7. Risks & Assumptions

- **RISK-001**: Tests that assert on JSON serialization key names (`.failures` → `.errors`) will fail — must be manually checked for `alias` or `serialization_alias` on the field
- **RISK-002**: The `Result.failure()` and `ValueResult.failure_value()` method names have `failure` in them — sed commands for `Failure`→`Error` must NOT match these method names (they should be preserved)
- **RISK-003**: String annotations (`-> "Failure"`) will not be caught by `sed` `\b` word boundaries in some cases — manual verification needed
- **ASSUMPTION-001**: The `.NET `Result.Failure(Error)` pattern already uses "Failure" as the method name, so keeping `Result.failure()` in Python is consistent
- **ASSUMPTION-002**: No external code references the Python `Failure` or `FailureType` by name (consumers use `InferenceResults.Errors.*` which return `Failure` objects but don't reference the class type directly)

---

## 8. Related Specifications / Further Reading

- `service/Api/src/Shared/Application/Models/Results/Result.cs` — .NET `Result` pattern with `Error` type
- `service/Api/src/Shared/Application/Models/Errors/Error.cs` — .NET `Error` struct definition
- `service/Api/src/Shared/Application/Models/Errors/Error.Type.cs` — .NET `ErrorType` constants
- `service/Embedding/src/schemas/results/result.py` — current Python `Result` with `Failure`
- `service/Embedding/src/schemas/results/failure.py` — file to be renamed
