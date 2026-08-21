# ReSys.Shop Thesis Diagram Generator
# Renders all Mermaid (.mmd), PlantUML (.puml), and benchmark chart sources
# Source tree: figures/chapters/part2/{ch1,ch2,ch3}/
# Output: PNG alongside each source file
# Benchmark charts: figures/.../ch3-evaluation/diagrams/generate_benchmark_figures.py
# (generator co-located with its output PNGs, mirroring the 04-implementations/diagram convention)

PLANTUML = java -jar plantuml.jar
MMDC = mmdc
CHARTS_SCRIPT = figures/chapters/part2/ch3-evaluation/diagrams/generate_benchmark_figures.py

MMD_SRC := $(shell find figures/chapters -name '*.mmd' -type f)
PUML_SRC := $(shell find figures/chapters -name '*.puml' -type f)

MMD_OUT := $(patsubst %.mmd,%.png,$(MMD_SRC))
PUML_OUT := $(patsubst %.puml,%.png,$(PUML_SRC))

ALL_OUT := $(MMD_OUT) $(PUML_OUT)

BENCHMARK_CHARTS := \
	figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_map.png \
	figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_precision.png \
	figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_recall.png \
	figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_latency.png \
	figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_throughput.png

.PHONY: all clean mermaid plantuml charts list

all: mermaid plantuml charts
	@echo "=== All $(words $(ALL_OUT)) diagrams + $(words $(BENCHMARK_CHARTS)) charts rendered ==="

mermaid: $(MMD_OUT)
	@echo "--- Mermaid done ($(words $(MMD_OUT)) diagrams) ---"

plantuml: $(PUML_OUT)
	@echo "--- PlantUML done ($(words $(PUML_OUT)) diagrams) ---"

charts: $(BENCHMARK_CHARTS)
	@echo "--- Benchmark charts done ($(words $(BENCHMARK_CHARTS)) charts) ---"

$(BENCHMARK_CHARTS): $(CHARTS_SCRIPT) ../benchmarks/outputs/thesis_catonly/results/thesis_results.json
	@echo "  [charts] regenerating benchmark charts from authoritative JSON"
	@python3 $(CHARTS_SCRIPT)

figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-core.png: figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-core.mmd
	@echo "  [mmd-erd] $(notdir $<) -> $(notdir $@)"
	@$(MMDC) -i $< -o $@ -w 2400 -s 1 -b transparent -t neutral

%.png: %.mmd
	@echo "  [mmd] $(notdir $<) -> $(notdir $@)"
	@$(MMDC) -i $< -o $@ -w 4800 -s 2 -b transparent -t default

%.png: %.puml
	@echo "  [puml] $(notdir $<) -> $(notdir $@)"
	@$(PLANTUML) -tpng -output "$(abspath $(dir $@))" "$<" > /dev/null 2>&1; \
	STARTUML_NAME=$$(grep -oP '@startuml \K.*' "$<" || echo ""); \
	if [ -n "$$STARTUML_NAME" ]; then \
	  GEN_PNG="$(abspath $(dir $@))/$$STARTUML_NAME.png"; \
	  if [ -f "$$GEN_PNG" ] && [ "$$GEN_PNG" != "$(abspath $@)" ]; then \
	    mv "$$GEN_PNG" "$(abspath $@)"; \
	  fi; \
	fi

list:
	@echo "=== Mermaid sources ($(words $(MMD_SRC))) ==="
	@for f in $(MMD_SRC); do echo "  $$f"; done
	@echo "=== PlantUML sources ($(words $(PUML_SRC))) ==="
	@for f in $(PUML_SRC); do echo "  $$f"; done

clean:
	@rm -rf $$(find figures/chapters -name '*.png' -type f)
	@echo "Cleaned all rendered diagrams."
