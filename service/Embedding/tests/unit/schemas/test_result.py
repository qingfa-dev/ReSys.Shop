"""
Unit tests for the Result / ValueResult monad.
"""
from embedding.schemas import Failure, Result, ValueResult
from PIL import Image


class TestResult:
    def test_ok_defaults(self):
        result = Result.ok()
        assert result.is_success is True
        assert result.status_code == 200
        assert result.message is None
        assert result.failures == []

    def test_ok_with_message(self):
        result = Result.ok(status_code=201, message="Created")
        assert result.status_code == 201
        assert result.message == "Created"

    def test_failure_single(self):
        fail = Failure.bad_request("E.Code", "desc")
        result = Result.failure(fail)
        assert result.is_success is False
        assert result.status_code == 400
        assert len(result.failures) == 1
        assert result.failures[0].code == "E.Code"

    def test_failure_list(self):
        fails = [
            Failure.validation("V1", "first"),
            Failure.validation("V2", "second"),
        ]
        result = Result.failure(fails)
        assert result.is_success is False
        assert len(result.failures) == 2
        # Status code is taken from the first failure
        assert result.status_code == 400

    def test_failure_empty_list_defaults_status_code(self):
        result = Result.failure([])
        assert result.is_success is False
        assert result.status_code == 400  # default when no failures provided

    def test_serialisation_uses_camel_case_aliases(self):
        """isSuccess and statusCode must survive model_dump(by_alias=True) for JSON responses."""
        result = Result.ok()
        d = result.model_dump(by_alias=True)
        assert "isSuccess" in d
        assert "statusCode" in d


class TestValueResult:
    def test_ok_value_string(self):
        result = ValueResult.ok_value("hello")
        assert result.is_success is True
        assert result.value == "hello"

    def test_ok_value_list(self):
        result = ValueResult.ok_value([1.0, 2.0, 3.0])
        assert result.value == [1.0, 2.0, 3.0]

    def test_ok_value_arbitrary_type_pil_image(self):
        """
        PIL Image is not a pydantic-native type.
        ValueResult must support arbitrary_types_allowed.
        """
        img = Image.new("RGB", (10, 10))
        result = ValueResult.ok_value(value=img)
        assert result.is_success is True
        assert isinstance(result.value, Image.Image)

    def test_failure_value_sets_value_to_none(self):
        fail = Failure.not_found("D.Missing", "not found")
        result = ValueResult.failure_value(fail)
        assert result.is_success is False
        assert result.value is None
        assert result.status_code == 404

    def test_failure_value_from_list(self):
        fails = [Failure.internal_error("E1", "a"), Failure.internal_error("E2", "b")]
        result = ValueResult.failure_value(fails)
        assert result.is_success is False
        assert len(result.failures) == 2
        assert result.status_code == 500

    def test_is_success_false_has_no_value(self):
        result = ValueResult.failure_value(Failure.validation("V", "bad"))
        assert result.value is None

    def test_chaining_failure_propagation(self):
        """
        Simulates how failures are propagated across service layers:
        inner result fails → outer wraps the same failures.
        """
        inner = ValueResult.failure_value(Failure.not_found("Inner.Missing", "not found"))
        outer = ValueResult.failure_value(inner.failures)
        assert outer.is_success is False
        assert outer.failures[0].code == "Inner.Missing"
