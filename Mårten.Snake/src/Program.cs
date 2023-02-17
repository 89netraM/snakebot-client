using System;
using Mårten.Snake;
using Mårten.Snake.GameObjects;
using Mårten.Snake.Services;
using Mårten.Snake.Services.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zarya;
using Zarya.Silk.NET;
using Zarya.SkiaSharp;

var config = new ConfigurationBuilder()
	.SetBasePath(Environment.CurrentDirectory)
	.AddJsonFile("appsettings.json")
	.AddEnvironmentVariables()
	.AddCommandLine(args)
	.Build();

var builder = GameBuilder.Create();
builder.Services.AddLogging(logging => logging
	.AddConfiguration(config.GetSection("Logging"))
	.AddConsole());

builder.AddSilkWindow();
builder.AddSkiaSharpRenderer();

builder.Services.Configure<GameOptions>(config.GetSection("SnakeBot"));
builder.Services.AddSingleton<ClientService>();
builder.Services.AddSingleton<PlayerInfoService>();
builder.Services.AddSingleton<MapInfoService>();
builder.Services.AddSingleton<SnakeColor>();
builder.Services.AddSingleton<SpritePool>();
builder.Services.AddSingleton<InputService>();
builder.Services.AddSingleton<ControllerBase>(ControllerFactory.Factory);

var game = builder.Build<SilkWindow>();
game.GameManager.Create<StarterObject>();
game.GameManager.Create<MapObject>();
game.Run();
