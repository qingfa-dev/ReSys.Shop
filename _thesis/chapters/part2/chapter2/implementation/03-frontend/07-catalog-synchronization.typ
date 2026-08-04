===== 3. Catalog Synchronization (URL State)
To support deep-linking and sharing, the application implements bidirectional synchronization between the application state and the URL bar.
- *Flow:* User modifies a filter $\to$ State updates $\to$ URL Query Parameter updates.
- *Benefit:* Reloading the page restores the exact combination of filters, sort order, and search queries.
