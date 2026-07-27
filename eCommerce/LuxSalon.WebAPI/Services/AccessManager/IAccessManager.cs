using LuxSalon.Model.Access;

namespace LuxSalon.WebAPI.Services.AccessManager
{
    public interface IAccessManager
    {
        Task<UserLoginResponse> LoginAsync(UserLoginRequest request);
        Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
