namespace Module.Promotions.Domain.CouponCodes;

/// <summary>Contains success messages and error factory methods for CouponCode operations.</summary>
public static class CouponCodeResult
{
    /// <summary>Success message factory for CouponCode operations.</summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Coupon code with ID '{id}' was successfully created.";
        public static string Redeemed => "Coupon code was successfully redeemed.";
        public static string Expired => "Coupon code was successfully expired.";
        public static string Canceled => "Coupon code was successfully canceled.";
    }

    /// <summary>Error factory methods returning typed Failure instances for CouponCode operations.</summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Coupon code is required.</summary>
        public static Error CodeRequired => Error.Validation(
            code: "CouponCode.Code.Required",
            description: "Coupon code is required.");

        public static Error CodeTooLong => Error.Validation(
            code: "CouponCode.Code.TooLong",
            description: $"Coupon code cannot exceed {CouponCodeConstant.Constraints.MaxCodeLength} characters.");

        public static Error InvalidState => Error.Validation(
            code: "CouponCode.State.Invalid",
            description: $"Coupon code state is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<CouponCodeState>())}");
        #endregion Validation

        #region Business
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "CouponCode.NotFound",
            description: $"Coupon code with ID '{id}' was not found.");

        public static Error AlreadyRedeemed => Error.Conflict(
            code: "CouponCode.AlreadyRedeemed",
            description: "Coupon code has already been redeemed.");

        public static Error AlreadyExpired => Error.Conflict(
            code: "CouponCode.AlreadyExpired",
            description: "Coupon code has already been expired.");

        public static Error AlreadyCanceled => Error.Conflict(
            code: "CouponCode.AlreadyCanceled",
            description: "Coupon code has already been canceled.");

        public static Error Expired => Error.Conflict(
            code: "CouponCode.Expired",
            description: "Coupon code has expired and cannot be redeemed.");

        public static Error Canceled => Error.Conflict(
            code: "CouponCode.Canceled",
            description: "Coupon code was canceled and cannot be redeemed.");

        /// <summary>Coupon code with the given code string was not found.</summary>
        public static Error NotFoundByCode => Error.NotFound(
            code: "CouponCode.NotFoundByCode",
            description: "Coupon code is invalid or expired.");

        /// <summary>Coupon code has already been applied to this order.</summary>
        public static Error AlreadyAppliedToOrder => Error.Conflict(
            code: "CouponCode.AlreadyAppliedToOrder",
            description: "This coupon has already been applied.");

        /// <summary>Coupon conditions are not met by the current cart.</summary>
        public static Error RulesNotMet => Error.Validation(
            code: "CouponCode.RulesNotMet",
            description: "The coupon conditions are not met by your current cart.");
        #endregion Business
    }
}