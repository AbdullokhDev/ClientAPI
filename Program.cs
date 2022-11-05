using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.AddSimpleConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect to PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClientDb>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Welcome endpoint
app.MapGet("/", () => "Welcome to Clients API!");

// Create new client
app.MapPost("/clients/", async(Klient n, ClientDb db)=> {
    db.Clients.Add(n);
    await db.SaveChangesAsync();

    return Results.Created($"/clients/{n.id}", n);
});

// Get list of clients
app.MapGet("/clients", async (ClientDb db) => await db.Clients.ToListAsync());

// Get single client by id
app.MapGet("/clients/{id:int}", async(int id, ClientDb db)=> 
{
    return await db.Clients.FindAsync(id)
            is Klient n
                ? Results.Ok(n)
                : Results.NotFound();
});

// Update client
app.MapPut("/clients/{id:int}", async(int id, Klient n, ClientDb db)=>
{
    if (n.id != id)
    {
        return Results.BadRequest();
    }

    var client = await db.Clients.FindAsync(id);
    
    if (client is null) return Results.NotFound();

    //found, so update with incoming note n.
    client.firstName = n.firstName;
    client.lastName = n.lastName;
    client.email = n.email;
    client.phoneNumber = n.phoneNumber;
    client.address = n.address;
    await db.SaveChangesAsync();
    return Results.Ok(client);
});

// Delete client
app.MapDelete("/clients/{id:int}", async(int id, ClientDb db)=>{

    var note = await db.Clients.FindAsync(id);
    if (note is not null){
        db.Clients.Remove(note);
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
});


app.Run();

// Creating Database tables with Entity Framework (ORM)
record Klient(int id){
    public string firstName { get; set; } = default!;
    public string lastName { get; set; } = default!;
    public string phoneNumber { get; set; } = default!;
    public string email { get; set; } = default!;
    public string address { get; set; } = default!;
}

class ClientDb: DbContext {
    public ClientDb(DbContextOptions<ClientDb> options): base(options) {

    }
    public DbSet<Klient> Clients => Set<Klient>();
}
