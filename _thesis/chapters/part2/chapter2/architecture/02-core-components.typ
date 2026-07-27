=== Core Components

The system is composed of three primary service groups:

+ *Storefront & Admin UI (Frontend):*
  Built with *Vue 3* and *Vite*, utilizing a component-based architecture. It provides a responsive interface that communicates with the backend via RESTful APIs. It is stateless and served via a lightweight Node.js server.

+ *Core API Service (Backend - Transactional):*
  The central nervous system built with *\.NET 10*. It handles authentication (Auth0/JWT), data persistence (PostgreSQL), and orchestrates business workflows using *MediatR*. It serves as the "Source of Truth" for all business data.

+ *ML Service Layer (AI Engine):*
  Split into two specialized Python 3.12 / FastAPI services:
  - *ReSys.ImageSearch (Production):* A high-performance, stateless inference engine optimized for serving vector embeddings (Fashion-CLIP) with low latency.
  - *ReSys.ML (Research):* A comprehensive workbench for training, evaluating, and comparing different model architectures (CNN vs Transformers) as part of the thesis research.
