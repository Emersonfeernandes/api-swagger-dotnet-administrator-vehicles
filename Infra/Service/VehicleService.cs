using Microsoft.EntityFrameworkCore;
namespace VeiculosEstacionamento.Infra.Interfaces;


public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _contexto;
    public VehicleService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Delete(Vehicle vehicle)
    {
        _contexto.Vehicles.Remove(vehicle);
        _contexto.SaveChanges();
    }

    public void UpdateVehicle(Vehicle vehicle)
    {
        _contexto.Vehicles.Update(vehicle);
        _contexto.SaveChanges();
    }

    public Vehicle? GetPerId(int id)
    {
        return _contexto.Vehicles.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Post(Vehicle vehicle)
    {
        _contexto.Vehicles.Add(vehicle);
        _contexto.SaveChanges();
    }

    public List<Vehicle> AllVehicles(int? page = 1, string? Model = null, string? Mark = null)
    {
        var query = _contexto.Vehicles.AsQueryable();
        if(!string.IsNullOrEmpty(Model))
        {
            query = query.Where(v => EF.Functions.Like(v.Model!.ToLower(), $"%{Model}%"));
        }

        int itemsPerPage = 10;

        if(page != null)
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);

        return query.ToList();
    }
}