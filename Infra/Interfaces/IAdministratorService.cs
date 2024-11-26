namespace VeiculosEstacionamento.Infra.Interfaces;

public interface IAdministratorService
{
    Administrator? Auth(AdminLogin login);
    Administrator Post(Administrator administrator);
    Administrator? GetPerId(int id);
    List<Administrator> AllAdmin(int? pagina);
}