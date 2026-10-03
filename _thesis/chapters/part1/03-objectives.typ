== Objectives and Scope

The main goal of this project is to build a prototype fashion e-commerce system that allows users to search for products using images instead of text. Rather than developing new AI models, this project focuses on evaluating how well existing visual search techniques can be integrated into a practical web application.

=== Technical Objectives

This project seeks to address specific engineering challenges by:

- *Demonstrating the Integration* of pre-trained deep learning models into a typical e-commerce stack (PostgreSQL + .NET).
- *Architecting a Polyglot System* that efficiently bridges .NET transactional logic with Python-based AI inference.
- *Validating the Feasibility* of using open-source vector databases (pgvector) for real-time similarity search.
- *Benchmarking Performance* of varying AI architectures (CNN vs. ViT) within a constrained hardware environment.

=== Research Questions

The project aims to answer the following questions:

+ *RQ1:* How does a fashion-specific model (Fashion-CLIP) compare to a general-purpose model (EfficientNet) when searching for similar fashion products?

+ *RQ2:* What are the trade-offs between search accuracy and processing speed when using different AI models?

+ *RQ3:* Can a microservices architecture effectively separate AI processing from the main web application while keeping response times reasonable for users?

=== Specific Tasks

To answer these questions, the following tasks were completed:

+ *Build an AI Service:*
  - Create a Python service using FastAPI that can load and run different image models
  - The service should be able to process images and return feature vectors
  - Target: respond within a few hundred milliseconds per image

+ *Set Up Vector Search:*
  - Configure PostgreSQL with the pgvector extension to store and search image vectors
  - Test that similarity searches work correctly with thousands of products

+ *Connect the Services:*
  - Build a .NET backend that communicates with the Python AI service
  - Ensure the full search process (upload image, extract features, search database, return results) completes in under one second

+ *Create the User Interface:*
  - Build a Vue.js frontend where users can upload images and see similar products
  - Make the interface responsive and easy to use

+ *Evaluate the Results:*
  - Measure search accuracy using standard metrics (how often does the system return relevant products?)
  - Measure processing speed and compare different models

=== Scope and Limitations

*Included Scope:*
- Image upload and visual search functionality
- Product recommendations based on visual similarity
- Basic e-commerce features (product catalog, shopping cart)
- Comparison of three AI models (EfficientNet, DINOv2, Fashion-CLIP)

*Excluded Scope:*
- Real payment processing (simulated only)
- Complex shipping and logistics
- User accounts with social login
- Mobile app development

=== Known Limitations

This project has several limitations that should be acknowledged:

+ *Dataset Size:* The evaluation uses 5,000 products, which is smaller than real e-commerce catalogs with millions of items. Results may not scale directly.

+ *Hardware:* Testing was done on a laptop with limited GPU capabilities. A production system would likely use more powerful servers.

+ *No User Testing:* Due to time constraints, the evaluation focuses on technical metrics rather than studies with real users. Results were visually inspected but no formal user experience testing was conducted.

+ *Pre-trained Models Only:* The project uses existing pre-trained models without fine-tuning them on the specific dataset. Custom training might improve results but was beyond the scope.

These limitations provide starting points for future improvements, which are discussed in the conclusion.

