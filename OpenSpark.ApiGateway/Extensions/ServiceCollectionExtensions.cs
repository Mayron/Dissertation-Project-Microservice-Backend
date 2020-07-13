using Microsoft.Extensions.DependencyInjection;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.Services.SDK;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Singletons
            services.AddSingleton<IFirestoreService, FirestoreService>();
            services.AddSingleton<IEventEmitterService, EventEmitterService>();
            services.AddSingleton<ILocalActorSystemService, LocalActorSystemService>();
            services.AddSingleton<IRemoteActorSystemService, RemoteActorSystemService>();
        }
    }
}