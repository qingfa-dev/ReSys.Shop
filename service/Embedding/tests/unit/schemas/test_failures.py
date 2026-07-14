"""
Unit tests for the Failure factory methods.
These are pure model tests — no I/O, no app startup.
"""
import pytest
from embedding.schemas import Failure, FailureType


class TestFailureFactories:
    def test_validation(self):
        f = Failure.validation("VAL001", "Invalid data")
        assert f.type == FailureType.Validation
        assert f.status_code == 400
        assert f.code == "VAL001"
        assert f.description == "Invalid data"

    def test_conflict(self):
        f = Failure.conflict("CON001", "Conflict detected")
        assert f.type == FailureType.Conflict
        assert f.status_code == 409
        assert f.code == "CON001"

    def test_not_found(self):
        f = Failure.not_found("NF404", "Not found")
        assert f.type == FailureType.NotFound
        assert f.status_code == 404

    def test_bad_request(self):
        f = Failure.bad_request("BR400", "Bad input")
        assert f.type == FailureType.BadRequest
        assert f.status_code == 400

    def test_internal_error(self):
        f = Failure.internal_error("ERR500", "Server error")
        assert f.type == FailureType.InternalError
        assert f.status_code == 500

    def test_unauthorized(self):
        f = Failure.unauthorized("AUTH401", "No token")
        assert f.type == FailureType.Unauthorized
        assert f.status_code == 401

    def test_forbidden(self):
        f = Failure.forbidden("FORB403", "No permission")
        assert f.type == FailureType.Forbidden
        assert f.status_code == 403

    def test_failure_is_immutable_pydantic_model(self):
        """Failures should be plain data objects, not mutated after creation."""
        f = Failure.validation("V001", "desc")
        with pytest.raises(Exception):
            f.code = "mutated"  # type: ignore

    def test_failure_serialises_to_dict(self):
        """Failure must survive a round-trip through model_dump (used in HTTPException detail)."""
        f = Failure.not_found("X.Missing", "Resource not found")
        d = f.model_dump()
        assert d["code"] == "X.Missing"
        assert d["status_code"] == 404
        assert d["type"] == FailureType.NotFound
