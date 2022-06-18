/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.IO;
using TIBCO.Rendezvous;

namespace TIBCO.Rendezvous.Examples
{
	/// <summary>
	/// RendezvousReviewLedger - sample CM Rendezvous cm ledger review
	/// This program reviews the ledger file of a persistent certified
	/// correspondent.  Transport parameters, cmname, ledger file, an
	/// optional limit on the number of message subjects to be reviewed,
	/// and the subject string are command line arguments to this program.
	/// 
	/// If none are specified, the following defaults are used:
	/// service     32765
	/// network     127.0.0.1       (loopback)
	/// daemon      null
	/// subject     ">"
	/// Non-blank cmname and ledger file parameters are required.
	/// 
	/// Examples:
	/// 
	/// Review all subjects in ledger cm.ldg created by cm transport
	/// with cmname cmsender
	///		RendezvousReviewLedger -cmname cmsender -ledger cm.ldg
	///		
	///	=======================================================================
	///	                        ***** WARNING *****                           
	///	The tibrvcmtransport_ReviewLedger function is intended to be used in
	///	the context of the running CM application.  This example shows how to
	///	obtain information on sent messages from a ledger, but it is not in
	///	context.
	///	Re-creating a persistent cm transport in a different program can have
	///	unintended consequences.  This program should not be used on a system
	///	with a network connection which would allow it to interfere with a
	///	production environment.
	///	=======================================================================
	/// </summary>
	public class ReviewLedgerApplication
	{
		// RVD transport parameters
		static string service = "32765";
		static string network = "127.0.0.1";
		static string daemon = null;

		// Subject we use to listen messages on
		static string subject = ">";
    
		// Our unique CM name
		static string cmname = null;

		static string ledgerFilename = null;
		static uint maximumSubjects;

		static uint messageCount;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] arguments)
		{
			InitializeParameters(arguments);

			/* If no cmname given, warn and exit. */
			if (cmname == null)
			{
				Console.Error.WriteLine(" Reusable cmname is required for ledger review");
				System.Environment.Exit(1);
			}

			/* If no ledger file given, warn and exit. */
			if (ledgerFilename == null)
			{
				Console.Error.WriteLine(" Ledger file is required for ledger review");
				System.Environment.Exit(1);
			}

			FileStream ledgerStream = null;
			/* Try to open the file for readonly access.  If not found, warn and exit. */
			try
			{
				ledgerStream = File.OpenRead(ledgerFilename);
			}
			catch(Exception exception)
			{
				Console.Error.WriteLine("Failed to open Ledger file: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}
			if (ledgerStream != null) ledgerStream.Close();


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
				netTransport = new NetTransport();
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create NetTransport: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create CM transport
			CMTransport cmTransport = null;
			try
			{
				cmTransport = new CMTransport(netTransport, cmname, true, ledgerFilename, false);

			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create CMTransport: " + exception.Message);
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}			

			cmTransport.ReviewLedger(new ReviewLedgerDelegate(OnReviewLedger), subject, null);

			//
			// Report the number of matches.  If maximum subject count has been reached,
			// report as stopped.
			//
			if ((maximumSubjects == 0) || (messageCount < maximumSubjects))
			{
				Console.Out.WriteLine("\n                            {0} matching subjects.", messageCount);
			} 
			else 
			{
				Console.Out.WriteLine("\n                     Stop review after {0} matching subjects.", messageCount);
			}

			TIBCO.Rendezvous.Environment.Close();
		}

		static bool OnReviewLedger(CMTransport cmTransport, string subject, Message message, object closure)
		{
			// Count the message
			messageCount++;

			// Get the number of fields in the ledger message.
			uint fieldCount = message.FieldCount;

			string ledgerSubject = message.GetField("subject");
			UInt32 totalMessages = message.GetField("total_msgs");
			UInt64 totalSize = message.GetField("total_size");
			UInt64 lastSequenceNumberSent = message.GetField("seqno_last_sent");

			Console.Out.WriteLine("Subject: " + ledgerSubject + " Last sequence number sent: " + lastSequenceNumberSent);

			// Listener data is provided in submessage fields, one per certified
			// listener for the subject, with index values from 4 to fieldCount - 1.
			// Get and display the data for each listener.			
			for (uint i = 4; i < fieldCount; i++)
			{
				Message subMessage = message.GetFieldByIndex(i);
				Console.WriteLine((string)subMessage.GetField("name") + " Last confirmed: " + (UInt64)subMessage.GetField("last_confirmed"));
			}

			// Display the usage totals if messages on this subject are stored.
			if (totalMessages > 0)
				Console.Out.WriteLine("                {0} messages occupy {1} bytes.", totalMessages, totalSize);

			if ((maximumSubjects > 0) && (messageCount >= maximumSubjects))
				return true;
			else
				return false;
		}

		static void Usage()
		{
			Console.Out.WriteLine("Usage: RendezvousReviewLedger [-service service] [-network network]");
			Console.Out.WriteLine("                              [-daemon daemon] [-ledger filename]");
			Console.Out.WriteLine("                              [-cmname cmname] [-maxsubj #subjects] subject");
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
				else if (arguments[i].Equals("-network"))
				{
					network = arguments[i+1];
					i += 2;
				}
				else if (arguments[i].Equals("-daemon"))
				{
					daemon = arguments[i+1];
					i += 2;
				}
				else if (arguments[i].Equals("-ledger"))
				{
					ledgerFilename = arguments[i+1];
					i += 2;
				}
				else if (arguments[i].Equals("-cmname"))
				{
					cmname = arguments[i+1];
					i += 2;
				}
				else if (arguments[i].Equals("-maxsubj"))
				{
					maximumSubjects = Convert.ToUInt32(arguments[i+1]);
					i += 2;
				}
				else
					Usage();
			}

			if (i < arguments.Length)
			{
				subject = arguments[i];
				i++;
			}

			return i;
		}
	}
}
