using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaConsole
{
	class Program
	{
		static ActivityHost[] processes;

		static void Main(string[] args)
		{
			var routingSlip = new RoutingSlip(new WorkItem[]
                {
                    new WorkItem<ReserveCarActivity>(new WorkItemArguments{{"vehicleType", "Compact"}}),
                    new WorkItem<ReserveHotelActivity>(new WorkItemArguments{{"roomType", "Suite"}}),
                    new WorkItem<ReserveFlightActivity>(new WorkItemArguments{{"destination", "DUS"}})
                });


			// imagine these being completely separate processes with queues between them
			processes = new ActivityHost[]
                                {
                                    new ActivityHost<ReserveCarActivity>(Send),
                                    new ActivityHost<ReserveHotelActivity>(Send),
                                    new ActivityHost<ReserveFlightActivity>(Send)
                                };

			// hand off to the first address
			Send(routingSlip.ProgressUri, routingSlip);
			Console.WriteLine("Press any key the stop this program...");
			Console.ReadLine();
		}

		static void Send(Uri uri, RoutingSlip routingSlip)
		{
			// this is effectively the network dispatch
			foreach (var process in processes)
			{
				if (process.AcceptMessage(uri, routingSlip))
				{
					break;
				}
			}
		}

	}
}
