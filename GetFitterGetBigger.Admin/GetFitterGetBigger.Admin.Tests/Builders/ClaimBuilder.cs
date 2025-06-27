using GetFitterGetBigger.Admin.Models.Authentication;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ClaimBuilder
    {
        private string? _claimId = "claim-1";
        private string? _claimType = "Admin-Tier";
        private string? _expirationDate = "2025-12-31";
        private string? _resource = "GetFitterGetBigger";

        public ClaimBuilder WithId(string? id)
        {
            _claimId = id;
            return this;
        }

        public ClaimBuilder WithType(string? type)
        {
            _claimType = type;
            return this;
        }

        public ClaimBuilder WithExpirationDate(string? expirationDate)
        {
            _expirationDate = expirationDate;
            return this;
        }

        public ClaimBuilder WithResource(string? resource)
        {
            _resource = resource;
            return this;
        }

        public ClaimBuilder AsAdminTier()
        {
            _claimType = "Admin-Tier";
            return this;
        }

        public ClaimBuilder AsPTTier()
        {
            _claimType = "PT-Tier";
            return this;
        }

        public ClaimBuilder AsFreeTier()
        {
            _claimType = "Free-Tier";
            return this;
        }

        public Claim Build()
        {
            return new Claim
            {
                ClaimId = _claimId,
                ClaimType = _claimType,
                ExpirationDate = _expirationDate,
                Resource = _resource
            };
        }

        public static ClaimResponse BuildClaimResponse(params Claim[] claims)
        {
            return new ClaimResponse
            {
                Claims = claims.ToList()
            };
        }
    }
}