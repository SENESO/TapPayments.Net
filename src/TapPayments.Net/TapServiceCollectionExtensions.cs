using System;
using Microsoft.Extensions.DependencyInjection;

namespace TapPayments
{
    /// <summary>
    /// ASP.NET Core DI helpers for TapPayments.Net.
    /// </summary>
    public static class TapServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TapClient as a singleton.
        /// </summary>
        public static IServiceCollection AddTapPayments(this IServiceCollection services, Action<TapClientOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var options = new TapClientOptions();
            configure(options);
            options.Validate();

            services.AddSingleton(options);
            services.AddSingleton<TapClient>();
            return services;
        }

        /// <summary>
        /// Registers TapClient as a singleton with the given secret key.
        /// </summary>
        public static IServiceCollection AddTapPayments(this IServiceCollection services, string secretKey)
            => AddTapPayments(services, o => o.SecretKey = secretKey);
    }
}
