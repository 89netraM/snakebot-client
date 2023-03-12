using Mårten.HamiltonPaths;
using Microsoft.Extensions.DependencyInjection;
using Zarya;
using Zarya.Silk.NET;
using Zarya.SkiaSharp;

var builder = GameBuilder.Create();

builder.AddSilkWindow();
builder.AddSkiaSharpRenderer();

builder.Services.AddSingleton<Map>();
builder.Services.AddTransient<MapObject>();

var game = builder.Build<SilkWindow>();
game.GameManager.Create<MapObject>();
game.Run();
