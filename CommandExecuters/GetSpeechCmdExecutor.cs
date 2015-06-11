using System;
using Robotics.API;
using GPSRCmdGen;

namespace GPSRCmdGen.CommandExecuters
{
	/// <summary>
	/// Implements a synchronous command execuer for the play command
	/// </summary>
	public class GetSpeechCmdExecutor : SyncCommandExecuter
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

		#region Inherited Methodos

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="command">Command object which contains the command to be executed</param>
		/// <returns>The Response object result of provided command execution. If no response is required, must return null</returns>
		/// <remarks>If the command execution is aborted the execution of this method is
		/// canceled and a failure response is sent if required</remarks>
		protected override Response SyncTask(Command command)
		{
			bool result = false;
			try
			{
				DifficultyDegree tier = DifficultyDegree.Unknown;
				switch (command.Parameters) {
					case "1": tier = DifficultyDegree.Easy; break;
					case "2": tier = DifficultyDegree.Moderate; break;
					case "3": tier = DifficultyDegree.High; break;
					default:
						throw new Exception();
				}

				Task t = this.gen.GenerateTask (tier);
				t.PrintTask();
				Response r = Response.CreateFromCommand(command, true);
				r.Parameters = t.ToString();
				return r;
			}
			catch
			{
				result = false;
			}

			return Response.CreateFromCommand(command, result);

		}

		#endregion
	}
}
