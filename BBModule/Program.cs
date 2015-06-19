using System;
using System.Collections.Generic;
using Robotics.API;
using Robotics.API.PrimitiveSharedVariables;
using GPSRCmdGen;
using BBModule.CommandExecuters;

namespace BBModule
{
	/// <summary>
	/// Contains the program control logic
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Random Task generator
		/// </summary>
		private Generator gen;

		/// <summary>
		/// BB Module
		/// </summary>
		private Module module;

		/// <summary>
		/// Request the user to choose an option for random task generation.
		/// </summary>
		/// <returns>The user's option.</returns>
		private char GetOption()
		{
			ConsoleKeyInfo k;
			Console.WriteLine("Press q to quit, c to clear.");
			Console.Write("Enter category 1, 2, or 3: ");
			do
			{
				k = Console.ReadKey(true);
			} while ((k.KeyChar != 'q') && (k.KeyChar != 'c') && ((k.KeyChar < '1') || (k.KeyChar > '3')));
			Console.WriteLine(k.KeyChar);
			return k.KeyChar;
		}

		/// <summary>
		/// Gets a randomly generated task based on user input.
		/// </summary>
		/// <returns>A randonly generated task.</returns>
		/// <param name="opc">User option (category).</param>
		private void GetTask(char opc)
		{
			Response r;
			switch (opc)
			{
				case '1':
				case '2':
				case '3':
					module.CommandManager.BeginCommandExecution("get_speech_cmd", opc.ToString());
					return;

				case 'c': Console.Clear(); return;
				default: return;
			}
		}

		/// <summary>
		/// Checks if at least one of the required files are present. If not, initializes the 
		/// directory with example files
		/// </summary>
		public static void InitializePath()
		{
			int xmlFilesCnt = System.IO.Directory.GetFiles(Loader.ExePath, "*.xml", System.IO.SearchOption.TopDirectoryOnly).Length;
			if ((xmlFilesCnt < 1) && !System.IO.Directory.Exists(Loader.GetPath("grammars")))
				ExampleFilesGenerator.GenerateExampleFiles();
		}

		/// <summary>
		/// Starts the user input loop
		/// </summary>
		public void Run()
		{
			char opc = '\0';
			Setup();
			do
			{
				opc = GetOption();
				GetTask(opc);
			}
			while (opc != 'q');
		}

		/// <summary>
		/// Initializes the random task Generator and loads data from lists and storage
		/// </summary>
		private void Setup()
		{

			this.gen = new Generator();
			this.module = new Module("GPSR-CMD-GEN", 2007);
			module.CommandManager.SharedVariablesLoaded += (cmdMan) =>
			{
				Console.WriteLine("Shared variables loaded!");
				StringSharedVariable recognizedSpeech = new StringSharedVariable("recognizedSpeech");
				this.module.AddSharedVariable<StringSharedVariable>(ref recognizedSpeech);
			};

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("GPSR Generator 0.1 Beta");
			Console.WriteLine();
			Console.Write("Loading objects...");
			this.gen.LoadObjects();
			Console.Write("Loading names...");
			this.gen.LoadNames();
			Console.Write("Loading locations...");
			this.gen.LoadLocations();
			Console.Write("Loading gestures...");
			this.gen.LoadGestures();
			Console.Write("Loading predefined questions...");
			this.gen.LoadQuestions();
			Console.Write("Loading grammars...");
			this.gen.LoadGrammars();
			this.gen.ValidateLocations();
			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("Starting {0}", module.ConnectionManager.ModuleName);
			this.module.CommandManager.CommandExecuters.Add(new GetSpeechCmdExecutor(gen));
			Console.WriteLine("Waiting for Blackboard to connect");
			this.module.ConnectionManager.Start();
			this.module.WaitForClientToConnect();
			module.CommandManager.Start();
			Console.WriteLine("Connected!");
			this.module.CommandManager.Ready = true;
			Console.WriteLine("Running!");
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			InitializePath();
			new Program().Run();
		}
	}
}
