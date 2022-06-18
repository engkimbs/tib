/// Copyright (c) 1998-$Date: 2016-12-13 12:54:43 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using TIBCO.Rendezvous;

namespace TIBCO.Rendezvous.Examples
{
	/// <summary>
	///  RendezvousFTMonitor - example TIBCO Rendezvous fault tolerant group
	///  monitor
	///  
	///  This program monitors the fault tolerant group TIBRVFT_TIME_EXAMPLE,
	///  the group established by the tibrvfttime timestamp message sending
	///  program.   It will report a change in the number of active members
	///  of that group.
	///  
	///  The RendezvousFTMonitor program must use the default communication
	///  parameters.
	/// </summary>
	public class FTMonitorApplication
	{
		static string groupName = "TIBRVFT_TIME_EXAMPLE";

		// RVD transport parameters
		static string service = null;
		static string network = null;
		static string daemon = null;

		static double lostInterval = 4.8; // matches tibrvfttime
		static uint oldNumberActive = 0;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] arguments)
		{
			try
			{
				TIBCO.Rendezvous.Environment.Open();
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to open Rendezvous Environment: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Network transport
			NetTransport netTransport = null;
			try
			{
				netTransport = new NetTransport(service, network, daemon);
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create NetTransport: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Set up the monitoring of the TIBRVFT_TIME_EXAMPLE group.
			FTGroupMonitor ftGroupMonitor = null;
			try
			{
				ftGroupMonitor = new FTGroupMonitor(Queue.Default, netTransport, groupName, lostInterval, null);
				ftGroupMonitor.GroupStateChanged += new GroupStateChangedEventHandler(OnGroupStateChanged);
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to start FT group monitor: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			Console.Out.WriteLine("RendezvousFTMonitor: Waiting for group information...");

			// dispatch Rendezvous events
			while(true)
			{
				try
				{
					Queue.Default.Dispatch();
				}
				catch(RendezvousException exception)
				{
					Console.Error.WriteLine("Exception dispatching default queue:\n" + exception.StackTrace);					
					break;
				}
			}

			// Force optimizer to keep alive listeners up to this point.
			GC.KeepAlive(ftGroupMonitor);

			TIBCO.Rendezvous.Environment.Close();
		}

		// Fault tolerance monitor callback called when FT detects a
		// change in the number of active members in group TIBRVFT_TIME_EXAMPLE.
		static void OnGroupStateChanged(object monitor, GroupStateChangedEventArgs arguments)
		{
			//  static int oldNumActive = 0;
			Console.Out.WriteLine("Group [{0}]: has {1} members (after {2}).", ((FTGroupMonitor) monitor).GroupName, arguments.NumberActiveMembers, (oldNumberActive > arguments.NumberActiveMembers ? "one deactivated" : "one activated" ));
			oldNumberActive = arguments.NumberActiveMembers;
		}
	}
}
