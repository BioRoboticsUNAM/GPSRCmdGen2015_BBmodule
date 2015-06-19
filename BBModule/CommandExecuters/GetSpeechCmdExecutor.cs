using System;
using Robotics.API;
using GPSRCmdGen;
using Robotics.API.PrimitiveSharedVariables;

namespace BBModule.CommandExecuters
{
	/// <summary>
	/// Implements a synchronous command execuer for the play command
	/// </summary>
	public class GetSpeechCmdExecutor : AsyncCommandExecuter
	{
		#region Variables

		private Generator gen;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of GetSpeechCmdExecutor
		/// </summary>
		/// <param name="category">Category of speech command to be generated. [1-3]</param>
		public GetSpeechCmdExecutor(Generator g)
			: base("get_speech_cmd")
		{
			this.gen = g;
		}

		#endregion

		#region Properties

		public override bool ParametersRequired { get { return true; } }

		#endregion

		#region Inherited Methodos

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="command">Command object which contains the command to be executed</param>
		/// <returns>The Response object result of provided command execution. If no response is required, must return null</returns>
		/// <remarks>If the command execution is aborted the execution of this method is
		/// canceled and a failure response is sent if required</remarks>
		protected override Response AsyncTask(Command command)
		{
			Response r = Response.CreateFromCommand(command, false);

			DifficultyDegree tier = DifficultyDegree.Unknown;
			switch (command.Parameters)
			{
				case "1": tier = DifficultyDegree.Easy; break;
				case "2": tier = DifficultyDegree.Moderate; break;
				case "3": tier = DifficultyDegree.High; break;
				default:
					return r;
			}

			Task t = this.gen.GenerateTask(tier);
			t.PrintTask();

			r = Response.CreateFromCommand(command, true);
			r.Parameters = t.ToString();
			var svs = this.CommandManager.SharedVariables;
			if (svs.Contains("recognizedSpeech"))
			{
				var s = svs["recognizedSpeech"];
				var recognizedSpeech = svs["recognizedSpeech"] as StringSharedVariable;
				if (recognizedSpeech != null)
					recognizedSpeech.TryWrite(r.Parameters);
			}
			return r;

		}

		public override void DefaultParameterParser(string[] parameters)
		{
			
		}

		#endregion
	}
}
