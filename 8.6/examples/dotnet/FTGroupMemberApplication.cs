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

	public class FTGroupMemberApplication
	{

		bool active;

		Transport transport;

		FTGroupMember ftGroupMember;

		Timer dateTimeBroadcastTimer;


		[MTAThread]
		static void Main(string[] arguments)
		{

			FTGroupMemberApplication ftGroupMemberApplication = new FTGroupMemberApplication(null, null, null, "TIBRVFT_TIME_EXAMPLE",1000); 

			ftGroupMemberApplication.Run();
		}



		public FTGroupMemberApplication(string service, string network, string daemon, string groupName, int interval)
		{

			this.active = false;

			try
			{
				TIBCO.Rendezvous.Environment.Open();
			}
			catch(RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}

			try
			{
				this.transport = new NetTransport(service, network, daemon);
			}
			catch (RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}

			this.ftGroupMember = null;

			try
			{
				ActionTokenReceivedEventHandler actionTokenReceivedEventHandler = new ActionTokenReceivedEventHandler(this.MyActionTokenReceivedEventHandler);

				ushort weight = 50;
				ushort activeGoal = 1;
				double heartbeatInterval = 1.5;
				double preparationInterval = 0;
				double activationInterval = 4.8;

				this.ftGroupMember = new FTGroupMember(Queue.Default, actionTokenReceivedEventHandler, this.transport, groupName, weight, activeGoal, heartbeatInterval, preparationInterval, activationInterval, null);
			}
			catch(RendezvousException rendezvousException)
			{
				Console.Error.WriteLine(rendezvousException.StackTrace);

				System.Environment.Exit(1);
			}
			
			this.dateTimeBroadcastTimer = new Timer(new TimerCallback(this.MyTimerCallback), null, 0, interval);
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

			GC.KeepAlive(this.ftGroupMember);

			TIBCO.Rendezvous.Environment.Close();
		}



		void MyActionTokenReceivedEventHandler(object groupMember, ActionTokenReceivedEventArgs actionTokenReceivedEventArgs)
		{

			switch (actionTokenReceivedEventArgs.ActionToken)
			{
				case ActionToken.PrepareToActivate:
					
					break;

				case ActionToken.Activate:

					Console.WriteLine(">>> Activation token received. <<<");

					this.active = true;

					break;

				case ActionToken.Deactivate:

					Console.WriteLine(">>> Deactivation token received. <<<");

					this.active = false;

					break;

			}
		}


		void MyTimerCallback(object state)
		{

			if (this.active)
			{
				Message message = new Message();

				message.SendSubject = "TIME_CHANNEL";

				message.AddField("DATE", DateTime.Now);

				this.transport.Send(message);

				Console.WriteLine(">>> Published:");

				Console.WriteLine(message.ToString());
			}
		}

	}


}
