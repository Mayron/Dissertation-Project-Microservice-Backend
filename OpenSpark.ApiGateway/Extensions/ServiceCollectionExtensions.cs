using Microsoft.Extensions.DependencyInjection;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Transient
            services.AddTransient<IMessageContextBuilderFactory, MessageContextBuilderFactory>();

            // Singletons
            services.AddSingleton<IFirestoreService, FirestoreService>();
            services.AddSingleton<IEventEmitter, EventEmitter>();
            services.AddSingleton<IActorSystem, ActorSystem>();
        }
    }
}