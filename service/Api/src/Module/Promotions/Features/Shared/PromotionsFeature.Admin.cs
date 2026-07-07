using BuildingBlocks.Identity.Domain.AccessControls;
using BuildingBlocks.Identity.Domain.AccessControls.Stores;

namespace Module.Promotions.Features.Shared;

public static partial class PromotionsFeature
{
    public static class Admin
    {
        public const string Route = "api/promotions";

        public static class Promotions
        {
            public const string BaseRoute = $"{Route}/promotions";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new promotion";
                public const string Summary = "Create promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Create;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a promotion by identifier";
                public const string Summary = "Get promotion by ID";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Read;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve promotions with paging";
                public const string Summary = "Get all promotions";
                public static PermissionMetadata Permission => PermissionStore.Promotions.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a promotion";
                public const string Summary = "Update promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Soft-delete a promotion";
                public const string Summary = "Delete promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Delete;
            }

            public static class Activate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/activate";
                public const string Description = "Activate a promotion";
                public const string Summary = "Activate promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Update;
            }

            public static class Deactivate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/deactivate";
                public const string Description = "Deactivate a promotion";
                public const string Summary = "Deactivate promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Update;
            }

            public static class Duplicate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/duplicate";
                public const string Description = "Duplicate a promotion with its rules and actions";
                public const string Summary = "Duplicate promotion";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Create;
            }
        }

        public static class PromotionCategories
        {
            public const string BaseRoute = $"{Route}/categories";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a promotion category";
                public const string Summary = "Create promotion category";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve promotion categories";
                public const string Summary = "Get promotion categories";
                public static PermissionMetadata Permission => PermissionStore.Promotions.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a promotion category by identifier";
                public const string Summary = "Get promotion category by ID";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Read;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a promotion category";
                public const string Summary = "Update promotion category";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a promotion category";
                public const string Summary = "Delete promotion category";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Delete;
            }
        }

        public static class PromotionRules
        {
            public const string BaseRoute = $"{Route}/promotions/{{promotionId:guid}}/rules";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a promotion rule";
                public const string Summary = "Create promotion rule";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionRules.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all rules for a promotion";
                public const string Summary = "Get promotion rules";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionRules.Read;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{ruleId:guid}}";
                public const string Description = "Delete a promotion rule";
                public const string Summary = "Delete promotion rule";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionRules.Delete;
            }
        }

        public static class PromotionActions
        {
            public const string BaseRoute = $"{Route}/promotions/{{promotionId:guid}}/actions";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a promotion action";
                public const string Summary = "Create promotion action";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionActions.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all actions for a promotion";
                public const string Summary = "Get promotion actions";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionActions.Read;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{actionId:guid}}";
                public const string Description = "Delete a promotion action";
                public const string Summary = "Delete promotion action";
                public static PermissionMetadata Permission => PermissionStore.Promotions.PromotionActions.Delete;
            }
        }

        public static class CouponCodes
        {
            public const string BaseRoute = $"{Route}/coupon-codes";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a coupon code for a promotion";
                public const string Summary = "Create coupon code";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve coupon codes";
                public const string Summary = "Get coupon codes";
                public static PermissionMetadata Permission => PermissionStore.Promotions.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a coupon code by identifier";
                public const string Summary = "Get coupon code by ID";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Read;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Cancel a coupon code";
                public const string Summary = "Cancel coupon code";
                public static PermissionMetadata Permission => PermissionStore.Promotions.Delete;
            }
        }
    }
}
