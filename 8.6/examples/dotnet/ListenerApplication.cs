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
	/// Generic Rendezvous subscriber.
	/// This program listens for any number of messages on a specified set of subject(s).
	/// Message(s) received are printed. Some platforms require proper quoting of the
	/// arguments to prevent the command line processor from modifying the command arguments.
	/// The user may terminate the program by typing Control-C. Optionally the user may specify
	/// communication parameters for transport creation.
	/// If none are specified the following defaults are used:
	/// service     "rendezvous" or "7500/udp"
	/// network     the result of gethostname
	/// daemon      "tcp:7500"
	/// 
	/// Examples:
	/// 
	/// Listen to every message published on subject a.b.c:
	/// RendezvousListener a.b.c
	/// 
	/// Listen to every message published on subjects a.b.c and x.*.Z:
	/// RendezvousListner a.b.c "x.*.Z"
	/// 
	/// Listen to every system advisory message:
	/// RendezvousListener "_RV.*.SYSTEM.>"
	/// 
	/// Listen to messages published on subject a.b.c using port 7566:
	/// RendezvousListener -service 7566 a.b.c
	/// </summary>
	class ListenerApplication
	{
		static string service = null;
		static string network = null;
		static string daemon = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] arguments)
		{
			int argumentsCount = InitializeParameters(arguments);

			if (argumentsCount >= arguments.Length)
			{
				Usage();
			}

			try
			{
				/* Create internal TIB/Rendezvous machinery */
				if (TIBCO.Rendezvous.Environment.IsIPM())
				{
					/*
					 * Prior to using the Rendezvous IPM library please read the appropriate
					 * sections of the user guide to determine if IPM is the correct choice
					 * for your application; it is likely not.
					 *
					 * To use the shared Rendezvous IPM library in .NET on Windows,
					 * first make sure it is located in your system path before the standard
					 * Rendezvous library.
					 *
					 * The IPM shared library can be found in %TIBRV_HOME%\bin\ipm.
					 *
					 * The IPM static library can be found in %TIBRV_HOME%\lib\ipm.
					 *
					 * To configure IPM you can do one of the following:
					 *
					 * 1) Nothing, and accept the default IPM RV parameter values.
					 *
					 * 2) Place a file named "tibrvipm.cfg" in your PATH, and have
					 * IPM automatically read in configuration values.
					 *
					 * 3) Call Environment.SetRVParameters, prior to Environment.Open:
					 *
					 *   string[] parameters =
					 *	new string[] {"-reliability", "3", "-reuse-port", "30000"};
					 *   Environment.SetRVParameters(parameters);
					 *   Environment.Open();
					 *
					 * 4) Call Environment.Open(string pathname), and have IPM read
					 * in the configuration values:
					 *
					 *   Environment.Open(".\\tibrvipm.cfg");
					 *
					 * An example configuration file, "tibrvipm.cfg", can be found in the
					 * "%TIBRV_HOME%\examples\IPM" directory of the Rendezvous installation.
					 *
					 */
					TIBCO.Rendezvous.Environment.Open(".\\tibrvipm.cfg");
				}
				else
				{
					TIBCO.Rendezvous.Environment.Open();
				}
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to open Rendezvous Environment: {0}", exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Network transport
			Transport transport = null;
			try
			{
				transport = new NetTransport(service, network, daemon);
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create NetTransport");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			Listener[] listeners = new Listener[arguments.Length - argumentsCount];
			int listenersCount = 0;
			// Create listeners for specified subjects
			while (argumentsCount < arguments.Length)
			{
				// create listener using default queue
				try
				{
					listeners[listenersCount] = new Listener(Queue.Default, transport, arguments[argumentsCount], null);
					listeners[listenersCount].MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);
					Console.Error.WriteLine("Listening on: " + arguments[argumentsCount]);
				}
				catch (RendezvousException exception)
				{
					Console.Error.WriteLine("Failed to create listener:");
					Console.Error.WriteLine(exception.StackTrace);
					System.Environment.Exit(1);
				}
				argumentsCount++;
				listenersCount++;
			}

			// dispatch Rendezvous events
			while(true)
			{
				try
				{
					Queue.Default.Dispatch();
				}
				catch(RendezvousException exception)
				{
					Console.Error.WriteLine("Exception dispatching default queue:");
					Console.Error.WriteLine(exception.StackTrace);
					break;
				}
			}

			// Force optimizer to keep alive listeners up to this point.
			GC.KeepAlive(listeners);

			TIBCO.Rendezvous.Environment.Close();
		}

		static void Usage()
		{
			Console.Out.Write("Usage: RendezvousListener [-service service] [-network network]");
			Console.Out.Write("                          [-daemon daemon] <subject-list>");
			System.Environment.Exit(1);
		}

		static int InitializeParameters(string[] arguments)
		{
			int i = 0;
			while(i < arguments.Length - 1 && arguments[i].StartsWith("-"))
			{
				if (arguments[i].Equals("-service"))
				{
					service = arguments[i+1];
					i += 2;
				}
				else
					if (arguments[i].Equals("-network"))
				{
					network = arguments[i+1];
					i += 2;
				}
				else
					if (arguments[i].Equals("-daemon"))
				{
					daemon = arguments[i+1];
					i += 2;
				}
				else
					Usage();
			}
			return i;
		}

		static void OnMessageReceived(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
		{
			Message message = messageReceivedEventArgs.Message;

			Console.Out.WriteLine("{0}: subject={1}, reply={2}, message={3}",
				DateTime.Now.ToString(),
				message.SendSubject,
				message.ReplySubject,
				message.ToString());
			Console.Out.Flush();
		}
	}
}
