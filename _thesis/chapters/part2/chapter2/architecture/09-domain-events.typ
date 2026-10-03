==== Domain Events

system uses *Domain Events* to decouple side effects. When an aggregate changes state, it raises an event (implementing `IDomainEvent`) which is dispatched to in-process handlers.

*Example Flow:*
1. `Product` aggregate processes `AddImage()`.
2. `Product` raises `ImageUploadedEvent`.
3. Database Transaction commits the Product change.
4. *Side Effect:* `GenerateImageEmbeddingHandler` listens to the event and initiates the background vectorization job.
