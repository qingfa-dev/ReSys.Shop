"""
Model management API endpoints — ONNX export triggering and status.
"""
from embedding.core.config import settings
from embedding.core.constants import Constants
from embedding.models.onnx.export_state import ExportState
from embedding.schemas import OnnxExportResponse, ValueResult
from fastapi import APIRouter, Depends, Request, Response, Security, status
from fastapi.security import APIKeyHeader

router = APIRouter(tags=["models"])

api_key_header = APIKeyHeader(
    name=Constants.Strings.X_API_KEY_HEADER, auto_error=False
)


async def verify_api_key(api_key: str = Security(api_key_header)) -> str:
    """Validate sidecar API key."""
    if api_key != settings.API_KEY:
        from fastapi import HTTPException

        raise HTTPException(status_code=403, detail="Invalid API Key")
    return api_key


@router.post(
    "/models/onnx/export",
    response_model=ValueResult[OnnxExportResponse],
    status_code=status.HTTP_202_ACCEPTED,
    summary="Trigger ONNX Model Export",
    description=(
        "Starts background ONNX export for all vision models. "
        "Returns immediately with current export status. "
        "If export is already running, returns the existing status."
    ),
)
async def trigger_onnx_export(
    request: Request,
    response: Response,
    key: str = Depends(verify_api_key),
):
    """Trigger ONNX export as a background process.

    Behavior:
    - First call: starts export thread, returns 202 with initial status.
    - Subsequent calls while running: returns 200 with current progress.
    - After completion: returns 200 with final report.

    Args:
        request: FastAPI request object (injected).
        response: FastAPI response object (injected, used to set status code).
        key: Validated API key (injected by Depends).

    Returns:
        ValueResult[OnnxExportResponse] with per-model export status.
    """
    state = ExportState()

    was_running = state.is_running()
    report = state.start_export()

    # Build response payload
    payload = OnnxExportResponse(
        overallStatus=report.overall_status.value,
        models=[
            {
                "modelName": m.model_name,
                "status": m.status.value,
                "durationMs": m.duration_ms,
                "error": m.error,
            }
            for m in report.models
        ],
        startTime=report.start_time,
        endTime=report.end_time,
        totalDurationMs=report.total_duration_ms,
    )

    # 202 if we just started; 200 if it was already running/completed
    if not was_running and report.overall_status.value == "running":
        response.status_code = status.HTTP_202_ACCEPTED
    else:
        response.status_code = status.HTTP_200_OK

    return ValueResult.ok_value(payload)
