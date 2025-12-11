using ProcessImage.Models.DTO;
using ProcessImage.Models;

namespace ProcessImage.Services.Interface
{
    public interface IUtilizatorService
    {
        Task<object> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<ProfilResponse> GetProfilAsync(int userId);
        Task UpdateProfilAsync(int userId, UpdateProfilRequest request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }


}
