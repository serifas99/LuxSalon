using System.ComponentModel.DataAnnotations;

namespace LuxSalon.Model.Requests
{
    public class UserUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Nullable namjerno: mobilna app (samostalno uređivanje profila) ne šalje ovo polje
        /// uopšte, pa ga backend (uz IgnoreNullValues Mapster config) mora ignorisati umjesto
        /// da ga tumači kao "false" i time nehotice deaktivira korisnika koji uredi svoj profil.
        /// Desktop (admin uređivanje korisnika) i dalje eksplicitno šalje true/false.
        /// </summary>
        public bool? IsActive { get; set; }
        public string? ProfileImageBase64 { get; set; }
    }
}
