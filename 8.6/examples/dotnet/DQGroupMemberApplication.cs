/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.Threading;
using TIBCO.Rendezvous;


namespace TIBCO.Rendezvous.Examples
{

	public class DQGroupMemberApplication
	{

		CMListener cmListener;


		[MTAThread]
		static void Main(string[] arguments)
		{

			DQGroupMemberApplication DQGroupMemberApplication = new DQGroupMemberApplication(null, null, null, "DQ.NAME", "cm.test.subject");

			DQGroupMemberApplication.Run();
		}



		public DQGroupMemberApplication(string service, string network, string daemon, string queueName, string queueSubject)
		{

			try
			{
				TIBCO.Rendezvous.Environment.Open();
			}
			catch(RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}

			NetTransport netTransport = null;

			try
			{
				netTransport = new NetTransport(service, network, daemon);
			}
			catch (RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}

			CMQueueTransport cmQueueTransport = null;

			try
			{
				cmQueueTransport = new CMQueueTransport(netTransport, queueName, 10, 1, 5, 3, 10);
			}
			catch(RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}

			try
			{
				this.cmListener = new CMListener(Queue.Default, cmQueueTransport, queueSubject, null);

				this.cmListener.MessageReceived += new MessageReceivedEventHandler(this.MyMessageReceivedEventHandler);
			}
			catch(RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}
		}



		public void Run()
		{
			while(true)
			{
				try
				{
					Queue.Default.Dispatch();
				}
				catch(RendezvousException rendezvousException)
				{
					Console.Error.WriteLine(rendezvousException.StackTrace);					

					break;
				}
			}

			TIBCO.Rendezvous.Environment.Close();
		}



		void MyMessageReceivedEventHandler(object cmListener, MessageReceivedEventArgs messageReceivedEventArgs)
		{

			Console.Out.WriteLine("Received message: {0}", messageReceivedEventArgs.Message);

			Console.Out.Flush();
		}


	}


}
