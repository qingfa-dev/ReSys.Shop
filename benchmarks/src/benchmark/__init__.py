"""
Resys Fashion Shop — Academic Benchmark

Compares FashionCLIP, CLIP ViT-B/32, CLIP ViT-L/14, SigLIP, and EVA-CLIP
on fashion product retrieval using Precision@K, Recall@K, mAP, nDCG, and latency.

Pipeline::

    Dataset → EmbeddingModel → Embeddings → Cosine Retrieval → Metrics → Reports

Quickstart::

    uv run benchmark benchmark --dataset deepfashion --models all
    uv run benchmark report --format all
"""

__version__ = "1.0.0"
