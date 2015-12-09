using NHibernate.Cfg;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Legacy;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MultiplePubSubQueuing.Controllers
{
    public class QueueController : Controller
    {
        //
        // GET: /Queue/

        public ActionResult Index()
        {
            return View("Queue");
        }

        public async Task<ActionResult> Publish(string sMessage)
        {
            //Configuration nhConfiguration = new Configuration();
            //nhConfiguration.Properties["dialect"] = "NHibernate.Dialect.MsSql2008Dialect";
            //nhConfiguration.Properties["connection.provider"] = "NHibernate.Connection.DriverConnectionProvider";
            //nhConfiguration.Properties["connection.driver_class"] = "NHibernate.Driver.Sql2008ClientDriver";

            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EnableInstallers();
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.UsePersistence<NHibernatePersistence>();
            busConfiguration.UseTransport<MsmqTransport>();

            //busConfiguration.UsePersistence<NHibernatePersistence, StorageType.Sagas>();
            busConfiguration.UsePersistence<NHibernatePersistence, StorageType.Subscriptions>();
            busConfiguration.UsePersistence<NHibernatePersistence, StorageType.Timeouts>();
            //busConfiguration.UsePersistence<NHibernatePersistence, StorageType.Outbox>();
            //busConfiguration.UsePersistence<NHibernatePersistence, StorageType.GatewayDeduplication>();


            busConfiguration.EndpointName("Publisher");

            //using (IBus bus = Bus.Create(busConfiguration).Start())
            //{
                //bus.Unsubscribe<MyFirstEvent>();
                //bus.Unsubscribe<FirstEvent>();
                //bus.Unsubscribe<SecondEvent>();
                //bus.Publish<MyFirstEvent>(new MyFirstEvent() { message = "MyFirstEvent" + sMessage });
                //bus.Publish<Two>(new Two() { message = "SecondEvent" + sMessage });
                //bus.Publish<One>(new One() { message = "FirstEvent" + sMessage });
            //}
            MultiPublish(sMessage, busConfiguration);

            ViewBag.Result = "Successfully published!";
            //RedirectToAction("Index");
            return View("Queue");
        }

        private async static void MultiPublish(string sMessage, BusConfiguration busConfiguration)
        {
            using (IBus bus = Bus.Create(busConfiguration).Start())
            {
                //bus.Unsubscribe<One>();
                //bus.Unsubscribe<Two>();

                Console.WriteLine("Start Time:" + System.DateTime.Now);
                //Fire multiple events.
                for (int i = 0; i < 100; i++)
                {
                    await Task.Factory.StartNew(() => bus.Publish<One>(new One() { message = "FirstEvent" + i + sMessage }));
                    //bus.Publish<One>(new One() { message = "FirstEvent" + sMessage });
                }
                Console.WriteLine("End Time:" + System.DateTime.Now);

                Console.WriteLine("Start Time:" + System.DateTime.Now);
                //Fire multiple events.
                for (int i = 0; i < 100; i++)
                {
                    await Task.Factory.StartNew(() => bus.Publish<Two>(new Two() { message = "SecondEvent" + i + sMessage }));
                    //bus.Publish<Two>(new Two() { message = "SecondEvent" + sMessage });
                }
                Console.WriteLine("End Time:" + System.DateTime.Now);
            }
        }
    }
}
