namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

public abstract record TaxonRuleParameter(string Type = "", string MatchPolicy = "", string Value = "");