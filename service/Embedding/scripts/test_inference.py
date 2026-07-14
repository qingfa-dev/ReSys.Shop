"""
CLI tool to test the running Inference Service.
Sends a request to the local API and prints the resulting embedding vector.
"""
import argparse
import json

import httpx

# ANSI colors
GREEN = "\033[92m"
YELLOW = "\033[93m"
RED = "\033[91m"
RESET = "\033[0m"

DEFAULT_URL = "http://localhost:5002/inference/embeddings"
DEFAULT_KEY = "embedding-service-key"
DEFAULT_IMAGE = "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&q=80&w=1000"


def test_inference(
    image_url: str,
    model: str,
    api_url: str,
    api_key: str,
    verbose: bool = False
):
    """Sends a POST request to the inference API."""
    payload = {
        "image_url": image_url,
        "model": model
    }
    headers = {
        "X-API-Key": api_key,
        "Content-Type": "application/json"
    }

    print(f"{YELLOW}==> Testing model '{model}' with image: {image_url}{RESET}")

    try:
        with httpx.Client(timeout=30.0) as client:
            response = client.post(api_url, json=payload, headers=headers)

        if response.status_code == 200:
            data = response.json()
            if data.get("isSuccess"):
                value = data["value"]
                vector = value["vector"]
                dim = value["dimension"]
                metadata = value.get("metadata", {})

                print(f"{GREEN}✔ Success!{RESET}")
                print(f"  Dimension: {dim}")
                print(f"  Model Version: {value['model_version']}")
                if metadata:
                    print(f"  Processing Time: {metadata.get('processing_time_ms')}ms")

                if verbose:
                    print(f"  Vector (first 5 elements): {vector[:5]}...")
            else:
                print(f"{RED}✘ API returned failure result:{RESET}")
                print(json.dumps(data.get("failures"), indent=2))
        else:
            print(f"{RED}✘ HTTP Error {response.status_code}:{RESET}")
            print(response.text)

    except httpx.ConnectError:
        print(f"{RED}✘ Error: Could not connect to the API at {api_url}.{RESET}")
        print("  Make sure the service is running (e.g., 'uv run fastapi dev src/main.py')")
    except Exception as e:
        print(f"{RED}✘ Unexpected error: {e}{RESET}")


def main():
    parser = argparse.ArgumentParser(description="Test the ReSys Inference API.")
    parser.add_argument("--image", default=DEFAULT_IMAGE, help="URL of the image to process.")
    parser.add_argument("--model", default="efficientnet_b0", help="Model ID to use.")
    parser.add_argument("--url", default=DEFAULT_URL, help="API endpoint URL.")
    parser.add_argument("--key", default=DEFAULT_KEY, help="X-API-Key header value.")
    parser.add_argument("--verbose", action="store_true", help="Print partial vector output.")

    args = parser.parse_args()

    test_inference(
        image_url=args.image,
        model=args.model,
        api_url=args.url,
        api_key=args.key,
        verbose=args.verbose
    )


if __name__ == "__main__":
    main()
