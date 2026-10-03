==== Health and Monitoring

To support reliable operation within a containerized environment, the service implements comprehensive health monitoring. This system verifies not only the availability of the web interface but also the operational status of the underlying hardware and AI models:
1. *Hardware Readiness:* Confirms that the required GPU (CUDA) environment is active and accessible.
2. *Model Integrity:* Verifies that the primary models are correctly loaded into memory and ready for inference.
3. *Execution Sanity:* Periodically runs a baseline inference task to ensure the entire pipeline, spanning from input preprocessing to output generation, is functioning correctly.

This proactive monitoring allows the system orchestrator to automatically detect and recover from hardware or runtime failures without manual intervention.
