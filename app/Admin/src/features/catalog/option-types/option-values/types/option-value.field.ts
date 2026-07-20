import { z } from "zod";

export function createOptionValueSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z
    .string()
    .min(1, t("catalog.validation.internal_name.required"))
    .max(100, t("catalog.validation.name.max_length")),
  presentation: z
    .string()
    .min(1, t("catalog.validation.display_name.required"))
    .max(100, t("catalog.validation.display_name.max_length")),
  position: z
    .number()
    .int(t("catalog.validation.position.whole"))
    .min(0, t("catalog.validation.position.min"))
    .default(0),
});
}

export type OptionValueParameters = z.infer<ReturnType<typeof createOptionValueSchema>>;
