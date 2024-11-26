## API baseada em ASP.NET Core e Entity Framework Core com autenticação JWT.

---

### 1. **Configuração Principal - `Program.cs`**

#### Funções:
- Configurações do **Entity Framework** para o banco de dados MySQL.
- Configuração de **JWT** para autenticação e geração de tokens.
- Definição de endpoints para autenticação, CRUD de veículos e integração com o Swagger.

**Destaques do código:**
- Uso de `AddDbContext` para configurar o banco de dados com `UseMySql`.
- Configuração de autenticação JWT com `TokenValidationParameters`.
- Geração de token JWT para autenticação de administradores.
- Endpoints implementados como *Minimal APIs* com restrições de autorização usando `RequireAuthorization`.

---

### 2. **Contexto do Banco de Dados - `ApplicationDbContext`**

#### Funções:
- Representa o banco de dados.
- Contém `DbSet` para tabelas de veículos (`Vehicles`) e administradores (`Administrators`).
- Configura o banco de dados no método `OnConfiguring` para usar strings de conexão do `appsettings.json`.

**Destaques do código:**
- Inicialização de dados (`HasData`) para criar um administrador padrão.
- `OnConfiguring` para configurar a conexão apenas se ela não estiver configurada.

---

### 3. **Entidades**

#### a. `Administrator`
- Representa os administradores do sistema.
- Campos com validações (`[Required]`, `[StringLength]`) e chaves primárias (`[Key]`).

#### b. `Vehicle`
- Representa veículos registrados no sistema.
- Campos como `Mark`, `Model`, e `Year` com validações.

---

### 4. **Serviços**

#### a. `IAdministratorService` e `AdministratorService`
- **Interface:** Define métodos como autenticação, obtenção de administradores por ID e paginação.
- **Implementação:** Contém a lógica para autenticar, listar e salvar administradores no banco de dados.

#### b. `IVehicleService` e `VehicleService`
- **Interface:** Define métodos como adicionar, atualizar, excluir e listar veículos.
- **Implementação:** Lógica de CRUD (Create, Read, Update, Delete) para veículos com paginação e filtros opcionais.

---

### 5. **Endpoints**

- **Login:**
  - Autentica administradores e retorna um token JWT.
- **Veículos:**
  - **GET `/vehicles`:** Lista todos os veículos (com autenticação).
  - **POST `/vehicles`:** Adiciona um novo veículo.
  - **PUT `/vehicles/{id}`:** Atualiza um veículo existente.
  - **DELETE `/vehicles/{id}`:** Remove um veículo pelo ID.

---

### 6. **Configurações do Arquivo `appsettings.json`**

- Contém:
  - String de conexão com o banco MySQL (`DefaultConnection`).
  - Chave e emissor do JWT (`Jwt:Key`, `Jwt:Issuer`).

---

### Comentários Sobre o Código

#### **Boas Práticas**
1. **Separation of Concerns (SoC):**
   - A lógica de negócios foi separada em serviços (`VehicleService`, `AdministratorService`).
   - As configurações de autenticação e banco estão isoladas.

2. **Validação e Configuração:**
   - Validações explícitas nos modelos garantem a consistência dos dados.
   - Configuração condicional em `OnConfiguring` evita duplicidade.

3. **Segurança:**
   - Uso de JWT para autenticação e autorização.
   - Senha de administrador no banco em texto claro **não é ideal**, mas pode ser ajustada com hashing no futuro.

---

#### **Melhorias Recomendadas**
1. **Hash de Senha:**
   - Use bibliotecas como `BCrypt` para armazenar senhas de forma segura.
   - Exemplo:
     ```csharp
     var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
     ```

2. **Paginação e Filtros em `Vehicles`:**
   - A API suporta filtros básicos, mas você pode expandir o suporte a consultas dinâmicas.

3. **Validação de Token:**
   - Adicione validação de expirados ao usar JWT.

4. **Mensagens de Erro Amigáveis:**
   - Substitua respostas genéricas como `Results.NotFound()` por mensagens mais informativas.

5. **Testes Unitários:**
   - Inclua testes para os serviços e endpoints.

---
# Prática:
---

