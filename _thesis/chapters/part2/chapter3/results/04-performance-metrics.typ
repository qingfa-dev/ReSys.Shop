=== System Latency Profile

#figure(
  image("/images/charts/results/efficiency-tradeoff.png", width: 100%),
  caption: [Inference Latency Comparison. Fashion-CLIP achieves sub-70ms inference on standard hardware.],
)

- *Search Response:* The system achieved an average inference latency of *67.7ms* (Fashion-CLIP) on the constrained MX330 GPU. The total end-to-end search latency (including network round-trip and vector retrieval) was recorded at *280ms*. While this exceeds the optimistic 200ms design target, it remains within the acceptable range for web-based interactivity (< 500ms).
