// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.DncEng.Configuration.Extensions;
using Microsoft.DotNet.ServiceFabric.ServiceHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;

namespace Maestro.Web
{
    internal static class Program
    {
        public static bool RunningInServiceFabric()
        {
            string fabricApplication = Environment.GetEnvironmentVariable("Fabric_ApplicationName");
            return !string.IsNullOrEmpty(fabricApplication);
        }

        /// <summary>
        ///     This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            if (RunningInServiceFabric())
            {
                ServiceFabricMain();
            }
            else
            {
                NonServiceFabricMain();
            }
        }

        private static void NonServiceFabricMain()
        {
            new HostBuilder()
                .ConfigureWebHostDefaults(b =>
                {
                    b.ConfigureKestrel(o => { })
                        .UseStartup<Startup>()
                        .UseUrls("http://localhost:8080/")
                        .CaptureStartupErrors(true);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddDefaultJsonConfiguration(context.HostingEnvironment);
                })
                .ConfigureLogging(
                    builder =>
                    {
                        builder.AddFilter(level => level > LogLevel.Debug);
                        builder.AddConsole();
                    })
                .Build()
                .Run();
        }

        private static void ServiceFabricMain()
        {
            ServiceHost.Run(host => host.RegisterStatelessWebService<Startup>("Maestro.WebType",
                hostBuilder =>
                {
                    hostBuilder.ConfigureAppConfiguration((context, builder) =>
                    {
                        builder.AddDefaultJsonConfiguration((IHostEnvironment) context.HostingEnvironment);
                    });
                }));
        }
    }
}
