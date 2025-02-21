﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dtmcli
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDtmcli(this IServiceCollection services, Action<DtmOptions> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);

            var op = new DtmOptions();
            setupAction(op);

            return AddDtmcliCore(services, op);
        }

        public static IServiceCollection AddDtmcli(this IServiceCollection services, IConfiguration configuration, string sectionName = "dtm")
        {
            services.Configure<DtmOptions>(configuration.GetSection(sectionName));
            
            var op=configuration.GetSection(sectionName).Get<DtmOptions>() ?? new DtmOptions();

            return AddDtmcliCore(services, op);
        }

        private static IServiceCollection AddDtmcliCore(IServiceCollection services,DtmOptions options)
        {
            AddHttpClient(services, options);
            AddDtmCore(services);

            return services;
        }

        private static void AddHttpClient(IServiceCollection services,DtmOptions options)
        {
            services.AddHttpClient(Constant.DtmClientHttpName, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromMilliseconds(options.DtmHttpTimeout);
            });
            services.AddHttpClient(Constant.BranchClientHttpName, client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(options.BranchHttpTimeout);
            });
        }

        private static void AddDtmCore(IServiceCollection services)
        {
            // trans releate
            services.AddSingleton<IDtmTransFactory, DtmTransFactory>();
            services.AddSingleton<IDtmClient, DtmClient>();
            services.AddSingleton<TccGlobalTransaction>();

            // barrier database relate
            services.AddSingleton<DtmImp.IDbSpecial, DtmImp.MysqlDBSpecial>();
            services.AddSingleton<DtmImp.IDbSpecial, DtmImp.PostgresDBSpecial>();
            services.AddSingleton<DtmImp.IDbSpecial, DtmImp.SqlServerDBSpecial>();
            services.AddSingleton<DtmImp.DbSpecialDelegate>();
            services.AddSingleton<DtmImp.DbUtils>();

            // barrier factory
            services.AddSingleton<IBranchBarrierFactory, DefaultBranchBarrierFactory>();
        }
    }
}