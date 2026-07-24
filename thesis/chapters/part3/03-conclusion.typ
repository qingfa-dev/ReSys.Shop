== Conclusion

This thesis presented the design, implementation, and evaluation of ReSys.Shop — a fashion e-commerce platform with Content-Based Image Retrieval capabilities.

The primary contributions are:

#enum(numbering: "1.")[
  [*Modular monolith architecture*] demonstrating that 8 self-contained business modules can communicate exclusively via in-process message dispatch, achieving module isolation while maintaining single-unit deployability.
][
  [*Vertical-slice feature organization*] showing that co-locating handler, endpoint, request, response, and validator per feature reduces cross-cutting concerns and improves testability.
][
  [*Explicit error handling*] through the Result<T> type system, eliminating exception-driven control flow and making all failure paths traceable.
][
  [*Comparative ML evaluation*] of 4 embedding models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) on fashion image retrieval, with Fashion-CLIP achieving the highest mAP (0.7455) while EfficientNet-B0 offered the best latency-throughput balance.
]

The system validates that a modular monolith with vertical slices can support both stable transactional domains and rapidly iterating ML-powered features without architectural compromise.
