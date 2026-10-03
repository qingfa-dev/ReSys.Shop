=== Defect Resolution Log

*Issue #1: Data Loading Stability*
- *Description:* Attempting to load the entire 5,000-image dataset in a single batch caused the application to timeout.
- *Severity:* Medium.
- *Resolution:* The data loading process was refactored to process images in smaller batches (50 at a time) with a background progress tracker.
- *Status:* RESOLVED

*Issue #2: Admin Privilege Leak*
- *Description:* During initial testing, the "Catalog Edit" button was visible to all guests due to a missing frontend role check.
- *Severity:* High.
- *Resolution:* Implemented a strict `v-if="user.isAdmin"` check in the UI and enforced API-level authorization guards.
- *Status:* RESOLVED
