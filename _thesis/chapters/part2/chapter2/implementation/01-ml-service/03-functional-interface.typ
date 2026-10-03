==== Functional Interface

The service operates through a granular, model-specific interface. Orchestration of multiple models is managed upstream by the Backend or through asynchronous task processors, ensuring that the ML service remains focused on the computational task of inference.

The primary interface allows for the generation of high-dimensional vector embeddings from provided image data. Upon receiving a processing request, the service:
1. Normalizes the input image according to the requirements of the selected model.
2. Executes the forward pass through the deep learning architecture.
3. Returns a standardized response containing the numerical embedding, its dimensions, and metadata regarding the processing time.
