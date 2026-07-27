using LuxSalon.Model.Access;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface IUserService : IBaseCRUDService<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>
    {
        Task<UserSensitveResponse?> GetByUsernameAsync(string username);
        Task<UserResponse?> GetWithRoleByIdAsync(int id);
        Task ChangePasswordAsync(UserPasswordChangeRequest request);

        /// <summary>
        /// Pregled klijenata (korisnici sa rolom "Customer") za desktop "Klijenti" ekran -
        /// ime i prezime, email, broj zakazanih termina, datum posljednjeg termina.
        /// </summary>
        Task<PageResult<KlijentPregledResponse>> GetKlijentiAsync(UserSearch search);
    }
}
