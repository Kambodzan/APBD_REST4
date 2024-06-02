    using APBD_REST4.Data;
    using APBD_REST4.Repositories;
    using APBD_REST4.Services;
    using Microsoft.EntityFrameworkCore;

    var builder = WebApplication.CreateBuilder();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    builder.Services.AddScoped<ITripRepositories, TripRepositories>();
    builder.Services.AddScoped<ITripService, TripService>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    app.Run();
