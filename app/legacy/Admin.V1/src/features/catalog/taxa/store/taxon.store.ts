import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { taxonRepository } from "../api/taxon.api";
import type { TaxonListItem, TaxonTreeItem } from "../models/taxon.response";
import type { TaxonRuleListItem } from "../models/taxon-rule.response";
import type { CreateTaxonRequest, UpdateTaxonRequest } from "../models/taxon.request";
import type { TaxonQuery } from "../types/taxon.query";
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from "../models/taxon-rule.request";

export const useTaxonStore = defineStore("taxon", () => {
  const currentTaxons = ref<TaxonListItem[]>([]);

  const currentRules = ref<TaxonRuleListItem[]>([]);

  const loading = ref(false);

  const error = ref<string | null>(null);

  const totalRecords = ref(0);

  const taxonTree = computed(() => {
    const map: Record<string, TaxonTreeItem> = {};

    const roots: TaxonTreeItem[] = [];

    currentTaxons.value.forEach((t) => {
      map[t.id] = { ...t, key: t.id, children: [] };
    });

    currentTaxons.value.forEach((t) => {
      const item = map[t.id];

      if (!item) return;

      if (t.parentId && map[t.parentId]) {
        map[t.parentId]!.children.push(item);
      } else if (!t.parentId) {
        roots.push(item);
      }
    });

    const sortTree = (nodes: TaxonTreeItem[]) => {
      nodes.sort((a, b) => a.position - b.position);

      nodes.forEach((n) => sortTree(n.children));
    };

    sortTree(roots);

    return roots;
  });

  async function fetchTaxons(taxonomyId: string, query?: TaxonQuery) {
    loading.value = true;

    error.value = null;

    const params: TaxonQuery = { ...query };
    const result = await taxonRepository.listByTaxonomyId(taxonomyId, params);

    if (result.isSuccess && result.items) {
      currentTaxons.value = result.items;

      totalRecords.value = result.totalCount || 0;
    } else if (!result.isSuccess) {
      error.value = result.errors?.[0]?.message || "Failed to fetch taxons";
    }

    loading.value = false;

    return result;
  }

  async function addTaxon(taxonomyId: string, request: Omit<CreateTaxonRequest, "taxonomy_id">) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.create(taxonomyId, { ...request });

    if (result.isSuccess) {
      await fetchTaxons(taxonomyId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to add taxon";
    }

    loading.value = false;

    return result;
  }

  async function updateTaxon(taxonomyId: string, taxonId: string, request: UpdateTaxonRequest) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.update(taxonomyId, taxonId, { ...request });

    if (result.isSuccess) {
      await fetchTaxons(taxonomyId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to update taxon";
    }

    loading.value = false;

    return result;
  }

  async function deleteTaxon(taxonomyId: string, taxonId: string) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.delete(taxonomyId, taxonId);

    if (result.isSuccess) {
      await fetchTaxons(taxonomyId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to delete taxon";
    }

    loading.value = false;

    return result;
  }

  // Rule Actions

  async function fetchRules(taxonomyId: string, taxonId: string) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.listRules(taxonomyId, taxonId);

    if (result.isSuccess && result.items) {
      currentRules.value = result.items;
    } else if (!result.isSuccess) {
      error.value = result.errors?.[0]?.message || "Failed to fetch rules";
    }

    loading.value = false;

    return result;
  }

  async function addRule(taxonomyId: string, taxonId: string, request: CreateTaxonRuleRequest) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.createRule(taxonomyId, taxonId, request);

    if (result.isSuccess) {
      await fetchRules(taxonomyId, taxonId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to add rule";
    }

    loading.value = false;

    return result;
  }

  async function updateRule(
    taxonomyId: string,
    taxonId: string,
    ruleId: string,
    request: UpdateTaxonRuleRequest,
  ) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.updateRule(taxonomyId, taxonId, ruleId, request);

    if (result.isSuccess) {
      await fetchRules(taxonomyId, taxonId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to update rule";
    }

    loading.value = false;

    return result;
  }

  async function deleteRule(taxonomyId: string, taxonId: string, ruleId: string) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.deleteRule(taxonomyId, taxonId, ruleId);

    if (result.isSuccess) {
      await fetchRules(taxonomyId, taxonId);
    } else {
      error.value = result.errors?.[0]?.message || "Failed to delete rule";
    }

    loading.value = false;

    return result;
  }

  async function regenerateProducts(taxonomyId: string, taxonId: string) {
    loading.value = true;

    error.value = null;

    const result = await taxonRepository.regenerateProducts(taxonomyId, taxonId);

    if (!result.isSuccess) {
      error.value = result.errors?.[0]?.message || "Failed to regenerate products";
    }

    loading.value = false;

    return result;
  }

  return {
    currentTaxons,

    currentRules,

    taxonTree,

    loading,

    error,

    totalRecords,

    fetchTaxons,

    addTaxon,

    updateTaxon,

    deleteTaxon,

    fetchRules,

    addRule,

    updateRule,

    deleteRule,

    regenerateProducts,
  };
});
