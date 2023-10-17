using System.CommandLine.Builder;
using Microsoft.Build.Construction;
using Microsoft.Extensions.DependencyInjection;

namespace EvoSC.Tool.Utils;

public static class DependencyInjectionUtils
{
    public static CommandLineBuilder AddDependencyInjection(this CommandLineBuilder builder, Action<IServiceCollection> servicesAction)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            var services = new ServiceCollection();
            servicesAction(services);

            services.AddSingleton(context.Console);
            services.AddSingleton(context.BindingContext.GetService<SolutionFile>());

            await using var serviceProvider = services.BuildServiceProvider();

            context.BindingContext.AddService<IServiceProvider>(_ => serviceProvider);

            var uniqueServiceTypes = new HashSet<Type>(services.Select(s => s.ServiceType));
            foreach (var type in uniqueServiceTypes)
            {
                context.BindingContext.AddService(type, _ => serviceProvider.GetRequiredService(type));
                var enumType = typeof(IEnumerable<>).MakeGenericType(type);
                context.BindingContext.AddService(enumType, _ => serviceProvider.GetServices(type));
            }
   
            await next(context);
        });

        return builder;
    }
}
