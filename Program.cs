using System;
using System.Collections.Generic;
using Robotics.API;
using GPSRCmdGen.CommandExecuters;

namespace GPSRCmdGen
{
	/// <summary>
	/// Contains the program control logic
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Random Task generator
		/// </summary>
		Generator gen;

		/// <summary>
		/// BB Module
		/// </summary>
		Module module;

		/// <summary>
		/// Request the user to choose an option for random task generation.
		/// </summary>
		/// <returns>The user's option.</returns>
		private char GetOption(){
			ConsoleKeyInfo k;
			Console.WriteLine("Press q to quit, c to clear.");
			Console.Write("Enter category 1, 2, or 3: ");
			do {
				k = Console.ReadKey (true);
			} while((k.KeyChar != 'q') && (k.KeyChar != 'c') && ((k.KeyChar < '1') || (k.KeyChar > '3')));
			Console.WriteLine (k.KeyChar);
			return k.KeyChar;
		}

		/// <summary>
		/// Gets a randomly generated task based on user input.
		/// </summary>
		/// <returns>A randonly generated task.</returns>
		/// <param name="opc">User option (category).</param>
		private Task GetTask(char opc)
		{
			DifficultyDegree tier = DifficultyDegree.Unknown;
			switch (opc) {
				case '1': tier = DifficultyDegree.Easy; break;
				case '2': tier = DifficultyDegree.Moderate; break;
				case '3': tier = DifficultyDegree.High; break;
				case 'c': Console.Clear (); return null;
				default: return null;
			}

			Console.WriteLine("Choosen category {0}", opc);
			return gen.GenerateTask (tier);
		}

		/// <summary>
		/// Checks if at least one of the required files are present. If not, initializes the 
		/// directory with example files
		/// </summary>
		public static void InitializePath()
		{
			int xmlFilesCnt = System.IO.Directory.GetFiles (Loader.ExePath, "*.xml", System.IO.SearchOption.TopDirectoryOnly).Length;
			if ((xmlFilesCnt < 1) && !System.IO.Directory.Exists (Loader.GetPath("grammars")))
				ExampleFilesGenerator.GenerateExampleFiles ();
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
				Task task = GetTask(opc);
				if (task != null)
					task.PrintTask();
			}
			while(opc != 'q');
		}

		/// <summary>
		/// Initializes the random task Generator and loads data from lists and storage
		/// </summary>
		private void Setup ()
		{

			gen = new Generator ();

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine ("GPSR Generator 0.1 Beta");
			Console.WriteLine ();
			Console.Write ("Loading objects...");
			gen.LoadObjects();
			Console.Write ("Loading names...");
			gen.LoadNames ();
			Console.Write ("Loading locations...");
			gen.LoadLocations ();
			Console.Write ("Loading gestures...");
			gen.LoadGestures();
			Console.Write("Loading predefined questions...");
			gen.LoadQuestions();
			Console.Write ("Loading grammars...");
			gen.LoadGrammars ();
			gen.ValidateLocations ();
			Console.WriteLine ();
			Console.WriteLine ();

			this.module = new Module("GPSR_cmd_gen", 2007);
		    this.module.CommandManager.CommandExecuters.Add(new GetSpeechCmdExecutor(gen));
		    this.module.CommandManager.Ready = true;

		    this.module.Start();
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			InitializePath ();
			if (args.Length == 0) {
				new Program ().Run ();
				return;
			}
			ParseArgs (args);

		}

		/// <summary>
		/// Parses the arguments.
		/// </summary>
		/// <param name="args">Arguments given to the application.</param>
		private static void ParseArgs (string[] args)
		{
			int category;
			DifficultyDegree tier;
			Program p = new Program ();

			p.Setup ();
			foreach (string arg in args) {
				if (!Int32.TryParse (arg, out category) || (category < 1) || (category > 3)) {
					Console.WriteLine ("Invalid category input {0}", arg);
					continue;
				}
				switch (category) {
					case 1: tier = DifficultyDegree.Easy; break;
					case 2: tier = DifficultyDegree.Moderate; break;
					case 3: tier = DifficultyDegree.High; break;
					default: return;
				}
				p.gen.GenerateTask(tier).PrintTask();
			}
		}
	}
}
