import type { OptionTypeListItem, OptionTypeDetail } from "../models/option-type.response";

export const OptionTypeMapper = {
  toListItem(dto: unknown): OptionTypeListItem {
    const r = (dto as Record<string, unknown>) ?? {};
    return {
      id: String(r.id ?? ""),
      name: String(r.name ?? ""),
      presentation: (r.presentation as string) ?? null,
      position: Number(r.position ?? 0),
      filterable: Boolean(r.filterable),
      optionValuesCount: Number(r.optionValuesCount ?? 0),
      productsCount: Number(r.productsCount ?? 0),
      createdAtUtc: String(r.createdAtUtc ?? ""),
      modifiedAtUtc: (r.modifiedAtUtc as string) ?? null,
    };
  },

  toDetail(dto: unknown): OptionTypeDetail {
    const r = (dto as Record<string, unknown>) ?? {};
    return {
      id: String(r.id ?? ""),
      name: String(r.name ?? ""),
      presentation: (r.presentation as string) ?? null,
      position: Number(r.position ?? 0),
      filterable: Boolean(r.filterable),
      createdAtUtc: String(r.createdAtUtc ?? ""),
      modifiedAtUtc: (r.modifiedAtUtc as string) ?? null,
    };
  },
};
