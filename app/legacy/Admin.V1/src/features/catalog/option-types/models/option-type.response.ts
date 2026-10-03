export interface OptionTypeListItem {
  id: string;
  name: string;
  presentation: string | null;
  position: number;
  filterable: boolean;
  optionValuesCount: number;
  productsCount: number;
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}

export interface OptionTypeDetail {
  id: string;
  name: string;
  presentation: string | null;
  position: number;
  filterable: boolean;
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}
