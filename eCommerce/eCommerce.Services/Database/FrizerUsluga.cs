using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Services.Database
{
    // Junction entitet: koje usluge koji frizer moze izvoditi (many-to-many)
    public class FrizerUsluga
    {
        [Key]
        public int Id { get; set; }

        public int FrizerId { get; set; }

        [ForeignKey("FrizerId")]
        public Frizer Frizer { get; set; } = null!;

        public int UslugaId { get; set; }

        [ForeignKey("UslugaId")]
        public Usluga Usluga { get; set; } = null!;
    }
}
