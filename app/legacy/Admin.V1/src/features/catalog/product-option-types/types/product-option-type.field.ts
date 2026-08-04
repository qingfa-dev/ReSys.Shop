import { z } from "zod";

export function createProductOptionTypeSchema(
  t: (key: string, args?: Record<string, unknown>) => string,
) {
  return z.object({
    optionTypeIds: z.array(z.string().uuid()).min(1, t("catalog.validation.option_type_ids.min")),
  });
}

export type ProductOptionTypeFormSchema = z.infer<ReturnType<typeof createProductOptionTypeSchema>>;
