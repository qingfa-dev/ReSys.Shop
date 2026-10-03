"""
Unit tests for the Error factory methods.
These are pure model tests — no I/O, no app startup.
"""
import pytest
from embedding.schemas import Error, ErrorType


class TestErrorFactories:
    def test_validation(self):
        f = Error.validation("VAL001", "Invalid data")
        assert f.type == ErrorType.Validation
        assert f.status_code == 400
        assert f.code == "VAL001"
        assert f.description == "Invalid data"

    def test_conflict(self):
        f = Error.conflict("CON001", "Conflict detected")
        assert f.type == ErrorType.Conflict
        assert f.status_code == 409
        assert f.code == "CON001"

    def test_not_found(self):
        f = Error.not_found("NF404", "Not found")
        assert f.type == ErrorType.NotFound
        assert f.status_code == 404

    def test_bad_request(self):
        f = Error.bad_request("BR400", "Bad input")
        assert f.type == ErrorType.BadRequest
        assert f.status_code == 400

    def test_internal_error(self):
        f = Error.internal_error("ERR500", "Server error")
        assert f.type == ErrorType.InternalError
        assert f.status_code == 500

    def test_unauthorized(self):
        f = Error.unauthorized("AUTH401", "No token")
        assert f.type == ErrorType.Unauthorized
        assert f.status_code == 401

    def test_forbidden(self):
        f = Error.forbidden("FORB403", "No permission")
        assert f.type == ErrorType.Forbidden
        assert f.status_code == 403

    def test_failure_is_immutable_pydantic_model(self):
        """Errors should be plain data objects, not mutated after creation."""
        f = Error.validation("V001", "desc")
        with pytest.raises(Exception):
            f.code = "mutated"  # type: ignore

    def test_failure_serialises_to_dict(self):
        """Error must survive a round-trip through model_dump (used in HTTPException detail)."""
        f = Error.not_found("X.Missing", "Resource not found")
        d = f.model_dump()
        assert d["code"] == "X.Missing"
        assert d["status_code"] == 404
        assert d["type"] == ErrorType.NotFound
