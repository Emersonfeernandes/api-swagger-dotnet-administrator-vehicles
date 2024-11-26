using Microsoft.EntityFrameworkCore;
namespace VeiculosEstacionamento.Infra.Interfaces;
public class AdministratorService : IAdministratorService
{
    private readonly ApplicationDbContext _contexto;
    public AdministratorService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public Administrator? GetPerId(int id)
    {
        return _contexto.Administrators.Where(v => v.Id == id).FirstOrDefault();
    }

    public Administrator Post(Administrator administrator)
    {
        _contexto.Administrators.Add(administrator);
        _contexto.SaveChanges();

        return administrator;
    }

    public Administrator? Auth(AdminLogin login)
    {
        var adm = _contexto.Administrators.Where(a => a.Email == login.Email && a.Password == login.Password).FirstOrDefault();
        return adm;
    }

    public List<Administrator> AllAdmin(int? page)
    {
        var query = _contexto.Administrators.AsQueryable();

        int itemsPerPage = 10;

        if(page != null)
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);

        return query.ToList();
    }
}