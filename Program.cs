using GenAIExpertEngineAPI.Services;
using Microsoft.Extensions.Options; // Needed for IOptions in ExpertRegistryService
using System.Collections.Generic; // Needed for List<ExpertDefinition>
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using GenAIExpertEngineAPI;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration Setup ---
builder.Configuration.AddJsonFile("experts.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("gamesystem_ose.json", optional: false, reloadOnChange: true);

// --- Dependency Injection Setup ---
builder.Services.Configure<List<ExpertDefinition>>(builder.Configuration.GetSection("Experts"));
builder.Services.Configure<GameSystemConfiguration>(builder.Configuration.GetSection("GameSystemData"));

//register your core services.
builder.Services.AddSingleton<ExpertRegistryService>();
builder.Services.AddSingleton<GameSystemRegistryService>();
builder.Services.AddSingleton<CharacterState>();
builder.Services.AddSingleton<ConversationHistoryService>();
builder.Services.AddSingleton<ToolDefinitions>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<ICharacterStateManager, CharacterStateManager>();

builder.Services.AddScoped<OrchestratorService>();
builder.Services.AddScoped<RefereeService>();
builder.Services.AddScoped<CharacterStateManager>();

//services for API Controllers. This is essential.
builder.Services.AddControllers(options =>
{
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
}).AddJsonOptions(options =>
{
    // This line registers your source-generated context. This is the fix for the new error.
    options.JsonSerializerOptions.TypeInfoResolver = ApplicationJsonSerializerContext.Default;
});

//services for Swagger/OpenAPI. This gives you the test page.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// --- Middleware Pipeline Setup ---

//Configure the HTTP request pipeline to use Swagger in Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseAuthorization(); // Keep this for future security needs

//endpoint mapping for your controllers. This is essential.
app.MapControllers();


app.Run();

// This makes the internal Program class visible to your test project, as we discussed.
// Ensure this is at the VERY END of the file.
public partial class Program { }