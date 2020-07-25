using Microsoft.Extensions.DependencyInjection;
using OpenSpark.ApiGateway.Services;
using ActorSystemService = OpenSpark.ApiGateway.Services.ActorSystemService;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Transient
            services.AddTransient<IMessageContextBuilderService, MessageContextBuilderService>();

            // Singletons
            services.AddSingleton<IFirestoreService, FirestoreService>();
            services.AddSingleton<IEventEmitterService, EventEmitterService>();
            services.AddSingleton<IActorSystemService, ActorSystemService>();
        }
    }
}