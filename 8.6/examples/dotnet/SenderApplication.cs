/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.Net;
using TIBCO.Rendezvous;

namespace TIBCO.Rendezvous.Examples
{
	/// <summary>
	///  RendezvousSender - sample Rendezvous message publisher.
	///  This program publishes one or more string messages on a specified
	///  subject.  Both the subject and the message(s) must be supplied as
	///  command parameters.  Message(s) with embedded spaces should be quoted.
	///  A field named "DATA" will be created to hold the string in each
	///  message.
	///  
	///  Optionally the user may specify communication parameters for 
	///  NetTransport constructor. If none are specified the following 
	///  defaults are used:
	///  
	///  service     "rendezvous" or "7500/udp"
	///  network     the result of gethostname
	///  daemon      "tcp:7500"
	///	 
	///  Normally a listener such as tibrvlisten should be started first.
	///  
	///  Examples:
	///  
	///  Publish two messages on subject a.b.c and default parameters:
	///  RendezvousSender a.b.c "This is my first message" "This is my second message"
	///  
	///  Publish a message on subject a.b.c using port 7566:
	///  RendezvousSender -service 7566 a.b.c message
	/// </summary>
	class SenderApplication
	{
		static string service = null;
		static string network = null;
		static string daemon = null;
		static int iterations = 1;

		const String FIELD_NAME = "DATA";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] arguments)
		{
			int argumentsCount = InitializeParameters(arguments);

			if (argumentsCount > (arguments.Length - 2))
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
				Console.Error.WriteLine("Failed to create NetTransport:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}
			
			// Create the message
			Message message = new Message();

			// Set send subject into the message
			try
			{
				message.SendSubject = arguments[argumentsCount++];
			}
			catch (RendezvousException exception) 
			{
				Console.Error.WriteLine("Failed to set send subject:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			try
			{
				// Send one message for each parameter
				while (argumentsCount < arguments.Length)
				{
					Console.Out.WriteLine("Publishing: subject={0} \"{1}\"",
						message.SendSubject,
						arguments[argumentsCount]);
					message.AddField(FIELD_NAME, arguments[argumentsCount], 0);
					for (int i = 0; i < SenderApplication.iterations; i++)
					{
						transport.Send(message);
					}
					argumentsCount++;
				}
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Error sending a message:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Close Environment, it will cleanup all underlying memory, destroy
			// transport and guarantee delivery.
			try
			{
				TIBCO.Rendezvous.Environment.Close();
			}
			catch(RendezvousException exception)
			{				
				Console.Error.WriteLine("Exception dispatching default queue:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}
		}
		
		static void Usage()
		{
			Console.Out.Write("Usage: RendezvousSender [-service service] [-network network]");
			Console.Out.Write("                        [-daemon daemon] <subject> <messages>");
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
					if (arguments[i].Equals("-iterations"))
				{
					iterations = Int32.Parse(arguments[i+1]);
					i += 2;
				}
				else
					Usage();
			}
			return i;
		}
	}	
}
