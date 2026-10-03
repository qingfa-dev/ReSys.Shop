import matplotlib.pyplot as plt
import os

# Data
models = ['EfficientNet-B0', 'Fashion-CLIP', 'DINOv2', 'CLIP (ViT-B/16)']
avg_ms = [27.99, 98.30, 94.70, 252.34]
std_ms = [8.17, 10.92, 4.92, 17.39]
colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728']

# Create output directory
script_dir = os.path.dirname(os.path.abspath(__file__))
output_dir = os.path.join(script_dir, "..", "..", "images", "diagrams", "charts")
os.makedirs(output_dir, exist_ok=True)

# Plot
plt.figure(figsize=(10, 6))
bars = plt.bar(models, avg_ms, yerr=std_ms, capsize=5, color=colors, alpha=0.8, edgecolor='black')

# Styling
plt.title('Inference Latency by Model (Mean ± Std Dev)', fontsize=14, fontweight='bold')
plt.ylabel('Latency (ms)', fontsize=12)
plt.xlabel('Model Architecture', fontsize=12)
plt.grid(axis='y', linestyle='--', alpha=0.7)
plt.ylim(0, 300)

# Add value labels
for bar in bars:
    height = bar.get_height()
    plt.text(bar.get_x() + bar.get_width()/2., height + 5,
             f'{height:.1f} ms',
             ha='center', va='bottom', fontsize=11, fontweight='bold')

# Save
output_path = os.path.join(output_dir, "latency_histogram.png")
plt.savefig(output_path, dpi=300, bbox_inches='tight')
print(f"Chart saved to: {output_path}")
