using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mårten.Snake.Services.Controllers;

public static class ControllerFactory
{
	private static readonly IReadOnlyDictionary<string, Type> controllers = new Dictionary<string, Type>
	{
		["Edge"] = typeof(EdgeController),
		["Food"] = typeof(FoodController),
		["Nibler"] = typeof(NiblerController),
	};

	public static ControllerBase Factory(IServiceProvider services)
	{
		var options = services.GetRequiredService<IOptions<GameOptions>>().Value;
		return (ControllerBase)ActivatorUtilities.CreateInstance(services, controllers[options.Controller]);
	}
}
