// Appendices Section
#page[
  #set align(center)
  #text(size: 14pt, weight: "bold")[APPENDICES]

  #v(1cm)
  #set align(left)

  Appendices are supplementary materials that are not essential to the main body of the thesis but provide additional information.

  = System Source Code Structure

  The implementation is divided into three primary repositories within the microservices ecosystem:

  - *Core API (.NET):* Implements the e-commerce domain logic, catalog management, and search orchestration.
  - *ML API (Python):* Contains the `ModelManager` singleton and extraction pipelines for the 5 candidate models.
  - *Web Portal (Vue.js):* Responsive discovery interface with image upload and reactive product carousels.

  = Data Availability

  The primary dataset used for evaluation is the *Fashion Product Images Dataset* @kaggle-fashion-dataset, available on Kaggle. This collection provides the ground-truth images used for both research evaluation and as the active catalog for the search feature. A high-resolution version was used for the preprocessing pipeline, where images were cleaned and normalized to a square aspect ratio (1:1) to optimize feature extraction. Experimental metadata and split information (train/val/test) are managed within the PostgreSQL database using JSONB metadata columns.

  = Hardware Specifications

  All benchmarks were performed on an Intel Core i7-1165G7 environment with 16GB RAM. Inference times reported in Chapter 3 reflect execution on the accompanying NVIDIA MX330 GPU where specified.
]

