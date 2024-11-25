namespace TechChallenge.API.Infra;

public static class HandlersMediatR
{
    public static void AddHandlersMediatR(this IServiceCollection service)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
        .ToArray();

        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
    }
}
