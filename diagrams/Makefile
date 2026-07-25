PLANTUML = java -jar plantuml.jar
MMDC = mmdc
OUTDIR = ../thesis/images/diagrams
PLANTUML_SRC = $(wildcard *.puml)
MMD_SRC = $(wildcard *.mmd)
PLANTUML_OUT = $(patsubst %.puml,$(OUTDIR)/%.png,$(PLANTUML_SRC))
MMD_OUT = $(patsubst %.mmd,$(OUTDIR)/%.png,$(MMD_SRC))

.PHONY: all clean plantuml mermaid

all: $(OUTDIR) plantuml mermaid

$(OUTDIR):
	mkdir -p $(OUTDIR)

plantuml: $(PLANTUML_OUT)

$(OUTDIR)/%.png: %.puml
	$(PLANTUML) -tpng -output $(OUTDIR) $<

mermaid: $(MMD_OUT)

$(OUTDIR)/%.png: %.mmd
	$(MMDC) -i $< -o $@ -w 1200 -b transparent

clean:
	rm -rf $(OUTDIR)/*.png
