/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using TIBCO.Rendezvous;

namespace TIBCO.Rendezvous.Examples
{
	/*-------------------------------------------------------------------
 * PersonalData is an example of a simple class supporting
 * Serializable interface.
 * This example program sends and receives objects of this class.
 *-----------------------------------------------------------------*/

	/// <summary>
	/// PersonalData is an example of a simple class supporting serialization.
	/// This example program sends and receives objects of this class.
	/// </summary>
	[Serializable]
	class PersonalData
	{
		string firstName  = "";		
		string lastName   = "";		
		[NonSerialized]string ssn = "XXX-XX-XXXX";
		int age = 0;

		public PersonalData()
		{
		}

		public PersonalData(string firstName, string lastName, int age, string ssn) 
		{
			this.firstName = firstName;
			this.lastName  = lastName;
			this.age       = age;
			this.ssn       = ssn;
		}

		public override string ToString() 
		{
			return this.lastName + ", " + this.firstName + " - " + this.age + " y.o. - SSN " + this.ssn;
		}
	}

	/// <summary>
	/// RendezvousSendObject - sends .NET objects via TIBCO Rendezvous messages.
	/// 
    /// This example demonstrates how to send and receive .NET
    /// objects which support Serializable attribute.
    /// 
    /// Note: this can only be used when both the sender and the receiver are
    /// .NET applications. Demonstrated technique must not be used when
    /// it is required to exchange messages with applications implemented
    /// in other environment.
    /// 
    /// This example does not have any parameters. It can be run with a
    /// simple command:
    /// 
    /// RendezvousSendObject
	/// </summary>
	class SendObjectApplication
	{
		// Our test subject
		const string SUBJECT = "test.send.java.object";

		// Field name we use to add object into Message
		const string FIELD_NAME = "object";

		static Dispatcher dispatcher = null;

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
				Console.Error.WriteLine("Failed to open Rendezvous Environment:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Network transport
			Transport transport = null;
			try
			{
				transport = new NetTransport();
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create NetTransport:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Queue
			Queue queue = null;
			try
			{
				queue = new Queue();
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create Queue:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Dispatcher
			dispatcher = null;
			try
			{
				dispatcher = new Dispatcher(queue);
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create Dispatcher:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create Listener
			Listener listener = null;
			try
			{
				listener = new Listener(queue, transport, SUBJECT, null);
				listener.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create Listener:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Create an object we want to send as a field in a Message
			PersonalData personalData = new PersonalData("John", "Doe", 25, "999-99-999");

			// Create the Message
			Message message = null;
			try
			{
				
				message = new Message();
				message.SendSubject = SUBJECT;
				AddObject(message, personalData);
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to create Message:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Send the message
			try
			{
				transport.Send(message);
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to send a Message:");
				Console.Error.WriteLine(exception.StackTrace);
				System.Environment.Exit(1);
			}

			// Wait until dispatcher is finished.
			dispatcher.Join();

			// Closing Environment
			TIBCO.Rendezvous.Environment.Close();
		}

		static void AddObject(Message message, object data)
		{
			IFormatter formatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			formatter.Serialize(memoryStream, data);
			memoryStream.Close();
			message.AddField(FIELD_NAME, memoryStream.ToArray(), 0);
		}

		static void OnMessageReceived(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
		{
			Message message = messageReceivedEventArgs.Message;

			byte[] data = message.GetField(FIELD_NAME, 0);

			MemoryStream memoryStream = new MemoryStream(data);
			IFormatter formatter = new BinaryFormatter();
			object anObject = formatter.Deserialize(memoryStream);

			if (anObject != null)
			{
				Console.Out.WriteLine("Retrieved object: class = {0}, ToString() = {1}",
					anObject.GetType().FullName,
					anObject.ToString());
			}
			else
			{
				Console.Out.WriteLine("Error: object not found in message.");
			}

			dispatcher.Destroy();
		}
	}
}
