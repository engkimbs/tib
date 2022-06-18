/// Copyright (c) 1998-$Date: 2016-12-13 12:47:31 -0800 (Tue, 13 Dec 2016) $ TIBCO Software Inc.
/// All Rights Reserved. Confidential & Proprietary.
/// TIB/Rendezvous is protected under US Patent No. 5,187,787.
/// For more information, please contact:
/// TIBCO Software Inc., Palo Alto, California, USA
using System;
using System.IO;
using System.Net;
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
	///  Sample sender application for custom data types.
	///  
	///  Optionally, a number of iterations may be specified:
	///  
	///  -iterations 20
	///  
	///  Optionally the user may specify communication parameters for 
	///  NetTransport constructor. If none are specified the following 
	///  defaults are used:
	///  
	///  -service "rendezvous" or "7500/udp"
	///  -network the result of gethostname
	///  -daemon "tcp:7500"
	/// </summary>
	class SelfConvertingStringSender
	{

		static string service = null;
		
		static string network = null;
		
		static string daemon = null;
		
		static int iterations = 1;

		
		const String FIELD_NAME = "SELF_CONVERTING_STRING";


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] args)
		{
			int argumentCount = InitializeParameters(args);

			if (argumentCount > (args.Length - 1))
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

			Message message = new Message();

			try
			{
				message.SendSubject = args[argumentCount++];
			}
			catch (RendezvousException exception) 
			{
				Console.Error.WriteLine("Failed to set send subject:");

				Console.Error.WriteLine(exception.StackTrace);
				
				System.Environment.Exit(1);
			}

			try
			{
				Console.Out.WriteLine("Publishing on subject {0}...", message.SendSubject);

				message.AddField(FIELD_NAME, (ICustomDataType) new SelfConvertingString("A self-converting string"));
					
				for (int i = 0; i < SelfConvertingStringSender.iterations; i++)
				{
					transport.Send(message);

					Console.Out.WriteLine("");

					Console.Out.WriteLine("Sent on subject {0}...", message.SendSubject);

					Console.Out.WriteLine(message.ToString());

					Console.Out.WriteLine("");
				}
			}
			catch (RendezvousException exception)
			{
				Console.Error.WriteLine("Failed to send message:");

				Console.Error.WriteLine(exception.StackTrace);

				System.Environment.Exit(1);
			}

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

			Console.Out.Write("Usage: SelfConvertingStringSender [-service service] [-network network]");
			Console.Out.Write("                                  [-daemon daemon] [-iterations n] <subject>");

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
				else
					if (args[i].Equals("-network"))
				{
					network = args[i + 1];

					i += 2;
				}
				else
					if (args[i].Equals("-daemon"))
				{
					daemon = args[i + 1];

					i += 2;
				}
				else
					if (args[i].Equals("-iterations"))
				{
					iterations = Int32.Parse(args[i + 1]);

					i += 2;
				}
				else
					Usage();
			}

			return i;
		}


	}	


}
