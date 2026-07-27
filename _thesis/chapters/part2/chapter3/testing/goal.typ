== GOAL OF EVALUATION

The purpose of this evaluation phase was to answer two main questions:

1. *Search Accuracy Evaluation:* When a user uploads an image, does the system return products that are actually similar?

2. *System Latency Evaluation:* Is the system fast enough to provide a good user experience?

=== Measuring Accuracy

To measure accuracy, the project used a standard metric called *Mean Average Precision at 10 (mAP\@10)*. This measures how many of the top 10 results are actually relevant.

For these evaluations, a result was considered "relevant" if it was in the same product category as the query image. For example, if the query was a dress, a result was counted as relevant if it was also a dress.

Other metrics tracked include:
- *Precision\@10:* The fraction of the top 10 results that are relevant
- *Top-1 Accuracy:* The frequency with which the very first result matches the correct category

=== Measuring Speed

For a search feature to be useful, it needs to respond quickly. The following metrics were tracked:

- *Inference Time:* The duration the AI model takes to process one image (in milliseconds)
- *Total Latency:* The complete time elapsed from uploading an image to receiving results
- *Throughput:* The volume of searches the system can handle per second

The target was to keep total latency under one second, which is generally considered acceptable for web search.
