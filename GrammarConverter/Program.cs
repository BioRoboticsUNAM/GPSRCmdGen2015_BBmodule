using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarConverter
{
	class Program
	{
		static void Main(string[] args)
		{
			Program p = new Program();
			p.Setup();
			p.Run();
		}

		private GConverter converter;

		public Program()
		{
			converter = new GConverter();
		}

		private void Setup()
		{
			Console.WriteLine("GPSR Generator 0.1 Beta");
			Console.WriteLine();
			Console.Write("Loading objects...");
			this.converter.LoadObjects();
			Console.Write("Loading names...");
			this.converter.LoadNames();
			Console.Write("Loading locations...");
			this.converter.LoadLocations();
			Console.Write("Loading gestures...");
			this.converter.LoadGestures();
			Console.Write("Loading predefined questions...");
			this.converter.LoadQuestions();
			Console.Write("Loading grammars...");
			this.converter.LoadGrammars();
			// this.gen.ValidateLocations();
			Console.WriteLine();
			Console.WriteLine();
		}

		private void Run()
		{
			converter.ConvertAll();
		}
	}
}
