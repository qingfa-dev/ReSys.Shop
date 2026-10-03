export enum PropertyKind {
  String = 0,
  Integer = 1,
  Float = 2,
  Boolean = 3,
  Date = 4,
  Html = 5,
}

export const PropertyKindOptions = [
  { label: 'String', value: PropertyKind.String },
  { label: 'Integer', value: PropertyKind.Integer },
  { label: 'Float', value: PropertyKind.Float },
  { label: 'Boolean', value: PropertyKind.Boolean },
  { label: 'Date', value: PropertyKind.Date },
  { label: 'HTML', value: PropertyKind.Html },
]
