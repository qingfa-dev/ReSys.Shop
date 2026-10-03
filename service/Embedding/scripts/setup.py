"""
Unified setup script for the inference service environment.
Handles .env creation, dependency syncing, and model export.
"""
import shutil
import subprocess
import sys
from pathlib import Path

# ANSI colors
GREEN = "\033[92m"
YELLOW = "\033[93m"
RED = "\033[91m"
RESET = "\033[0m"

SERVICE_DIR = Path(__file__).parent.parent


def run_command(command, description, cwd=SERVICE_DIR):
    """Executes a shell command and prints progress."""
    print(f"{YELLOW}==> {description}...{RESET}")
    try:
        subprocess.run(command, shell=True, check=True, cwd=cwd)
        print(f"{GREEN}✔ Successfully completed: {description}{RESET}")
        return True
    except subprocess.CalledProcessError as e:
        print(f"{RED}✘ Error during {description}: {e}{RESET}")
        return False


def setup_env_files():
    """Ensures .env exists based on template."""
    print(f"{YELLOW}==> Setting up environment files...{RESET}")
    template = SERVICE_DIR / ".env.template"
    target_env = SERVICE_DIR / ".env"

    if not template.exists():
        print(f"{RED}✘ Error: .env.template not found in {SERVICE_DIR}{RESET}")
        return

    if not target_env.exists():
        shutil.copy(template, target_env)
        print(f"{GREEN}✔ Created .env from template{RESET}")
    else:
        print("ℹ .env already exists, skipping...")


def sync_dependencies():
    """Syncs dependencies using uv."""
    return run_command("uv sync", "Syncing dependencies with uv")


def export_models():
    """Optionally exports models to ONNX."""
    print(f"\n{YELLOW}==> (Optional) Export models to ONNX?{RESET} [y/N] ", end="")
    choice = input().lower()
    if choice == "y":
        return run_command("uv run python scripts/export_onnx.py", "Exporting models to ONNX")
    else:
        print("ℹ Skipping ONNX export.")
        return True


def main():
    """Main setup flow."""
    print(f"{GREEN}--- Starting ReSys Inference Service Setup ---{RESET}\n")

    setup_env_files()

    if not sync_dependencies():
        print(f"\n{RED}✘ Setup failed during dependency sync.{RESET}")
        sys.exit(1)

    if not export_models():
        print(
            f"\n{YELLOW}⚠ Models could not be exported, but environment is otherwise ready.{RESET}"
        )

    print(f"\n{GREEN}✔ Setup complete! You can now run the service with:{RESET}")
    print(f"  {YELLOW}uv run fastapi dev src/main.py{RESET}")


if __name__ == "__main__":
    main()
