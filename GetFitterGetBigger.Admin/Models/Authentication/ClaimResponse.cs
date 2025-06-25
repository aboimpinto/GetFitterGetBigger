namespace GetFitterGetBigger.Admin.Models.Authentication;

using System.Collections.Generic;

public class ClaimResponse
{
    public string? Token { get; set; }
    public List<GetFitterGetBigger.Admin.Models.Authentication.Claim>? Claims { get; set; }
}
