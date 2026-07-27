== CONVOLUTIONAL NEURAL NETWORKS: THE TRADITIONAL APPROACH

Convolutional Neural Networks (CNNs) have been the dominant architecture for computer vision since 2012. This section explains how CNNs work and introduces EfficientNet-B0, which serves as the baseline model for comparison in this project.

=== Brief History

CNNs have dominated computer vision since 2012, when AlexNet showed they could outperform traditional methods on image classification tasks @he2016deep. Since then, many improved architectures have been developed:

- *VGGNet (2014):* Showed that deeper networks perform better
- *ResNet (2015):* Introduced skip connections to train very deep networks
- *EfficientNet (2019):* Optimized the balance between depth, width, and resolution @tan2019efficientnet

For this project, EfficientNet-B0 was chosen because it provides a good balance between accuracy and speed.

=== Convolutional Operations and Feature Extraction

The key idea behind CNNs is the *convolution* operation. Instead of looking at an entire image at once, a small filter (typically 3×3 or 5×5 pixels) slides across the image, detecting local patterns.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Layer Type*], [*What It Detects*],
    [Early layers], [Simple patterns: edges, colors, corners],
    [Middle layers], [Combinations: textures, shapes, parts],
    [Late layers], [High-level features: objects, styles],
  ),
  caption: [What different CNN layers learn to detect]
)

As information flows through the network, the model builds up increasingly complex representations. Early layers might detect the edge of a sleeve, middle layers might recognize "a striped pattern," and late layers might understand "a formal button-down shirt."

=== EfficientNet-B0: The Baseline Model

EfficientNet is a family of models that use *compound scaling*, which is a technique that balances network depth, width, and input resolution to maximize accuracy for a given computational budget @tan2019efficientnet.

#figure(
  image("/images/diagrams/01-ml-models/ml-01-efficientnet-b0.png", width: 80%),
  caption: [EfficientNet-B0 architecture showing the flow from input image to feature vector]
) <fig-efficientnet-arch>

Key characteristics of EfficientNet-B0:

- *Input size:* 224 × 224 pixels
- *Output embedding:* 1,280 dimensions
- *Parameters:* ~5.3 million (relatively small)
- *Inference speed:* Fast on both CPU and GPU

=== Inductive Bias: Local Pattern Detection vs Semantic Understanding

CNNs have an *inductive bias* toward local patterns. This means they are naturally good at detecting:

- Color distributions
- Texture patterns (stripes, florals, solids)
- Simple shapes and edges

However, CNNs may struggle with:

- Understanding overall "style" or "aesthetic"
- Matching items that look similar but have different colors
- Capturing the semantic meaning of fashion concepts

For example, a CNN might match a red plaid shirt to another red plaid item, but miss that a user looking at a "casual weekend shirt" might also like blue denim shirts. This limitation motivated exploring alternative architectures like Vision Transformers.

=== EfficientNet-B0 as Baseline: Establishing Performance Benchmarks

Even though newer models exist, EfficientNet-B0 serves as an important baseline because:

1. *It is well-understood:* Extensive documentation and community support
2. *It is fast:* Important for real-time search applications
3. *It provides reasonable accuracy:* Good enough for many use cases
4. *It enables comparison:* Helps quantify the improvement from newer models

By comparing EfficientNet to Vision Transformers, this project can measure how much accuracy is gained and at what computational cost.


While CNNs excel at detecting local patterns through their convolutional layers, they have limitations in capturing long-range dependencies and global context. This motivated researchers to explore alternative architectures inspired by natural language processing: *Vision Transformers*. The next section introduces this newer approach and explains how it addresses CNN limitations.
