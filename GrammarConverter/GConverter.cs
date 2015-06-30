using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using GPSRCmdGen;
using GPSRCmdGen.Containers;

namespace GrammarConverter
{
	/// <summary>
	/// Generates Random Sentences for the GPSR test
	/// </summary>
	public class GConverter
	{
		#region Variables

		/// <summary>
		/// Stores all known gestures
		/// </summary>
		private List<Gesture> allGestures;
		/// <summary>
		/// Stores all known locations
		/// </summary>
		private LocationManager allLocations;
		/// <summary>
		/// Stores all known names
		/// </summary>
		private List<PersonName> allNames;
		/// <summary>
		/// Stores all known objects
		/// </summary>
		private GPSRObjectManager allObjects;
		/// <summary>
		/// Stores all known questions
		/// </summary>
		private List<PredefindedQuestion> allQuestions;
		/// <summary>
		/// Stores all generation grammars
		/// </summary>
		private List<Grammar> allGrammars;

		/// <summary>
		/// The Xml Writer used to write de grammar into an Xml file
		/// </summary>
		private XmlWriter writer;
		/// <summary>
		/// The set of production rules for the current grammar 
		/// </summary>
		private Dictionary<string, ProductionRule> rules;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of Generator
		/// </summary>
		public GConverter()
		{
			// Initialize all objects
			this.allGestures = new List<Gesture> ();
			this.allLocations = LocationManager.Instance;
			this.allNames = new List<PersonName> ();
			this.allObjects = GPSRObjectManager.Instance;
			this.allGrammars = new List<Grammar> ();
			this.allQuestions = new List<PredefindedQuestion>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the list that stores all known locations
		/// </summary>
		public LocationManager AllLocations { get{return this.allLocations; } }

		/// <summary>
		/// Gets the list that stores all known gestures
		/// </summary>
		public List<Gesture> AllGestures { get{return this.allGestures; } }

		/// <summary>
		/// Gets the list that stores all known names
		/// </summary>
		public List<PersonName> AllNames{ get { return this.allNames; } }

		/// <summary>
		/// Stores all known objects
		/// </summary>
		public GPSRObjectManager AllObjects { get { return this.allObjects; } }

		/// <summary>
		/// Stores all known questions
		/// </summary>
		public List<PredefindedQuestion> AllQuestions { get { return this.allQuestions; } }

		#endregion

		#region Methods

		public void ConvertAll()
		{
			foreach (Grammar g in allGrammars)
				Convert(g);
		}

		private void Convert(Grammar g)
		{
			this.rules = new Dictionary<string, ProductionRule>(g.ProductionRules);
			Queue<ProductionRule> pending = new Queue<ProductionRule>(rules.Values);
			XmlWriterSettings settings = CreateWritterSettings();
			using (writer = XmlWriter.Create(g.Name + ".xml", settings))
			{
				WriteGrammarElement();
				WriteMainRule();
				while(pending.Count > 0){
					ProductionRule rule = pending.Dequeue();
					WriteRule(rule);
				}
				WriteLists();
				writer.Flush();
			}
		}

		private void WriteLists()
		{
			WriteGestures();
			WriteNames();
			WriteLocations();
			WriteObjectsAndCategories();
			WriteQuestions();
		}

		private static XmlWriterSettings CreateWritterSettings()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.CheckCharacters = true;
			settings.CloseOutput = true;
			settings.Encoding = System.Text.Encoding.UTF8;
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = Environment.NewLine;
			return settings;
		}

		private void WriteGrammarElement()
		{
			/*
			 * <grammar xmlns="http://www.w3.org/2001/06/grammar"
		 tag-format=
		 mode="voice"
		 xml:lang="en-US"
		 version="1.0"
		 root="Objects">
			*/
			writer.WriteStartElement("grammar");
			writer.WriteAttributeString("tag-format", "semantics/1.0");
			writer.WriteAttributeString("mode", "voice");
			writer.WriteAttributeString("lang", "xml", "en-US");
			writer.WriteAttributeString("version", "1.0");
			writer.WriteAttributeString("root", "Main");
		}

		private void WriteRule(ProductionRule rule)
		{
			if (rule.NonTerminal == "$Main")
				return;

			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", rule.NonTerminal.Substring(1));
			WriteProductions(rule);
			writer.WriteEndElement();
		}

