using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using PatientBillingAPI.Data;
using PatientBillingAPI.Dtos;
using PatientBillingAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db
builder.Services.AddDbContext<BillingDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("BillingDb"));
});

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// GET /patients
app.MapGet("/patients", async (BillingDbContext db) =>
{
    var patients = await db.Patients
        .AsNoTracking()
        .Select(p=> new PatientReadDto(p.Id, p.Name, p.DateOfBirth))
        .ToListAsync();

        return Results.Ok(patients);
});

// GET /patients{id}
app.MapGet("/patients/{id:int}", async (int id, BillingDbContext db) =>
{
    var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    if (patient is null) return Results.NotFound(new { message = "Patuient not found."});

    return Results.Ok(new PatientReadDto(patient.Id, patient.Name, patient.DateOfBirth));
});

// GET /patients/{id}/invoices
app.MapGet("/patients/{id::int}/invoices", async (int id, BillingDbContext db) =>
{
    var patientExists = await db.Patients.AnyAsync(p => p.Id == id);
    if (!patientExists) return Results.NotFound(new { message = "Patient not found." });

    var invoices = await db.Invoices    
        .AsNoTracking()
        .Where(i => i.PatientId == id)
        .OrderByDescending(i => i.CreatedAtUtc)
        .Select(i => new InvoiceReadDto(i.Id, i.PatientId, i.Amount, i.Status, i.CreatedAtUtc, i.PaidAtUtc))
        .ToListAsync();

    return Results.Ok(invoices);
});

// POST /patients
app.MapPost("/patients", async(PatientCreateDto dto, BillingDbContext db) =>
{
    if(string.IsNullOrWhiteSpace(dto.Name))
    return Results.BadRequest(new { message = "Name is required."});

    var patient = new Patient
    {
        Name = dto.Name.Trim(),
        DateOfBirth = dto.DateOfBirth
    };

    db.Patients.Add(patient);
    await db.SaveChangesAsync();

    return Results.Created($"/patients/{patient.Id}",
        new PatientReadDto(patient.Id, patient.Name, patient.DateOfBirth));
});

// GET /invoices?status=Pending&patientId=1&minAmount=10&maxAmount=500&from=2026-01-01to2026-12-31
app.MapGet("/invoices", async (
    BillingDbContext db,
    string? status,
    int? patientId,
    decimal? minAmount,
    decimal? maxAmount,
    DateTime? from,
    DateTime? to
    ) =>
{
    IQueryable<Invoice> query = db.Invoices.AsNoTracking();

    if (patientId is not null)
        query = query.Where(i => i.PatientId == patientId.Value);
    
    if(!string.IsNullOrWhiteSpace(status))
    {
        if(!Enum.TryParse<InvoiceStatus>(status, ignoreCase: true, out var parsedStatus))
            return Results.BadRequest(new { message = "Invalid status. Use Pending or Paid."});

        query = query.Where(i => i.Status == parsedStatus);
            
    }

    if (minAmount is not null)
        query = query.Where(i => i.Amount >= minAmount.Value);

    if(maxAmount is not null)
        query = query.Where(i => i.Amount <= maxAmount.Value);

    if (from is not null)
        query = query.Where(i => i.CreatedAtUtc >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));

    if (to is not null)
        query = query.Where(i => i.CreatedAtUtc >= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));        

    var invoices = await query
        .OrderByDescending(i => i.CreatedAtUtc)
        .Select(i => new InvoiceReadDto(i.Id, i.PatientId, i.Amount, i.Status, i.CreatedAtUtc, i.PaidAtUtc))
        .ToListAsync();
    return Results.Ok(invoices);
});

// GET /invoices/{id}
app.MapGet("/invoices/{id:int}", async(int id, BillingDbContext db) =>
{
    var invoice = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    if (invoice is null) return Results.NotFound(new { message = "Invoice not found." });

    return Results.Ok(new InvoiceReadDto(invoice.Id, invoice.PatientId, invoice.Amount, invoice.Status, invoice.CreatedAtUtc, invoice.PaidAtUtc));
});

// POST /invoices
app.MapPost("/invoices", async (InvoiceCreateDto dto, BillingDbContext db) =>
{
    if(dto.Amount <= 0)
        return Results.BadRequest(new { message = "Amount must be greater than 0."});
    
    var patientExists = await db.Patients.AnyAsync(p => p.Id == dto.PatientId);
    if (!patientExists)
        return Results.BadRequest(new { message = "PatientID does not exist."});
    
    var invoice = new Invoice
    {
        PatientId = dto.PatientId,
        Amount = dto.Amount,
        Status = InvoiceStatus.Pending
    };

    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();

    return Results.Created($"/invoices/{invoice.Id}",
        new InvoiceReadDto(invoice.Id, invoice.PatientId, invoice.Amount, invoice.Status, invoice.CreatedAtUtc, invoice.PaidAtUtc));
});

// PUT /invoices/{id:int}/pay
app.MapPut("/invoices/{id:int}/pay", async (int id, BillingDbContext db) =>
{
    var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
    if (invoice is null) return Results.NotFound(new { message = "Invoice not found." });

    if (invoice.Status == InvoiceStatus.Paid)
        return Results.BadRequest(new { message = "Invoice already paid." });

    invoice.Status = InvoiceStatus.Paid;
    invoice.PaidAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new InvoiceReadDto(invoice.Id, invoice.PatientId, invoice.Amount, invoice.Status, invoice.CreatedAtUtc, invoice.PaidAtUtc));
});

app.Run();

public partial class Program {}