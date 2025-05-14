using TravelAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//doodaj kontrolery (API)
builder.Services.AddControllers();

//swagger(dokumentacjai testowanie)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//rejestracja SqlConnectionFactory
builder.Services.AddScoped<SqlConnectionFactory>();

var app = builder.Build();


// wlacz Swagger tylko w trybie deweloperskim
app.UseSwagger();
app.UseSwaggerUI();

//HTTPS (mozna wylaczyć jesli niepotrzebne)
app.UseHttpsRedirection();

//autoryzacja
app.UseAuthorization();

//mapa endpointow 
app.MapControllers();

app.Run();