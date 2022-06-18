/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.IO;
using System.Text;


using TIBCO.Rendezvous;



namespace TIBCO.Rendezvous.Examples
{

	/// <summary>
	/// The definition of a simple custom data type adapter.
	/// </summary>
	class SelfConvertingStringAdapter : ICustomDataTypeAdapter 
	{

		public byte TypeID
		{

			get
			{

				return 128;
			}


		}



		public ICustomDataType Decode(byte[] bytes)
		{

			BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
			
			int codePage = binaryReader.ReadInt32();

			string stringInstance = Encoding.GetEncoding(codePage).GetString(binaryReader.ReadBytes(bytes.Length - 4), 0, bytes.Length - 4);

			return new SelfConvertingString(codePage, stringInstance);
		}


		public byte[] Encode(ICustomDataType instanceOfCustomDataType)
		{

			SelfConvertingString selfConvertingString = (SelfConvertingString) instanceOfCustomDataType;


			MemoryStream memoryStream = new MemoryStream();


			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

			binaryWriter.Write(selfConvertingString.CodePage);

			binaryWriter.Write(Encoding.Default.GetBytes(selfConvertingString.StringInstance));


			return memoryStream.ToArray();
		}



		public SelfConvertingStringAdapter()
		{
		}


	}


	
	/// <summary>
	/// The definition of a simple custom data type.
	/// </summary>
	class SelfConvertingString : ICustomDataType 
	{

		public int CodePage
		{

			get
			{

				return this.codePage;
			}


		}


		public string StringInstance
		{

			get
			{

				return this.stringInstance;
			}


		}



		int codePage;

		string stringInstance;


		public SelfConvertingString(string stringInstance)
		{

			this.codePage = Encoding.Default.CodePage;

			this.stringInstance = stringInstance;
		}


		public SelfConvertingString(int codePage, string stringInstance)
		{

			this.codePage = codePage;

			this.stringInstance = stringInstance;
		}



		public override string ToString()
		{

			return "Encoding Name = " + Encoding.GetEncoding(this.codePage).EncodingName + ", String = '" + this.stringInstance + "'";
		}


	}



	/// <summary>
	///  Sample listener application for custom data types.
	///  
	///  Optionally the user may specify communication parameters for 
	///  NetTransport constructor. If none are specified the following 
	///  defaults are used:
	///  
	///  -service "rendezvous" or "7500/udp"
	///  -network the result of gethostname
	///  -daemon "tcp:7500"
	/// </summary>
	class SelfConvertingStringListener
	{

		static string service = null;
		
		static string network = null;
		
		static string daemon = null;


		const String FIELD_NAME = "SELF_CONVERTING_STRING";


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] args)
		{

			int argumentCount = InitializeParameters(args);

			if (argumentCount >= args.Length)
			{
				Usage();
			}

			try
			{
				TIBCO.Rendezvous.Environment.Open();
			}
			catch(RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to open Rendezvous environment:");

				Console.Error.WriteLine(exception.StackTrace);

				System.Environment.Exit(1);
			}

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

			// NOTE: Registering our custom data type.
			Message.RegisterCustomDataType(typeof(SelfConvertingString), new SelfConvertingStringAdapter());

			Listener[] listeners = new Listener[args.Length - argumentCount];

			int listenerCount = 0;

			while (argumentCount < args.Length)
			{
				try
				{
					listeners[listenerCount] = new Listener(Queue.Default, transport, args[argumentCount], null);

					listeners[listenerCount].MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);
					
					Console.Error.WriteLine("Listening on: " + args[argumentCount]);
				}
				catch (RendezvousException exception)
				{
					Console.Error.WriteLine("Failed to create listener:");

					Console.Error.WriteLine(exception.StackTrace);
					
					System.Environment.Exit(1);
				}

				argumentCount++;
				
				listenerCount++;
			}

			while (true)
			{
				try
				{
					Queue.Default.Dispatch();
				}
				catch(RendezvousException exception)
				{
					Console.Error.WriteLine("Failed to dispatch default queue:");

					Console.Error.WriteLine(exception.StackTrace);

					break;
				}
			}

			GC.KeepAlive(listeners);

			try
			{
				TIBCO.Rendezvous.Environment.Close();
			}
			catch(RendezvousException exception)
			{				
				Console.Error.WriteLine("Failed to close environment:");

				Console.Error.WriteLine(exception.StackTrace);
				
				System.Environment.Exit(1);
			}
		}

		
		static void Usage()
		{

			Console.Out.Write("Usage: RendezvousListener [-service service] [-network network]");
			
			Console.Out.Write("                          [-daemon daemon] <subject-list>");
			
			System.Environment.Exit(1);
		}

		
		static int InitializeParameters(string[] args)
		{

			int i = 0;

			while ((i < (args.Length - 1)) && args[i].StartsWith("-"))
			{
				if (args[i].Equals("-service"))
				{
					service = args[i + 1];
					
					i += 2;
				}
				else if (args[i].Equals("-network"))
				{
					network = args[i + 1];

					i += 2;
				}
				else if (args[i].Equals("-daemon"))
				{
					daemon = args[i + 1];

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

			Console.Out.WriteLine("");

			Console.Out.WriteLine("Received on subject {0}...", message.SendSubject);

			Console.Out.WriteLine("{0}: {1}", DateTime.Now.ToString(), message.ToString());

			MessageField messageField = message.GetField(FIELD_NAME);

			SelfConvertingString selfConvertingString = (SelfConvertingString) messageField.Value;

			Console.Out.WriteLine("{0}: {1}", DateTime.Now.ToString(), selfConvertingString.ToString());

			Console.Out.WriteLine("");

			Console.Out.Flush();
		}
	

	}


}
