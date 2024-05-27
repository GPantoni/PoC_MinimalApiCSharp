using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapGet("frases",
    async () => await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

app.MapGet("/tarefas", async (AppDbContext dbContext) => await dbContext.Tarefas.ToListAsync());

app.MapGet("/tarefas/{id}",
    async (AppDbContext db, int id) =>
        await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

app.MapGet("/tarefas/concluida", async (AppDbContext db) => await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

app.MapPost("/tarefas", async (AppDbContext db, Tarefa tarefa) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (AppDbContext db, int id, Tarefa inputTarefa) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    
    if (tarefa is null)
        return Results.NotFound();

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (AppDbContext db, int id) =>
{
    if (await db.Tarefas.FindAsync(id) is not Tarefa tarefa)
        return Results.NotFound();
    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();
    return Results.Ok(tarefa);

});


app.Run();

internal class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}