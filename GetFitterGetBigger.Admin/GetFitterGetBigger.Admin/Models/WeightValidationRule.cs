namespace GetFitterGetBigger.Admin.Models
{
    public class WeightValidationRule
    {
        public static bool ValidateWeight(string weightTypeCode, decimal? weight)
        {
            return weightTypeCode switch
            {
                "BODYWEIGHT_ONLY" => weight == null || weight == 0,
                "NO_WEIGHT" => weight == null || weight == 0,
                "BODYWEIGHT_OPTIONAL" => weight == null || weight >= 0,
                "WEIGHT_REQUIRED" => weight > 0,
                "MACHINE_WEIGHT" => weight > 0,
                _ => false
            };
        }

        public static string GetValidationMessage(string weightTypeCode)
        {
            return weightTypeCode switch
            {
                "BODYWEIGHT_ONLY" => "This exercise uses bodyweight only. Weight cannot be specified.",
                "NO_WEIGHT" => "This exercise does not use weights. Weight cannot be specified.",
                "BODYWEIGHT_OPTIONAL" => "This exercise can be done with or without additional weight.",
                "WEIGHT_REQUIRED" => "This exercise requires a weight to be specified.",
                "MACHINE_WEIGHT" => "This exercise requires a machine weight to be specified.",
                _ => "Unknown weight type."
            };
        }

        public static bool RequiresWeightInput(string weightTypeCode)
        {
            return weightTypeCode switch
            {
                "BODYWEIGHT_ONLY" => false,
                "NO_WEIGHT" => false,
                "BODYWEIGHT_OPTIONAL" => true,
                "WEIGHT_REQUIRED" => true,
                "MACHINE_WEIGHT" => true,
                _ => false
            };
        }
    }
}