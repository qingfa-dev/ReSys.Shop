import { z } from 'zod';
import { PropertyKind } from '../types/property-type.domain.types';

export const PropertyTypeSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  kind: z.nativeEnum(PropertyKind).default(PropertyKind.String),
  position: z.number().int().min(0).default(0),
  filterable: z.boolean().default(false),
});

export type PropertyTypeFormData = z.infer<typeof PropertyTypeSchema>;
