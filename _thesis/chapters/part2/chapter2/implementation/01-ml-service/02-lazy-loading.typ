#import "/template/ctu-styles.typ" as styles
==== Service Architecture (Lazy Loading)

To optimize resource usage, the service employs a *Lazy Loading* strategy via a Singleton `ModelManager`. Deep learning models are only loaded into GPU memory upon their first request. This prevents the service from consuming gigabytes of VRAM at startup for models that may not be immediately needed.

```python
class ModelManager:
    _instance = None
    _embedders = {} # Cache for loaded models

    def get_embedder(self, model_name):
        if model_name in self._embedders:
            return self._embedders[model_name]

        // Lazy load on first request
        if model_name == "dinov2_vits14": model = load_dino_model()
        elif model_name == "fashion_clip": model = load_fashion_clip()

        self._embedders[model_name] = model
        return model
```

#figure(
  placement: none,
  image("../../../../../images/diagrams/01-ml-models/ml-06-model-lifecycle.png", width: 45%),
  caption: [Lazy Loading State Machine: Visual tracking of the Model Manager's lifecycle states (Unloaded $\to$ Ready $\to$ Evicted).],
)


#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0016-embeddings.png", width: 90%),
  caption: [Vector Generation Sequence: The asynchronous pipeline from image reception to embedding storage.],
)

/*
  BENCHMARK DATA SOURCE: src/services/ReSys.ML/results/thesis_validation/performance_benchmarks.csv

  Model,             Role,            Avg_ms,  Std_ms,  P95_ms,  Throughput
  efficientnet_b0,   Search,          27.99,   8.17,    46.00,   35.72
  fashion_clip,      Search,          98.30,   10.92,   115.92,  10.17  <-- Used in Chart
  dinov2_vits14,     Search,          94.70,   4.92,    103.08,  10.56
  clip_vit_b16,      Recommendation,  252.34,  17.39,   278.62,  3.96
*/
#figure(
  placement: none,
  image("../../../../../images/diagrams/charts/latency_histogram.png", width: 80%),
  caption: [Performance Metrics: Inference latency distribution for Fashion-CLIP (Mean: 98.3ms, P95: 115.9ms). Data derived from Thesis Validation Benchmark (N=1000).],
)
