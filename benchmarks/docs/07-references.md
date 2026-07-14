# 07 — References

Academic papers, tools, datasets, and further reading.

## Core Papers

### Fashion-CLIP
**Chia et al., "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge"**, SIGIR 2022

- Introduced Fashion-CLIP, fine-tuned on 700K+ fashion image-text pairs
- Showed domain-specific fine-tuning improves fashion retrieval over generic CLIP
- [HuggingFace Model](https://huggingface.co/patrickjohncyh/fashion-clip)

### CLIP (Generic)
**Radford et al., "Learning Transferable Visual Models From Natural Language Supervision"**, ICML 2021

- OpenAI's foundational vision-language model
- Trained on 400M image-text pairs from the internet
- [OpenAI Blog](https://openai.com/research/clip) | [GitHub](https://github.com/openai/CLIP)

### EfficientNet
**Tan & Le, "EfficientNet: Rethinking Model Scaling for Convolutional Neural Networks"**, ICML 2019

- Introduced compound scaling (depth × width × resolution)
- EfficientNet-B0 achieves state-of-the-art accuracy-parameter trade-off
- [Google AI Blog](https://ai.googleblog.com/2019/05/efficientnet-improving-accuracy-and.html)

### ResNet
**He et al., "Deep Residual Learning for Image Recognition"**, CVPR 2016

- Introduced residual connections, enabling very deep networks
- ResNet-50 is the most widely used CNN baseline in computer vision
- [arXiv](https://arxiv.org/abs/1512.03385)

## CBIR & Retrieval Metrics

### Mean Average Precision
**Zheng et al., "SIFT Meets CNN: A Decade Survey of Instance Retrieval"**, IEEE TPAMI 2017

- Comprehensive survey of image retrieval methods
- Established mAP as the standard metric for CBIR evaluation

### Precision@K and Recall@K
Standard information retrieval metrics. See any IR textbook:
- Manning, Raghavan, Schütze, "Introduction to Information Retrieval", Cambridge University Press, 2008
- [Free online version](https://nlp.stanford.edu/IR-book/)

### nDCG
**Järvelin & Kekäläinen, "Cumulated Gain-Based Evaluation of IR Techniques"**, ACM TOIS 2002

- Introduced DCG and nDCG for ranked retrieval evaluation

## Datasets

### Fashion Product Images Dataset
**Param Aggarwal, Kaggle**

- 44,000+ fashion product images with rich metadata
- Small variant (~5,000 images) for lightweight experimentation
- [Kaggle — Full Dataset](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset)
- [Kaggle — Small Dataset](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small)

### DeepFashion
**Liu et al., "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations"**, CVPR 2016

- 800,000+ images with 50 categories and 1,000 attributes
- More complex than Fashion Product Images; used in many fashion AI papers
- [Project Page](http://mmlab.ie.cuhk.edu.hk/projects/DeepFashion.html)

## Tools & Libraries

| Tool | Purpose | Link |
|------|---------|------|
| **PyTorch** | Deep learning framework | [pytorch.org](https://pytorch.org) |
| **Transformers (HuggingFace)** | Pre-trained model loading | [huggingface.co/docs/transformers](https://huggingface.co/docs/transformers) |
| **OpenCLIP** | Open-source CLIP implementation | [github.com/mlfoundations/open_clip](https://github.com/mlfoundations/open_clip) |
| **torchvision** | PyTorch vision models (ResNet, EfficientNet) | [pytorch.org/vision](https://pytorch.org/vision) |
| **NumPy** | Numerical computing | [numpy.org](https://numpy.org) |
| **Pillow (PIL)** | Image loading and preprocessing | [pillow.readthedocs.io](https://pillow.readthedocs.io) |
| **pgvector** | Vector similarity search for PostgreSQL | [github.com/pgvector/pgvector](https://github.com/pgvector/pgvector) |
| **FAISS** | Facebook AI Similarity Search (alternative to pgvector) | [github.com/facebookresearch/faiss](https://github.com/facebookresearch/faiss) |
| **Typst** | Document typesetting (thesis tables) | [typst.app](https://typst.app) |

## Similar Benchmarks

### FashionIQ
**Wu et al., "Fashion IQ: A New Dataset Towards Natural Language Guided Retrieval"**, ICCV 2019

- Combines image + text feedback for interactive retrieval
- More complex than our benchmark (requires dialogue)

### DARN
**Huang et al., "Cross-Domain Image Retrieval with a Dual Attribute-aware Ranking Network"**, ICCV 2015

- Early work on fashion retrieval with attribute prediction

### OutfitNet
**Han et al., "Learning Fashion Compatibility with Bidirectional LSTMs"**, ACM MM 2017

- Focuses on outfit compatibility (does this shirt go with these pants?)
- Different problem from similarity search

## Further Reading

### Survey Papers
1. **Rostamzadeh et al., "Fashion Genome: Understanding Fashion Images with Structured Annotations"** — Overview of fashion AI tasks
2. **Gu et al., "Fashion Retrieval: A Survey"** — Comprehensive survey of fashion retrieval methods

### Metric Learning
1. **Schroff et al., "FaceNet: A Unified Embedding for Face Recognition and Clustering"**, CVPR 2015 — Triplet loss for learning embeddings
2. **Hermans et al., "In Defense of the Triplet Loss for Person Re-Identification"**, arXiv 2017 — Deep metric learning best practices

### Vector Databases
1. **pgvector documentation** — How to use PostgreSQL for vector search
2. **FAISS wiki** — In-memory approximate nearest neighbor search
3. **Pinecone blog** — Vector database concepts (vendor blog but good explanations)

## Thesis Writing Resources

### Academic Writing
- **Booth et al., "The Craft of Research"**, University of Chicago Press — How to structure arguments
- **Strunk & White, "The Elements of Style"** — Concise writing

### Statistical Reporting
- **APA 7th Edition** — Standard for reporting statistics in psychology/social sciences
- **Cumming, "Understanding the New Statistics"** — Effect sizes, confidence intervals, modern statistical practice

### CTU Thesis Guidelines
- Check with your department for specific formatting requirements
- Our Typst template is in `benchmarks/old/_thesis/`

## Online Resources

- [Papers With Code — Fashion Retrieval](https://paperswithcode.com/task/fashion-retrieval) — Leaderboards and papers
- [HuggingFace Model Hub](https://huggingface.co/models) — Pre-trained model downloads
- [Kaggle Datasets](https://www.kaggle.com/datasets) — Public datasets
- [PyTorch Hub](https://pytorch.org/hub/) — Pre-trained PyTorch models
- [r/MachineLearning](https://reddit.com/r/MachineLearning) — Community discussions (filter by "Discussion" tag)

---

*This reference list is maintained as part of the ReSys.Shop benchmark project. Suggest additions via pull request or issue.*
