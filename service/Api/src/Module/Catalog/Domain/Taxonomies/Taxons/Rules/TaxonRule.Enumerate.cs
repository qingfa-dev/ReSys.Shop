using System.Runtime.Serialization;

namespace Module.Catalog.Domain.Taxonomies.Taxons.Rules;

public enum TaxonRuleMatchPolicy
{
    [EnumMember(Value = "is_equal_to")] IsEqualTo,
    [EnumMember(Value = "is_not_equal_to")] IsNotEqualTo,
    [EnumMember(Value = "contains")] Contains,
    [EnumMember(Value = "does_not_contain")] DoesNotContain,
    [EnumMember(Value = "starts_with")] StartsWith,
    [EnumMember(Value = "ends_with")] EndsWith,
    [EnumMember(Value = "greater_than")] GreaterThan,
    [EnumMember(Value = "less_than")] LessThan,
    [EnumMember(Value = "greater_than_or_equal")] GreaterThanOrEqual,
    [EnumMember(Value = "less_than_or_equal")] LessThanOrEqual,
    [EnumMember(Value = "in")] In,
    [EnumMember(Value = "not_in")] NotIn,
    [EnumMember(Value = "is_null")] IsNull,
    [EnumMember(Value = "is_not_null")] IsNotNull
}

public enum TaxonRuleType
{
    [EnumMember(Value = "product_name")] ProductName,
    [EnumMember(Value = "product_sku")] ProductSku,
    [EnumMember(Value = "product_description")] ProductDescription,
    [EnumMember(Value = "product_price")] ProductPrice,
    [EnumMember(Value = "product_weight")] ProductWeight,
    [EnumMember(Value = "product_available")] ProductAvailable,
    [EnumMember(Value = "product_archived")] ProductArchived,
    [EnumMember(Value = "variant_price")] VariantPrice,
    [EnumMember(Value = "variant_sku")] VariantSku,
    [EnumMember(Value = "product_status")] ProductStatus,
}