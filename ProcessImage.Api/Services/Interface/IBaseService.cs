namespace ProcessImage.Services.Interface
{
    public interface IBaseService
    {
        int GetUserId();
        int? GetUserIdOrNull();
        string GetUserEmail();
        int GetClaimAsInteger(string claimType);

    }
}