### **1. Contexto do Banco de Dados - `ApplicationDbContext`**

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration? _configAppSettings;
    public ApplicationDbContext(IConfiguration configAppSettings)
    {
        _configAppSettings = configAppSettings;
    }

    public DbSet<Vehicle> Vehicles { get; set; } // Representa a tabela 'Vehicles' no banco de dados.
    public DbSet<Administrator> Administrators { get; set; } // Representa a tabela 'Administrators'.

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        // Adiciona dados iniciais na tabela Administrators.
        modelBuilder.Entity<Administrator>().HasData(
            new Administrator{
                Id = 1,
                Email = "admin@admin",
                Password = "123456", // Idealmente, deveria estar hashada para segurança.
                Name = "Admin",
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configAppSettings!.GetConnectionString("MySql")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(
                    stringConexao, // Obtém a string de conexão do `appsettings.json`.
                    ServerVersion.AutoDetect(stringConexao) // Detecta a versão do MySQL automaticamente.
                );
            }
        }
    }
}
```

#### **O que faz?**
- **`DbSet<Vehicle>` e `DbSet<Administrator>`:** Representam tabelas do banco de dados.
- **`OnConfiguring`:** Configura a conexão ao banco de dados.
- **`OnModelCreating`:** Adiciona um administrador inicial automaticamente no banco.

---

### **2. Entidades**

#### **Administrator**
```csharp
public class Administrator {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get;set; } = default!; // Chave primária, gerada automaticamente.

    [Required]
    [StringLength(255)]
    public string Email { get;set; } = default!; // E-mail do administrador.

    [Required]
    [StringLength(50)]
    public string Password { get;set; } = default!; // Senha (idealmente, usar hashing).

    [Required]
    [StringLength(10)]
    public string Name { get;set; } = default!; // Nome do administrador.
}
```

#### **Vehicle**
```csharp
public class Vehicle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } // Chave primária.

    [Required]
    [StringLength(150)]
    public string? Mark { get; set; } // Marca do veículo.

    [Required]
    [StringLength(150)]
    public string? Model { get; set; } // Modelo do veículo.

    [Required]
    public int Year { get; set; } // Ano de fabricação.
}
```

#### **O que fazem?**
Essas classes representam as tabelas no banco de dados:
- **Administrator:** Tabela para administradores.
- **Vehicle:** Tabela para veículos.

---

### **3. Serviços**

#### **Interfaces**

##### Administradores
```csharp
public interface IAdministratorService
{
    Administrator? Auth(AdminLogin login); // Autentica o administrador.
    Administrator Post(Administrator administrator); // Adiciona um novo administrador.
    Administrator? GetPerId(int id); // Retorna um administrador pelo ID.
    List<Administrator> AllAdmin(int? pagina); // Lista todos os administradores, com paginação.
}
```

##### Veículos
```csharp
public interface IVehicleService
{
    List<Vehicle> AllVehicles(int? page = 1, string? Model = null, string? Mark = null); // Lista veículos, com filtros opcionais.
    Vehicle? GetPerId(int id); // Retorna um veículo pelo ID.
    void Post(Vehicle vehicle); // Adiciona um novo veículo.
    void UpdateVehicle(Vehicle vehicle); // Atualiza um veículo existente.
    void Delete(Vehicle vehicle); // Remove um veículo.
}
```

#### **Implementações**

##### Administradores
```csharp
public class AdministratorService : IAdministratorService
{
    private readonly ApplicationDbContext _contexto;
    public AdministratorService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public Administrator? Auth(AdminLogin login)
    {
        // Busca um administrador com o e-mail e senha fornecidos.
        return _contexto.Administrators.FirstOrDefault(a => a.Email == login.Email && a.Password == login.Password);
    }

    public Administrator Post(Administrator administrator)
    {
        _contexto.Administrators.Add(administrator);
        _contexto.SaveChanges();
        return administrator;
    }

    public Administrator? GetPerId(int id)
    {
        return _contexto.Administrators.FirstOrDefault(v => v.Id == id);
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
```

##### Veículos
```csharp
public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _contexto;
    public VehicleService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Post(Vehicle vehicle)
    {
        _contexto.Vehicles.Add(vehicle);
        _contexto.SaveChanges();
    }

    public void UpdateVehicle(Vehicle vehicle)
    {
        _contexto.Vehicles.Update(vehicle);
        _contexto.SaveChanges();
    }

    public void Delete(Vehicle vehicle)
    {
        _contexto.Vehicles.Remove(vehicle);
        _contexto.SaveChanges();
    }

    public Vehicle? GetPerId(int id)
    {
        return _contexto.Vehicles.FirstOrDefault(v => v.Id == id);
    }

    public List<Vehicle> AllVehicles(int? page = 1, string? Model = null, string? Mark = null)
    {
        var query = _contexto.Vehicles.AsQueryable();

        if (!string.IsNullOrEmpty(Model))
            query = query.Where(v => EF.Functions.Like(v.Model!.ToLower(), $"%{Model}%"));

        if (!string.IsNullOrEmpty(Mark))
            query = query.Where(v => EF.Functions.Like(v.Mark!.ToLower(), $"%{Mark}%"));

        int itemsPerPage = 10;
        if (page != null)
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);

        return query.ToList();
    }
}
```

#### **O que fazem?**
- Serviços encapsulam a lógica de acesso ao banco.
- `AdministratorService`: Lida com autenticação e CRUD de administradores.
- `VehicleService`: Lida com CRUD e filtros de veículos.

---

### **4. Configuração Principal - `Program.cs`**

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 25))));
```
- Configura o **Entity Framework Core** com MySQL.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidIssuer = jwtIssuer
        };
    });
```
- Configura o **JWT** para autenticação.

```csharp
app.UseAuthentication();
app.UseAuthorization();
```
- Habilita autenticação e autorização no middleware.

```csharp
app.MapPost("/login", async (string email, string password, ApplicationDbContext context) =>
{
    var admin = await context.Administrators.FirstOrDefaultAsync(a => a.Email == email && a.Password == password);
    if (admin is null) return Results.Unauthorized();
    var token = GenerateJwtToken(admin, jwtKey, jwtIssuer);
    return Results.Ok(new { Token = token });
});
```
- Endpoint `/login` gera tokens JWT.

---

### **Resumo**

- **Endpoints Minimal API:** Fornecem acesso a CRUD de veículos.
- **Autenticação JWT:** Assegura que apenas usuários autorizados acessem endpoints protegidos.
- **Banco de Dados:** Configurado com Entity Framework Core para MySQL.