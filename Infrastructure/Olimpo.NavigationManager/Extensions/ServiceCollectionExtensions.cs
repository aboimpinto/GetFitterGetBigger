using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Olimpo;

public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TMetadata>(this IServiceCollection services, TMetadata metadata) where TService : class
        {
            return services.AddScoped<TService, TService, TMetadata>(metadata);
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="implementation">The implementation.</param>
        /// <param name="context">The context.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, TImplementation implementation, string context) where TService : class
                                                                                                                                                                where TImplementation : class, TService
        {
            return services.AddScoped<TService, TImplementation, string>(implementation, context);
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="implementationFactory">The implementation factory.</param>
        /// <param name="context">The context.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, string context) where TService : class
                                                                                                                                                                                               where TImplementation : class, TService
        {
            return services.AddScoped<TService, TImplementation, string>(implementationFactory, context);
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="implementationFactory">The implementation factory.</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation, TMetadata>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, TMetadata metadata) where TService : class
                                                                                                                                                                                                              where TImplementation : class, TService
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return services.AddScoped<TService>(s => s.GetServices<IServiceMetadata<TService, TMetadata>>()
                                                      .OfType<IServiceMetadata<TService, TImplementation, TMetadata>>()
                                                      .First(x => Equals(x.Metadata, metadata))
                                                      .CachingImplementationFactory(s)) // This registration ensures that only one instance is created in the scope
                           .AddScoped((Func<IServiceProvider, IServiceMetadata<TService, TMetadata>>)(s => new ServiceMetadata<TService, TImplementation, TMetadata>(metadata, implementationFactory)));
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="implementation">The implementation.</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation, TMetadata>(this IServiceCollection services, TImplementation implementation, TMetadata metadata) where TService : class
                                                                                                                                                                               where TImplementation : class, TService
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            return services.AddScoped<TService>(s => s.GetServices<IServiceMetadata<TService, TMetadata>>()
                                                      .OfType<IServiceMetadata<TService, TImplementation, TMetadata>>()
                                                      .First(sm => Equals(sm.Metadata, metadata))
                                                      .CachingImplementationFactory(s))
                           .AddScoped<IServiceMetadata<TService, TMetadata>>(s => new ServiceMetadata<TService, TImplementation, TMetadata>(metadata, ss => implementation));
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation, TMetadata>(this IServiceCollection services, TMetadata metadata) where TService : class
                                                                                                                                               where TImplementation : class, TService
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            services.TryAddTransient<TImplementation>();
            if (typeof(TService) != typeof(TImplementation))
                services.AddScoped<TService>(s => s.GetServices<IServiceMetadata<TService, TMetadata>>()
                                                   .OfType<IServiceMetadata<TService, TImplementation, TMetadata>>()
                                                   .First(sm => Equals(sm.Metadata, metadata))
                                                   .CachingImplementationFactory(s));

            return services.AddScoped<IServiceMetadata<TService, TMetadata>>(s => new ServiceMetadata<TService, TImplementation, TMetadata>(metadata, ss => ss.GetService<TImplementation>()));
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="context">The context.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, string context) where TService : class
        {
            return services.AddScoped<TService, string>(context);
        }

        /// <summary>
        /// Add scoped.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="context">The context.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, string context) where TService : class
                                                                                                                                where TImplementation : class, TService
        {
            return services.AddScoped<TService, TImplementation, string>(context);
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <typeparamref name="TService"></typeparamref></returns>
        public static TService GetService<TService>(this IServiceProvider serviceProvider, string context)
        {
            return context == null ? serviceProvider.GetService<TService, string>() : serviceProvider.GetService<TService, string>(m => m == context);
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A <typeparamref name="TService"></typeparamref></returns>
        public static TService GetService<TService, TMetadata>(this IServiceProvider serviceProvider, Func<TMetadata, bool> predicate = null)
        {
            var relevantMetadata = serviceProvider.GetServices<IServiceMetadata<TService, TMetadata>>();
            var selectedMetadata = predicate != null ? relevantMetadata.FirstOrDefault(sm => predicate(sm.Metadata)) : relevantMetadata.FirstOrDefault();
            return selectedMetadata != null ? selectedMetadata.CachingImplementationFactory(serviceProvider) : default;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A list of tservices.</returns>
        public static IEnumerable<TService> GetServices<TService, TMetadata>(this IServiceProvider serviceProvider, Func<TMetadata, bool> predicate = null)
        {
            var metadata = serviceProvider.GetServices<IServiceMetadata<TService, TMetadata>>();

            return (predicate != null ? metadata.Where(sm => predicate(sm.Metadata)) : metadata).Select(sm => sm.CachingImplementationFactory(serviceProvider));
        }

        /// <summary>
        /// The service metadata interface.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        private interface IServiceMetadata<TService, out TMetadata>
        {
            /// <summary>
            /// Gets caching implementation factory.
            /// </summary>
            Func<IServiceProvider, TService> CachingImplementationFactory { get; }

            /// <summary>
            /// Gets metadata.
            /// </summary>
            TMetadata Metadata { get; }
        }

        /// <summary>
        /// The service metadata interface.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        private interface IServiceMetadata<TService, TImplementation, out TMetadata> : IServiceMetadata<TService, TMetadata> where TService : class
                                                                                                                             where TImplementation : class, TService
        {
            /// <summary>
            /// Gets caching implementation factory.
            /// </summary>
            new Func<IServiceProvider, TImplementation> CachingImplementationFactory { get; }
        }

        /// <summary>
        /// Service metadata.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        private class ServiceMetadata<TService, TImplementation, TMetadata> : IServiceMetadata<TService, TImplementation, TMetadata> where TService : class
                                                                                                                                     where TImplementation : class, TService
        {
            private TImplementation _cachedImplementation;

            Func<IServiceProvider, TService> IServiceMetadata<TService, TMetadata>.CachingImplementationFactory => CachingImplementationFactory;

            /// <summary>
            /// Gets caching implementation factory.
            /// </summary>
            public Func<IServiceProvider, TImplementation> CachingImplementationFactory { get; }

            /// <summary>
            /// Gets metadata.
            /// </summary>
            public TMetadata Metadata { get; }

            /// <summary>
            /// Constructor for ServiceMetadata.
            /// </summary>
            /// <param name="metadata"></param>
            /// <param name="implementationFactory"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public ServiceMetadata(TMetadata metadata, Func<IServiceProvider, TImplementation> implementationFactory)
            {
                if (implementationFactory == null)
                {
                    throw new ArgumentNullException(nameof(implementationFactory));
                }

                Metadata = metadata;
                CachingImplementationFactory = s => _cachedImplementation ?? (_cachedImplementation = implementationFactory(s));
            }
        }
    }
