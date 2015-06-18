using System;
using System.Collections.Generic;
using System.Text;

namespace GPSRCmdGen
{
	/// <summary>
	/// Represents a task randomly generated from a grammar.
	/// The task is composed by list of tokens (original strings or
	/// wildcards with their replacements and metadata).
	/// </summary>
	public class Task
	{
		/// <summary>
		/// Stores the list of grammar's tokens
		/// </summary>
		private List<Token> tokens;

		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.Task"/> class.
		/// </summary>
		public Task (){
			this.tokens = new List<Token>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GPSRCmdGen.Task"/> class.
		/// </summary>
		/// <param name="tokens">The list of tokens to be used to build the task</param>
		public Task (List<Token> tokens)
		{
			if(tokens == null)return;
			this.tokens = new List<Token>(tokens);
		}

		/// <summary>
		/// Gets the list of tokens that compose the task.
		/// </summary>
		public List<Token> Tokens
		{
			get { return this.tokens; }
			internal set { this.tokens = value; }
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="GPSRCmdGen.Task"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="GPSRCmdGen.Task"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if(tokens.Count > 0)
			sb.Append(tokens[0].Name);
			for (int i = 1; i < tokens.Count; ++i)
			{
				sb.Append(tokens[i].Name);
			}
			return sb.ToString();
		}


		/// <summary>
		/// Prints a task including metadata into the output stream.
		/// </summary>
		public void PrintTask()
		{
			// switch Console color to white, backuping the previous one
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
			// Prints a === line
			string pad = String.Empty.PadRight (Console.BufferWidth - 1, '=');
			Console.WriteLine (pad);
			Console.WriteLine();
			// Prints task string and metadata
			Console.WriteLine(this.ToString().PadRight(4));
			this.PrintTaskMetadata();
			Console.WriteLine();
			// Prints another line
			Console.WriteLine (pad);
			// Restores Console color
			Console.ForegroundColor = pc;
			Console.WriteLine();
		}

		/// <summary>
		/// Prints the task metadata.
		/// </summary>
		private void PrintTaskMetadata()
		{
			Console.WriteLine();
			List<string> remarks = new List<string>();
			// Print named metadata
			foreach (Token token in this.Tokens)
				PrintMetadata(token, remarks);
			PrintRemarks(remarks);
		}

		/// <summary>
		/// Prints the metadata of the given Token
		/// </summary>
		/// <param name="token">The token onject containing the metadata to print</param>
		/// <param name="remarks">A list to store all metadata whose token has no name</param>
		private void PrintMetadata(Token token, List<string> remarks)
		{
			if (token.Metadata.Count < 1) return;
			// Store remarks for later
			if (String.IsNullOrEmpty(token.Name))
			{
				remarks.AddRange(token.Metadata);
				return;
			}
			// Print current token metadata
			Console.WriteLine("{0}", token.Name);
			foreach (string md in token.Metadata)
				Console.WriteLine("\t{0}", md);
		}

		/// <summary>
		/// Prints remaining metadata stored in the remarks list
		/// </summary>
		/// <param name="remarks">List of remarks strings</param>
		private static void PrintRemarks(List<string> remarks)
		{
			if (remarks.Count > 0)
			{
				Console.WriteLine("remarks");
				foreach (string r in remarks)
					Console.WriteLine("\t{0}", r);
			}
		}
		
		/*
			 * 
Go to the bedroom, find a person and tell the time.
Navigate to the kitchen, find a person and follow her.
Attend to the dinner-table, grasp the crackers, and take them to the side-table.
Go to the shelf, count the drinks and report to me.
Take this object and bring it to Susan at the hall.
Bring a coke to the person in the living room and answer him a question.
Offer a drink to the person at the door (robot needs to solve which drink will be delivered).

			*/

			/// 	answering,
			/// 	counting,
			/// 	finding,
			/// 	following,
			/// 	grasping,
			/// 	handling,
			/// 	navigating,
			/// 	opening,
			/// 	pouring,
			/// 	retrieving,
			/// 	saying
			/*
			 *********************************************************
			 * Count
			 *********************************************************
			 * Count the (ObjectCategory|AlikeObjects) at the (PlacementLocation)...
			 * Count the (People|PeopleByGender|PeopleByGesture) at the (Room)...
			 * ...and report to (me|Name (at|in|which is in|) the (Room).
			 * 
			 * (go|navigate) to the (PlacementLocation), count the (ObjectCategory|AlikeObjects)...
			 * (go|navigate) to the (Room) count the (People|PeopleByGender|PeopleByGesture)...
			 * ...and report to (me|Name (at|in|which is in) the (Room)).
			 * 
			 * Tell (me|to Name (at|in|which is in) the (Room))...
			 * ...how many (ObjectCategory|AlikeObjects) are in the (PlacementLocation).
			 * ...how many (People|PeopleByGender|PeopleByGesture) are in the (Room).
			 * 
			 * Grammar:
			 * $count  = $count1 | $count2 | $count3
			 * 
			 * $count1 = count the $cntxat $report
			 * $cntxat = $cntoat | $cntpat
			 * $cntoat = $object at the $PlacementLocation
			 * $cntpat = $people at the $Room
			 * 
			 * $count2 = $navigt $docntx $report 
			 * $navigt = $GoVerb to the 
			 * $docntx = $docnto | $docntp
			 * $docnto = $PlacementLocation, count the $object
			 * $docntp = $Room, count the $people
			 * 
			 * $count3 = Tell $target how many $ctable
			 * $ctable = $objain | $pplain
			 * $objain = $object are in the $PlacementLocation
			 * $pplain = $people are in the $Room
			 * 
			 * $object = objects | $ObjectCategory | $AlikeObjects
			 * $people = people | $PeopleByGender | $PeopleByGesture
			 * $report = and report to $target
			 * $target = me | ($Name (at | in | which is in) the $Room)
			 * 
			 * 
			 * 
			 *********************************************************
			 * 
			 *********************************************************
			 */
	}


}

