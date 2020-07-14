using Microsoft.Extensions.DependencyInjection;
using OpenSpark.ApiGateway.Services;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Singletons
            services.AddSingleton<IFirestoreService, FirestoreService>();
            services.AddSingleton<IEventEmitterService, EventEmitterService>();
            services.AddSingleton<IActorSystemService, ActorSystemService>();
            services.AddSingleton<IActorFactoryService, ActorFactoryService>();
        }
    }
}