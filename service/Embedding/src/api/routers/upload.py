"""
Image upload and retrieval endpoints.
Stores uploaded images to local filesystem and serves them back.
"""
import os
import time
import uuid
from pathlib import Path

from embedding.api.routers.inference import verify_api_key
from embedding.core.config import settings
from embedding.core.rate_limit import limiter
from embedding.schemas.results.error import Error
from embedding.schemas.results.result import Result, ValueResult
from fastapi import APIRouter, Depends, File, Request, Response, UploadFile, status
from fastapi.responses import FileResponse, JSONResponse

router = APIRouter(tags=["images"])

ALLOWED_MIME_TYPES = {"image/jpeg", "image/png", "image/webp"}
MAX_UPLOAD_SIZE = 10 * 1024 * 1024  # 10 MB


def _sanitize_filename(filename: str) -> str:
    """Strip path separators and non-alphanumeric chars from filename."""
    name = Path(filename).name
    safe = "".join(c for c in name if c.isalnum() or c in "._- ")
    if not safe:
        return f"upload_{uuid.uuid4().hex[:8]}"
    return safe


def _ensure_upload_dir() -> Path:
    """Create upload directory if it doesn't exist."""
    upload_dir = Path(settings.UPLOAD_DIR).resolve()
    upload_dir.mkdir(parents=True, exist_ok=True)
    return upload_dir


@router.post(
    "/images/upload",
    response_model=ValueResult,
    status_code=status.HTTP_200_OK,
    summary="Upload an image",
    description="Uploads an image file to the sidecar for later embedding. "
                "Returns a URL path for use with POST /embeddings.",
)
@limiter.limit(settings.RATE_LIMIT)
async def upload_image(
    request: Request,
    response: Response,
    image: UploadFile = File(...),
    key: str = Depends(verify_api_key),
):
    """Stores an uploaded image and returns its serving URL."""
    if image.content_type not in ALLOWED_MIME_TYPES:
        response.status_code = status.HTTP_400_BAD_REQUEST
        return Result.failure(
            Error.bad_request(
                "Image.UnsupportedType",
                f"Unsupported content type: {image.content_type}. "
                f"Allowed: {', '.join(ALLOWED_MIME_TYPES)}",
            )
        )

    image_bytes = await image.read()
    if len(image_bytes) > MAX_UPLOAD_SIZE:
        response.status_code = status.HTTP_413_REQUEST_ENTITY_TOO_LARGE
        return Result.failure(
            Error.bad_request(
                "Image.TooLarge",
                f"File too large. Max size: {MAX_UPLOAD_SIZE // (1024 * 1024)} MB",
            )
        )

    safe_name = _sanitize_filename(image.filename or "upload.bin")
    stem, ext = os.path.splitext(safe_name)
    upload_dir = _ensure_upload_dir()
    dest = upload_dir / safe_name
    if dest.exists():
        dest = upload_dir / f"{stem}_{int(time.time())}{ext}"

    dest.write_bytes(image_bytes)

    return ValueResult.ok_value({"url": f"/images/{dest.name}"})


@router.get(
    "/images/{name:path}",
    summary="Serve an uploaded image",
    description="Returns a stored image file by filename.",
)
async def serve_image(name: str, key: str = Depends(verify_api_key)):
    """Serves a previously uploaded image file."""
    safe_name = _sanitize_filename(name)
    upload_dir = Path(settings.UPLOAD_DIR).resolve()
    file_path = upload_dir / safe_name

    if not file_path.exists() or not file_path.is_file():
        return JSONResponse(
            status_code=404,
            content=Result.failure(
                Error.not_found("Image.NotFound", f"Image not found: {name}")
            ).model_dump(by_alias=True),
        )

    return FileResponse(
        path=str(file_path),
        media_type="application/octet-stream",
        filename=safe_name,
    )
