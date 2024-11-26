
namespace VeiculosEstacionamento.Infra.Interfaces;

public interface IVehicleService
{
    List<Vehicle> AllVehicles(int? page = 1, string? Model = null, string? Mark = null);
    Vehicle? GetPerId(int id);
    void Post(Vehicle vehicle);
    void UpdateVehicle(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}