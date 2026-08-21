# Language Level Audit + Re-Leveled Rewrite — Chapter 1, Part A (§1.1–1.3)

Chapter 1 is 24 pages, so this is split into two files. This one covers §1.1 (Fashion E-Commerce), §1.2 (CBIR), and §1.3 (Machine Learning Models), the sections with the most rhetorical language to fix. §1.4–1.6 (Vector Databases, Platform Architecture, Related Work) follow in the next file.

**Note on technical passages:** much of §1.3 is necessarily technical (CNN layer behavior, ResNet skip connections, ViT patch embedding, DINOv2 training). These stay close to the original, technical precision matters more than simplification here, and the sentences describing mechanisms are already reasonably plain. The audit below focuses on where real language-level issues exist, not on flagging correct technical writing as a problem.

**Note on factual corrections already established:** the three "15 to 20%" instances and the "3.4 percent lower mAP@10" figure in this range were already corrected for accuracy in the earlier factual review. Those corrected numbers (5.4%, 7.7%) are used in the rewrite below, not re-litigated here.

---

## STAGE 1 — Sentence-by-sentence audit (§1.1–1.2)

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "Global fashion e-commerce reached 770 billion USD in 2024 and is projected to surpass one trillion USD by 2030." | Clean, factual. | [NO ISSUE] |
| 2 | "However, traditional text search bars cause a 30 percent search abandonment rate because keyword queries struggle to match visual intent." | Same sourcing problem flagged in Part 1 (citation doesn't support this figure). "Struggle to match visual intent" is also a personified, slightly literary phrase for a search bar. | [METHODOLOGY] + [AI-LIKE] |
| 3 | "Fashion is fundamentally visual." | Short, clear, good model sentence for this level. | [NO ISSUE] |
| 4 | "Attributes like silhouette, drape, color harmony, and pattern density do not translate easily into search terms." | "Translate easily into search terms" is a bit abstract but understandable; borderline, not flagged as a hard problem. | [NO ISSUE] |
| 5 | "A customer can easily recognize a garment in an image but often fails to find it using keywords." | Clear, natural, good sentence. | [NO ISSUE] |
| 6 | "Major platforms like ASOS, Zalando, and Shopee invest in visual search because millions of catalog items rely on inconsistent vendor metadata." | "Rely on inconsistent vendor metadata" is a dense abstract noun phrase doing a lot of work; a simpler phrasing exists. | [UNCLEAR] |
| 7 | "Identical patterns often carry different labels across suppliers, hiding relevant products from keyword search." | "Hiding relevant products" personifies the labels; mildly literary but understandable. | [AI-LIKE] (mild) |
| 8 | "Visual search solves this metadata problem, directly preventing lost revenue from search abandonment." | "Directly preventing lost revenue" is a business-report-style phrase, reads as more polished/native than the rest of the paragraph. | [AI-LIKE] |
| 9 | "Content-Based Image Retrieval replaces text queries with image queries." | Clear, simple. | [NO ISSUE] |
| 10 | "Rather than indexing products by human-authored labels, CBIR systems encode images into dense vector representations and measure similarity through mathematical distance functions." | Long sentence (28 words), but the structure is a fair, teachable contrast. Borderline; could be split but isn't a serious problem. | [UNCLEAR] (minor) |
| 11 | "This section introduces the core concepts and mathematical foundations of CBIR as applied to fashion product search." | Throat-clearing section-preview sentence, adds no information. Common AI/formal-writing pattern. | [AI-LIKE] |
| 12 | "Rather than requiring users to describe what they want in words, CBIR lets them search by example." | Clear and natural once "requiring" is simplified slightly. Otherwise fine. | [NO ISSUE] (minor) |
| 13 | "The system encodes a query image into a dense vector embedding: a fixed-length sequence of numbers that captures shape, texture, colour, and pattern." | "Dense vector embedding" is necessary technical term, keep. Rest is clear. | [TECHNICAL TERM] |
| 14 | "This approach bypasses the need for consistent textual labels." | "Bypasses the need for" is a slightly formal but acceptable phrase, common in technical writing generally, not flagged as a serious problem. | [NO ISSUE] |
| 15 | "The semantic gap... is the discrepancy between the visual richness of a garment and a user's ability to express that richness in keywords." | "Discrepancy between the visual richness... and a user's ability to express that richness" is a dense, abstract, layered noun construction, reads as advanced/native academic style. | [AI-LIKE] |
| 16 | "CBIR bridges this gap by operating directly on visual content rather than on human-authored metadata." | "Bridges this gap" repeats the same metaphor flagged in Part 1; consistent problem across the thesis. | [AI-LIKE] |
| 17 | "The embedding serves as a universal descriptor that captures attributes such as fabric texture, silhouette proportion, and colour gradients automatically, without any keyword translation step." | "Serves as a universal descriptor" is an abstract, slightly grand claim-sounding phrase, plus a long sentence overall. | [AI-LIKE] |
| 18 | Mathematical foundations passages (1.2.3, 1.2.3.1) | Necessarily technical, formula-based. Language is already plain around the equations. No changes needed beyond very minor wording. | [NO ISSUE] / [TECHNICAL TERM] |
| 19 | "For normalized fashion embeddings, scores above 0.70 generally correspond to strong visual similarity perceptible to human shoppers." | Already flagged in the earlier factual audit as an uncited empirical claim; also "perceptible to human shoppers" is a slightly formal phrase. | [METHODOLOGY] + [TOO ADVANCED] (minor) |
| 20 | CBIR Pipeline (1.2.3.3), 4 numbered steps | Clear, list-style, plain. Good model for this level. | [NO ISSUE] |

## STAGE 1 — Sentence-by-sentence audit (§1.3, non-technical passages only)

| # | Original | Issue | Class |
|---|---|---|---|
| 21 | "CNNs have dominated computer vision since AlexNet (2012)." | "Dominated" is a common enough word, acceptable. | [NO ISSUE] |
| 22 | "Local patterns cascade into global understanding: edges become textures in middle layers, garments in late layers." | "Cascade into global understanding" is a metaphor stacked on technical content, mildly advanced but the following clause explains it clearly, so it's understandable in context. | [TOO ADVANCED] (mild) |
| 23 | "CNNs have a strong inductive bias toward local patterns" | "Inductive bias" is a real, necessary machine learning term, keep exactly. | [TECHNICAL TERM] |
| 24 | "This lets gradients flow unimpeded, enabling 50-, 101-, or 152-layer networks to train effectively." | "Unimpeded" is a genuinely rare, advanced word (means "without being blocked"); simpler alternative exists. | [TOO ADVANCED] |
| 25 | "CLIP (Contrastive Language-Image Pre-training) bridges vision and language, enabling search using both visual and textual queries." | Same "bridges" metaphor again, third occurrence across the chapter so far. | [AI-LIKE] |
| 26 | "CLIP's natural language understanding enables fashion concepts like 'bohemian style' or 'minimalist design'..." | Reasonably clear given the concrete examples that follow; not flagged as a serious problem. | [NO ISSUE] |
| 27 | "This flexibility makes CLIP-based models the primary choice for the visual search feature." | Clear, direct, good sentence for this level. | [NO ISSUE] |
| 28 | "Fashion-CLIP provides the best overall balance of retrieval quality, search flexibility, and inference performance for the target deployment scenario" | "Best overall balance... for the target deployment scenario" is a polished, almost report-style phrase, similar pattern flagged in Part 1's "optimal balance." | [AI-LIKE] |
| 29 | "General CLIP variants suit multi-category marketplaces with lower fashion accuracy." | Compressed but understandable, acceptable at this level. | [NO ISSUE] |
| 30 | "CLIP ViT-L/14 offers the largest capacity at 428 million parameters but requires substantial GPU VRAM." | "Substantial" is a moderately advanced word where "a lot of" would be simpler and equally accurate. | [TOO ADVANCED] (mild) |

---

## STAGE 2 — Methodology claims requiring verification (§1.1–1.3)

**CLAIM:** "Traditional text search bars cause a 30 percent search abandonment rate."
**STATUS:** POSSIBLY INCORRECT (repeat of the Part 1 finding)
**REASON:** citation [2] (Pinterest's search-volume press release) doesn't support this figure.
**WHAT THE AUTHOR MUST CONFIRM:** same as Part 1, either find a real source or remove the specific number.

**CLAIM:** "For normalized fashion embeddings, scores above 0.70 generally correspond to strong visual similarity perceptible to human shoppers."
**STATUS:** NEEDS VERIFICATION
**REASON:** no citation given; reads as a general empirical claim about human perception rather than your own observation.
**WHAT THE AUTHOR MUST CONFIRM:** is this from your own testing during development, or a general claim you read somewhere? If it's your own observation, the rewrite below reframes it that way (already applied in the earlier factual review, kept here).

**CLAIM:** "All models were evaluated under identical conditions: 5,000 fashion product images... split into training and query sets."
**STATUS:** SUPPORTED BY TEXT (consistent with Chapter 3's methodology)
**REASON:** matches the detailed protocol described later in §3.4.
**WHAT THE AUTHOR MUST CONFIRM:** nothing further here; full detail is audited separately when we reach Chapter 3.

---

## STAGE 3 — Re-leveled rewrite (§1.1–1.3)

```
1 BACKGROUND AND RELATED WORK

This chapter presents the background knowledge needed for this project.
- Fashion E-commerce. Market context, the semantic gap, and the business
  case for visual search.
- Content-Based Image Retrieval. Embeddings, cosine similarity, and the
  CBIR pipeline.
- Machine Learning Models. CNN, ViT, CLIP, and Fashion-CLIP
  architectures.
- Vector Databases. ANN search, HNSW, IVFFlat, and pgvector.
- Platform Architecture and Technology Stack. Modular monolith, vertical
  slice architecture, .NET, Vue, PostgreSQL, Redis, the Python service,
  orchestration, authentication, and benchmarks.
- Related Work and Research Gap. Academic research, commercial systems,
  and what makes this project different.

1.1 FASHION E-COMMERCE

Global fashion e-commerce reached 770 billion USD in 2024 and is expected
to pass one trillion USD by 2030 [1]. Text-based search bars have
difficulty matching what users actually want to find, since keyword
queries often do not match the user's visual intent.

Fashion is a visual product category. Attributes like silhouette, drape,
colour combination, and pattern density are not easy to describe using
search terms. A customer can often recognize a garment in a photo, but
cannot easily find it using keywords.

Major platforms like ASOS, Zalando, and Shopee invest in visual search
because their catalogues contain millions of items, and the text
descriptions (metadata) written by different vendors are often
inconsistent. The same pattern or style can have different labels from
different suppliers, so keyword search misses relevant products. Visual
search avoids this metadata problem and can reduce lost sales caused by
failed searches.

1.2 CONTENT-BASED IMAGE RETRIEVAL

Content-Based Image Retrieval (CBIR) uses image queries instead of text
queries. Instead of indexing products using labels written by humans,
CBIR systems convert images into vector representations and measure
similarity using mathematical distance functions.

1.2.1 Visual Search Concepts
Content-Based Image Retrieval (CBIR) uses image queries instead of text
queries. Instead of asking users to describe what they want in words,
CBIR allows them to search using an example image.

The system converts a query image into a vector embedding: a
fixed-length list of numbers that represents shape, texture, colour, and
pattern. It then finds catalogue items whose embeddings are closest in
vector space. Visually similar products produce similar vectors; different
products produce vectors that are far apart.

This method does not need consistent text labels. A photo of a dress with
a specific neckline can find visually similar products, no matter how the
catalogue describes them.

1.2.2 The Semantic Gap
The semantic gap, introduced in Section 1.1, is the difference between how
visually rich a garment is and how well a user can describe that in words.
CBIR closes this gap by working directly with the image content instead of
text metadata written by humans. The embedding acts as a general
description that automatically captures attributes such as fabric texture,
silhouette shape, and colour, without needing any text translation step.

1.2.3 Mathematical Foundations of Embeddings
[Formula and technical explanation unchanged, already at an appropriate
level for this audience.]

1.2.3.1 The Latent Space
[Unchanged, already clear and appropriately technical.]

1.2.3.2 Measuring Similarity: Cosine Similarity
[Formula unchanged.]
Cosine similarity produces values from +1.0 (identical direction) to 0.0
(no relationship) down to -1.0 (opposite direction). During development,
checking retrieval results by hand suggested that scores above about 0.70
usually matched products that looked visually similar; the platform uses
this value as a configurable threshold, described further in Section 2.3.4.

1.2.3.3 The CBIR Pipeline
[Unchanged, already clear four-step list.]

1.3 MACHINE LEARNING MODELS

This section describes three types of models used for visual feature
extraction: convolutional neural networks (CNN), vision transformers
(ViT), and CLIP-based multimodal models. Each part explains how the
model works, which specific versions were tested, and their trade-offs for
fashion retrieval. The section ends with the final model selection
decision.

1.3.1 Convolutional Neural Networks
CNNs have been the main approach in computer vision since AlexNet
(2012). Models tested: ResNet-50, ResNet-101, EfficientNet-B0, and
EfficientNet-B4.

1.3.1.1 Hierarchical Feature Extraction
A CNN processes an image using a series of learned filters. Each filter is a
small window (usually 3 by 3 pixels) that moves across the image to
detect local patterns [3], building more complex representations step by
step:

[Table 1 unchanged]

Local patterns build up into a global understanding of the image: edges
become textures in the middle layers, and garments in the later layers.
CNNs are naturally good at detecting local patterns. This means they are
strong at recognizing texture and colour, but may miss relationships
between parts of the image that are far apart.

1.3.1.2 ResNet and Skip Connections
Deeper networks can learn richer features, but they suffer from vanishing
gradients: the training signal becomes weaker as it moves backward
through many layers. ResNet solves this using skip connections: shortcut
paths that skip over convolutional blocks and add the block's input
directly to its output [3]. This allows the gradient to pass through the
network without being blocked, so networks with 50, 101, or even 152
layers can still be trained effectively.

[Figure 1 caption unchanged]

ResNet-50 (25.6M parameters, 2,048-dim embeddings) is still a strong
baseline model for image retrieval. ResNet-101 (44.5M parameters) adds
extra depth for comparison.

1.3.1.3 EfficientNet and Compound Scaling
Traditional scaling increases a network in only one direction: depth,
width, or resolution. EfficientNet instead uses compound scaling, which
increases all three at the same time using a learned scaling factor [4].
This produces a family of models (B0 through B7) with good accuracy and
fewer parameters than typical CNNs.

[Figure 2 caption unchanged]

EfficientNet-B0 uses 5.3M parameters and 1,280-dim embeddings, and
works well for CPU-only deployment. EfficientNet-B4 (19.3M parameters,
1,792-dim embeddings) offers more capacity.

1.3.1.4 Evaluated CNN Variants
[Table 2 unchanged.]

1.3.2 Vision Transformers
Vision Transformers (ViTs) apply the transformer architecture, originally
built for language tasks, to images. Instead of convolutional filters, they
use self-attention across image patches.

1.3.2.1 Patch Embedding and Tokenization
Transformers were first developed for NLP tasks such as translation and
text generation. Their key feature is self-attention, which lets the model
consider relationships between all parts of the input at the same time. In
2020, researchers showed that this idea also works well for images [10]:
- Split the image into a grid of fixed-size patches (for example, 14 by 14
  or 16 by 16 pixels).
- Flatten each patch into a vector and treat it as a token, similar to a
  word in a sentence.
- Add position information so the model keeps track of where each patch
  came from.
- Pass the sequence through transformer layers using multi-head
  self-attention.

1.3.2.2 Global Context via Self-Attention
Unlike CNNs, which focus on local patterns, self-attention can find
relationships across the whole image starting from the first layer. A ViT
can directly compare any two patches, even if they are far apart in the
image. For fashion, this helps the model understand that a collar and
cuffs match, even though they appear on opposite sides of the image, which
is useful for retrieval tasks where overall silhouette and drape matter as
much as local texture.

1.3.2.3 DINOv2 and Self-Supervised Pre-Training
Supervised learning needs human-labeled data, which is expensive to
produce. Self-supervised learning instead learns directly from the images.
DINOv2 uses a student-teacher self-distillation approach [11]:
- Take an image and create two different views (different crops, slightly
  different colours).
- Pass them through student and teacher networks that have the same
  architecture.
- Train the student network to match the teacher's output.
- Update the teacher network using a moving average of the student's
  weights.
- Repeat this process across 142 million uncurated images.

[Figure 3 caption unchanged]

DINOv2 produces features with strong object-level structure, such as
silhouettes, part shapes, and garment boundaries, without needing category
labels. This makes it well suited to fashion, where labeled data is often
limited.

1.3.2.4 Structural Fidelity for Fashion Retrieval
DINOv2 is particularly good at capturing structure: shapes, silhouettes,
and proportions. It can match garments by cut (A-line, fitted, oversized)
and by proportion (cropped vs. full-length), and it can separate shape
from colour. This is useful for finding a dress with the same shape but a
different colour.

1.3.2.5 DINOv2 Model Specifications
[Table 3 unchanged.]

1.3.2.6 Trade-offs and Limitations
Vision Transformers have different trade-offs compared to CNNs:
Advantages:
- Better at understanding global structure and long-range relationships.
- Trained without human labels, so potentially more general.
- Strong at capturing shape and silhouette.
Disadvantages:
- Slower than CNNs (needs more computation).
- Needs larger input images to work best.
- May be more than what is needed for simple colour or pattern matching.

1.3.3 CLIP and Fashion-CLIP
CLIP (Contrastive Language-Image Pre-training) connects vision and
language, allowing search using both images and text. Fashion-CLIP, a
version fine-tuned for fashion, is the main model used for visual search in
this project.

1.3.3.1 Contrastive Language-Image Pre-Training
Traditional image models classify images into fixed categories ("cat,"
"dog," "dress"). CLIP instead learns to match images with text
descriptions [5].
During training, CLIP was shown 400 million image-text pairs from the
public web (for example, a floral dress paired with "colourful floral
summer dress," or sneakers paired with "white running shoes on grass").
From these pairs, the model learned to:
- Convert images into vectors (image encoder).
- Convert text into vectors (text encoder).
- Make matching image-text pairs produce similar vectors.

1.3.3.2 Dual-Tower Architecture
CLIP has two separate towers:
[Figure 4 caption unchanged]
- Image Tower. Processes the image using a Vision Transformer (ViT-B/16,
  ViT-B/32, or ViT-L/14).
- Text Tower. Processes text using a transformer.
Both towers produce vectors of the same size (512 dimensions for ViT-B
variants, 768 for ViT-L), so images and text can be compared directly
using cosine similarity.

1.3.3.3 Multimodal Embedding Space
Because CLIP understands natural language, it can match fashion concepts
like "bohemian style" or "minimalist design" to images, even for abstract
descriptions like "something for a casual Friday." However, general CLIP
was trained on general internet images, not fashion specifically, so it may
not clearly separate concepts like "A-line dress" from "sheath dress," or
"Bohemian" from "vintage."

1.3.3.4 Multimodal Query Capabilities
The dual-tower design allows types of search that are not possible with
vision-only models such as DINOv2 or EfficientNet:
- Text-to-image search. A user types "red floral summer dress"; the text
  encoder maps this description into the same embedding space as the
  catalogue images.
- Hybrid queries. An uploaded photo combined with text refinement ("like
  this, but in blue"), by encoding both and combining the results.
This flexibility is the main reason CLIP-based models were chosen for
visual search in this project.

1.3.3.5 Fashion-CLIP and Domain-Specific Fine-Tuning
Fashion-CLIP is trained further on top of CLIP, using over 700,000
fashion product images with detailed descriptions covering garment
categories, fabric textures, style, and occasion [6]. This extra training
helps Fashion-CLIP understand:
- Fashion-specific vocabulary ("A-line," "empire waist," "distressed
  denim").
- Style categories ("streetwear," "preppy," "athleisure").
- Occasion suitability ("office wear," "cocktail party," "beach vacation").
Fashion-CLIP uses the same ViT-B/16 architecture as CLIP, producing
512-dimensional embeddings. The benchmark in Chapter 3 shows that
Fashion-CLIP achieves a 5.4% higher mAP than general CLIP under the
category-only evaluation (Section 3.5).

[Figure 5 caption unchanged]

1.3.3.6 Evaluated CLIP Variants
[Table 4 unchanged.]

1.3.4 Model Selection and Justification
This section explains why Fashion-CLIP was chosen as the main embedding
model for visual search.

1.3.4.1 Candidate Models
Six pre-trained models across three architecture families were tested:
[Table 5 unchanged, aside from earlier factual correction already applied
to model counts elsewhere in the thesis.]

1.3.4.2 Evaluation Methodology
All models were tested under the same conditions: 5,000 fashion product
images from the Fashion Product Images dataset, split into training and
query sets. The hardware used was consumer-grade: an Intel i7-1165G7
CPU with 16 GB RAM, with all inference running on CPU.

The metrics used were Mean Average Precision (mAP@10) as the main
measure of retrieval quality, Precision at K (P@K) for accuracy at the top
of the results, Recall at K (R@K) for how many relevant items were
found, and inference latency in milliseconds. A retrieved product was
considered relevant if it was in the same category as the query image. The
full evaluation method, benchmark results, and cross-validation approach
are described in Chapter 3.

1.3.4.3 Weighted Selection Criteria
The model selection was based on four criteria: retrieval quality (mAP@10
and P@K scores), inference speed (under 300 ms total response time),
multimodal capability (search by image and by text), and hardware
compatibility with the memory and compute limits of consumer-grade
hardware.

1.3.4.4 Selection Decision
Fashion-CLIP was chosen as the main embedding model for visual search.
Three reasons support this decision.

First, retrieval quality: Fashion-CLIP had the highest mAP among the
models tested, outperforming general CLIP by 5.4% under the systematic
benchmark presented in Chapter 3 (Section 3.5) [6].

Second, multimodal capability: Fashion-CLIP's dual-tower architecture
supports search by image, by text, and by combined image-and-text
queries, which vision-only models such as DINOv2 and EfficientNet
cannot do.

Third, domain specialization: fine-tuning on 700,000 fashion images gives
Fashion-CLIP a better understanding of fashion-specific vocabulary, styles,
and garment attributes than general-purpose models have.

Fashion-CLIP gives the best overall combination of retrieval quality,
search flexibility, and inference speed for this project's target use case,
although EfficientNet-B0 offers faster CPU inference, and DINOv2 is
stronger at matching by structure and silhouette.

1.3.4.5 Alternative Deployment Scenarios
Other models may be better for different deployment situations.
EfficientNet-B0 gives the fastest inference, using only 5.3 million
parameters, but its mAP is 7.7 percent lower than Fashion-CLIP's, and it
has no text-to-image capability. DINOv2 is strong at shape and silhouette
matching but does not support multimodal search. General CLIP variants
work well for marketplaces that sell many product types, though with
lower fashion-specific accuracy. CLIP ViT-L/14 has the largest capacity, at
428 million parameters, but needs a large amount of GPU memory.

The full numerical comparison and error analysis across all models are
shown in Chapter 3.
```

---

## STAGE 4 — Final consistency check (§1.1–1.3)

| Check | Result |
|---|---|
| Vocabulary difficulty | Removed repeated "bridges/bridging" metaphor (appeared 3 times, now replaced with "connects" or "closes this gap"), "unimpeded" → "without being blocked," "substantial" → "a large amount of." Technical vocabulary (inductive bias, latent space, cosine similarity, self-attention, contrastive pre-training) all kept exactly, these are necessary and expected. |
| Sentence length | A few 28-35 word sentences shortened or split; most technical explanation sentences were already appropriately paced and left mostly unchanged. |
| Grammar | No errors introduced. |
| Repeated phrases | "Bridges/bridging" was overused in the original (3 occurrences); rewrite varies the phrasing ("connects," "closes this gap") rather than repeating one word every time, while staying simple. |
| AI-like formulaic expressions | Removed: "This section introduces the core concepts and mathematical foundations," "serves as a universal descriptor," "directly preventing lost revenue," "best overall balance... for the target deployment scenario." |
| Technical terminology | Preserved exactly: CBIR, cosine similarity, embedding, latent space, inductive bias, vanishing gradients, skip connections, compound scaling, self-attention, self-supervised, contrastive pre-training, dual-tower, multimodal, mAP, P@K, R@K. |
| Numbers | All model parameter counts, dimensions, percentages, and dataset sizes kept identical to source (aside from the already-established 5.4%/7.7% factual corrections). |
| Claims vs. evidence | The 30% abandonment figure and the 0.70 similarity threshold are both flagged again here (same issues as Part 1); the rewrite reframes the 0.70 threshold as an observation, consistent with the earlier factual fix, but does not invent a source for the 30% figure. |
| Meaning preserved | Checked against the original paragraph by paragraph; no technical content added or removed. |

---

## A. Ten most important problems (§1.1–1.3)

1. "Bridges/bridging" metaphor used three separate times (semantic gap, CLIP, CBIR sections), overused and native-sounding.
2. "This section introduces the core concepts and mathematical foundations of CBIR" — pure throat-clearing, adds nothing.
3. "The discrepancy between the visual richness of a garment and a user's ability to express that richness in keywords" — dense, layered, native-level phrasing.
4. "The embedding serves as a universal descriptor" — grand, abstract claim-style phrase.
5. "This lets gradients flow unimpeded" — "unimpeded" is genuinely rare/advanced vocabulary.
6. "Directly preventing lost revenue from search abandonment" — business-report style, out of place next to simpler surrounding sentences.
7. "Fashion-CLIP provides the best overall balance of retrieval quality, search flexibility, and inference performance for the target deployment scenario" — polished, marketing-style phrase.
8. The 30% abandonment statistic (repeated from Part 1) — sourcing problem, not just a language one.
9. The 0.70 similarity threshold stated as general fact without a source — same issue as Part 1.
10. Several 28-35 word sentences with multiple embedded clauses (e.g., the semantic gap definition, the CBIR indexing sentence) that would read more naturally split into two.

## B. Words/phrases to avoid

bridges, bridging, discrepancy, richness, serves as a universal descriptor, unimpeded, substantial, dominated, cascade into, directly preventing, best overall balance, target deployment scenario, perceptible to (as in "perceptible to human shoppers")

## C. Words/phrases that are safe and natural for your level

connects, closes the gap, difference between, without being blocked, a large amount of, main approach, works well for, gives the best combination of, matches, compares, still a strong baseline, was chosen because

## D. Writing style to use consistently

Same as Part 1: short-to-medium sentences, one main idea each, common verbs instead of metaphors ("connects" instead of "bridges," "closes the gap" instead of "bridges this gap"). For technical explanations (how CNNs work, how attention works), keep the plain, step-by-step, almost tutorial tone already present in most of the original, that register is actually a good match for your level and doesn't need much change. The problems are concentrated in the shorter framing sentences around each technical section (introductions, transitions, and selection justifications), where the original reaches for more literary or business-report language than the surrounding technical writing uses.

---

Ready for §1.4–1.6 (Vector Databases, Platform Architecture, Related Work) next, same process.
