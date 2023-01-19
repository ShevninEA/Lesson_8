using Lesson_8.Models.Reports;
using Lesson_8.Services.Impl;
using Lesson_8.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autofac.Configuration;
using Lesson_8.Extention;

namespace Lesson_8
{
    internal class Program
    {
        private static Random random = new Random();

        private static WebApplication? _app;

        public static WebApplication App 
        {
            get 
            {
                if (_app == null)
                {
                    _app = CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

                    if (!_app.Environment.IsDevelopment())
                    {
                        _app.UseExceptionHandler("/Home/Error");
                    }
                    _app.UseStaticFiles();

                    _app.UseRouting();

                    _app.UseAuthorization();

                    _app.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                }
                return _app;
            }
        }

        public static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var webApplicationBuilder = WebApplication.CreateBuilder(args);
            webApplicationBuilder.Host
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(container => // Autofac
            {

                var config = new ConfigurationBuilder()
                        .AddJsonFile("autofac.config.json", true, false);
                var module = new ConfigurationModule(config.Build());
                var builder = new ContainerBuilder();
                builder.RegisterModule(module);

            })
            .ConfigureHostConfiguration(options =>
                options.AddJsonFile("appsettings.json"))
            .ConfigureAppConfiguration(options =>
                options.AddJsonFile("appsettings.json")
                .AddXmlFile("appsettings.xml", true)
                .AddIniFile("appsettings.ini", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args))
            .ConfigureLogging(options =>
                options.ClearProviders()
                    .AddConsole()
                    .AddDebug())
            .ConfigureServices(ConfigureServices);

            return webApplicationBuilder;
        }


        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddControllersWithViews();

            #region Register Base Services

            // Стандартный способ регистрации сервиса (Microsoft.Extensions.DependencyInjection)
            services.AddTransient<IOrderService, OrderService>();


            #endregion

            #region Configure EF DBContext Service

            services.AddDbContext<OrdersDbContext>(options =>
            {
                options.UseSqlServer(host.Configuration["Settings:DatabaseOptions:ConnectionString"]);
            });

            #endregion
        }

        public static IServiceProvider Services => App.Services;

        static async Task Main(string[] args)
        {
            var app = App;
            await app.StartAsync();
            await PrintBuyersAsync();
            Console.ReadKey(true);
            await app.StopAsync();
        }

        private static async Task PrintBuyersAsync()
        {
            await using (var servicesScope = Services.CreateAsyncScope())
            {
                var services = servicesScope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<OrdersDbContext>();

                //await context.Database.MigrateAsync();

                foreach (var buyer in context.Buyers)
                {
                    logger.LogInformation($"Покупатель >>> {buyer.Id} {buyer.LastName} {buyer.Name} {buyer.Patronymic} {buyer.Birthday.ToShortDateString()}");
                }

                var orderService = services.GetRequiredService<IOrderService>();


                await orderService.CreateAsync(random.Next(1, 6), "123, Russia, Address", "+79001112233", new (int, int)[] {
                    new ValueTuple<int, int>(1, 1)
                });


                //var catalog = new ProductsCatalog
                //{
                //    Name = "Каталог товаров",
                //    Description = "Актуальный список товаров на дату",
                //    CreationDate = DateTime.Now,
                //    Products = context.Products
                //};

                //string templateFile = "Templates/DefaultTempate.docx";
                //IProductReport report = new ProductReportWord(templateFile);

                //CreateReport(report, catalog, "Report.docx");

                Console.ReadKey(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportGenerator">Объект - генератор отчета</param>
        /// <param name="catalog">Объект с данными</param>
        /// <param name="reportFileName">Наименование файла-отчета</param>
        private static void CreateReport(IProductReport reportGenerator, ProductsCatalog catalog, string reportFileName)
        {
            reportGenerator.CatalogName = catalog.Name;
            reportGenerator.CatalogDescription = catalog.Description;
            reportGenerator.CreationDate = catalog.CreationDate;
            reportGenerator.Products = catalog.Products.Select(product => (product.Id, product.Name, product.Category, product.Price));

            var reportFileInfo = reportGenerator.Create(reportFileName);
            reportFileInfo.Execute();
        }
    }
}