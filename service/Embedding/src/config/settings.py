from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    app_name: str = "Embedding Service"
    app_version: str = "0.1.0"
    cors_origins: list[str] = ["*"]

    # ML model configuration
    embedding_model: str = "fashion-clip"  # Default model; overridden by EMBEDDING_MODEL env var

    model_config = {"env_prefix": ""}