		private void WriteMainRule()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "Main");
			WriteProductions(rules["$Main"]);
			writer.WriteEndElement();
		}

		private void WriteProductions(ProductionRule productionRule)
		{
			if (productionRule.Replacements.Count < 1)
				return;
			else if (productionRule.Replacements.Count < 2)
			{
				WriteReplacement(productionRule.Replacements[0]);
				return;
			}
			
			writer.WriteStartElement("one-of");
			foreach (var replacement in productionRule.Replacements)
				WriteReplacement(replacement);
			writer.WriteEndElement();
		}

		private void WriteReplacement(string replacement)
		{
			writer.WriteStartElement("item");

			int cc = 0;
			while (cc < replacement.Length)
			{
				if (replacement[cc] == '$')
				{
					string nt = FetchNonTerminal(replacement, ref cc);
					if(!rules.ContainsKey(nt))
						writer.WriteValue(nt);
					else
						WriteRuleRef(nt.Substring(1));
				}
				else if (replacement[cc] == '{')
				{
					string wc = FetchWildCard(replacement, ref cc);
					if (wc == "void")
						continue;
					WriteRuleRef(wc);
				}
				else
				{
					string t = FetchTerminals(replacement, ref cc);
					writer.WriteValue(t);
				}
			}

			
			writer.WriteEndElement();
		}

		private void WriteRuleRef(string rule)
		{
			writer.WriteStartElement("ruleref");
			writer.WriteAttributeString("uri", "#" + rule);
			writer.WriteEndElement();
		}

		private string FetchWildCard(string replacement, ref int cc)
		{
			++cc;
			Scanner.SkipSpaces(replacement, ref cc);
			int bcc = cc;
			while ((cc < replacement.Length) && Scanner.IsLAlpha(replacement[cc])) ++cc;
			string wcName = replacement.Substring(bcc, cc - bcc);
			int bOpen = 1;
			while (cc < replacement.Length && (bOpen > 0))
			{
				if (replacement[cc] == '{') ++bOpen;
				else if (replacement[cc] == '}') --bOpen;
				++cc;
			}

			switch (wcName)
			{
				case "category":
					return "__categories__";

				case "gesture":
					return "__gestures__";

				case "name": case "female": case "male":
					return "__names__";

				case "location": case "beacon": case "placement": case "room":
					return "__locations__";

				case "object": case "aobject": case "kobject":
					return "__objects__";

				case "question":
					return "__questions__";

				case "void":
					return "void";
			}
			return String.Format("__{0}__", wcName);
		}

		private string FetchNonTerminal(string s, ref int cc)
		{
			char c;
			int bcc = cc++;
			while (cc < s.Length)
			{
				c = s[cc];
				if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z')) || (c == '_'))
					++cc;
				else
					break;
			}
			return s.Substring(bcc, cc - bcc);
		}

		private string FetchTerminals(string replacement, ref int cc)
		{
			int bcc = cc;
			while (cc < replacement.Length && (replacement[cc] != '{') && (replacement[cc] != '$')) ++cc;

			return replacement.Substring(bcc, cc - bcc);
		}

		private void WriteGestures()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__gestures__");
			writer.WriteStartElement("one-of");
			foreach (INameable element in this.allGestures)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(element.Name);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void WriteNames()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__names__");
			writer.WriteStartElement("one-of");
			foreach (INameable element in this.allNames)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(element.Name);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void WriteLocations()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__locations__");
			writer.WriteStartElement("one-of");
			foreach (INameable element in this.allLocations)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(element.Name);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void WriteObjectsAndCategories()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__objects__");
			writer.WriteStartElement("one-of");
			Dictionary<string,Category> categories = new Dictionary<string,Category>();
			foreach (GPSRObject o in this.allObjects.Objects)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(o.Name);
				if(!categories.ContainsKey(o.Category.Name))
					categories.Add(o.Category.Name, o.Category);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__categories__");
			writer.WriteStartElement("one-of");
			foreach (Category c in categories.Values)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(c.Name);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void WriteQuestions()
		{
			writer.WriteStartElement("rule");
			writer.WriteAttributeString("id", "__questions__");
			writer.WriteStartElement("one-of");
			foreach (PredefindedQuestion q in this.allQuestions)
			{
				writer.WriteStartElement("item");
				writer.WriteValue(q.Question);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}



		#endregion

		#region Load Methods

		/// <summary>
		/// Loads the set of gestures from disk. If no gestures file is found, 
		/// the default set is loaded from Factory
		/// </summary>
		public void LoadGestures ()
		{
			try {
				this.allGestures = Loader.Load<GestureContainer> (Loader.GetPath("Gestures.xml")).Gestures;
				Green("Done!");
			} catch {
				Err("Failed! Application terminated");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// Loads the grammars from disk. If no grammars are found, the application is
		/// terminated.
		/// </summary>
		public void LoadGrammars ()
		{
			try {
				this.allGrammars = Loader.LoadGrammars ();
				Green("Done!");
			} catch {

				Err ("Failed! Application terminated");
				Environment.Exit (0);
			}
		}

		/// <summary>
		/// Loads the set of locations from disk. If no locations file is found, 
		/// the default set is loaded from Factory
		/// </summary>
		public void LoadLocations ()
		{
			try {
			this.allLocations = Loader.LoadLocations (Loader.GetPath("Locations.xml"));
				Green("Done!");
			} catch {
				Err("Failed! Application terminated");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// Loads the set of names from disk. If no names file is found, 
		/// the default set is loaded from Factory
		/// </summary>
		public void LoadNames ()
		{
			try {
				this.allNames = Loader.Load<NameContainer> (Loader.GetPath("Names.xml")).Names;
				Green("Done!");
			} catch {
				Err("Failed! Application terminated");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// Loads the set of objects and categories from disk. If no objects file is found, 
		/// the default set is loaded from Factory
		/// </summary>
		public void LoadObjects ()
		{
			try {
				this.allObjects = Loader.LoadObjects(Loader.GetPath("Objects.xml"));
				Green("Done!");
			} catch {
				Err("Failed! Application terminated");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// Loads the set of questions from disk. If no questions file is found, 
		/// the default set is loaded from Factory
		/// </summary>
		public void LoadQuestions()
		{
			try
			{
				this.allQuestions = Loader.Load<QuestionsContainer>(Loader.GetPath("Questions.xml")).Questions;
				Green("Done!");
			}
			catch
			{
				Err("Failed! Application terminated");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// Validates all default locations of categories are contained in the locations array.
		/// </summary>
		public void ValidateLocations()
		{
			throw new NotImplementedException();
			//foreach(Category c in this.AllObjects.Categories)
			//{
			//    if (!this.AllLocations.Contains (c.DefaultLocation))
			//        this.AllLocations.Add (c.DefaultLocation);
			//}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Writes the provided exception's Message to the console in RED text
		/// </summary>
		/// <param name="ex">Exception to be written.</param>
		public static void Err(Exception ex){
			Err (null, ex);
		}

		public static void Err(string format, params object[] args)
		{
			Err (String.Format (format, args));
		}

		/// <summary>
		/// Writes the provided message string to the console in RED text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Err(string message)
		{
			Err(message, (Exception)null);
		}

		/// <summary>
		/// Writes the provided message string and exception's Message to the console in RED text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		/// <param name="ex">Exception to be written.</param>
		public static void Err(string message, Exception ex)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			if(!String.IsNullOrEmpty(message))
				Console.WriteLine (message);
			if(ex != null)
				Console.WriteLine ("Exception {0}:", ex.Message);
			Console.ForegroundColor = pc;
		}

		/// <summary>
		/// Writes the provided exception's Message to the console in YELLOW text
		/// </summary>
		/// <param name="ex">Exception to be written.</param>
		public static void Warn(Exception ex)
		{
			Err (null, ex);
		}

		public static void Warn(string format, params object[] args)
		{
			Err (String.Format (format, args));
		}

		/// <summary>
		/// Writes the provided message string to the console in YELLOW text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Warn(string message)
		{
			Err(message, (Exception)null);
		}

		/// <summary>
		/// Writes the provided message string and exception's Message to the console in YELLOW text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		/// <param name="ex">Exception to be written.</param>
		public static void Warn(string message, Exception ex)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			if(!String.IsNullOrEmpty(message))
				Console.WriteLine (message);
			if(ex != null)
				Console.WriteLine ("Exception {0}:", ex.Message);
			Console.ForegroundColor = pc;
		}

		/// <summary>
		/// Writes the provided message string to the console in GREEN text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Green(string message)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine (message);
			Console.ForegroundColor = pc;
		}

		#endregion
	}
}

