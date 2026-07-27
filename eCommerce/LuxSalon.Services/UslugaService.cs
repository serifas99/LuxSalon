using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
{
    public class UslugaService : BaseCRUDService<Usluga, UslugaResponse, UslugaSearchObject, UslugaInsertRequest, UslugaUpdateRequest>, IUslugaService
    {
        public UslugaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UslugaInsertRequest> insertValidator, IValidator<UslugaUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        // Termin -> Usluga je DeleteBehavior.Restrict (namjerno - istorija termina se ne smije
        // izgubiti), pa bi hard delete usluge koja ima bilo kakav termin (i zavrsen i otkazan)
        // inace pao na FK constraint-u u SQL Serveru i zavrsio kao nejasan 500 "Server side error".
        // Ovdje se ta situacija prepoznaje unaprijed i vraca se jasna, ljudska poruka.
        public override async Task DeleteAsync(int id)
        {
            var imaTermina = await _dbContext.Termini.AnyAsync(t => t.UslugaId == id);
            if (imaTermina)
                throw new ClinetException("Ova usluga se ne moze obrisati jer postoje termini vezani za nju. Umjesto brisanja, deaktivirajte je (Aktivna = Ne).");

            await base.DeleteAsync(id);
        }

        protected override async Task<IQueryable<Usluga>> IncludeRelatedEntitiesAsync(UslugaSearchObject? search, IQueryable<Usluga> query = null)
        {
            query = query.Include(u => u.UslugaKategorija);
            return await Task.FromResult(query);
        }

        protected override IEnumerable<Usluga> ApplyFilters(IEnumerable<Usluga> query, UslugaSearchObject? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Naziv))
                {
                    query = query.Where(u => u.Naziv.Contains(search.Naziv, StringComparison.OrdinalIgnoreCase));
                }

                if (search.UslugaKategorijaId.HasValue)
                {
                    query = query.Where(u => u.UslugaKategorijaId == search.UslugaKategorijaId.Value);
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == search.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(search.Tag))
                {
                    query = query.Where(u => u.Tagovi.Contains(search.Tag, StringComparison.OrdinalIgnoreCase));
                }
            }

            return query;
        }
    }
}
