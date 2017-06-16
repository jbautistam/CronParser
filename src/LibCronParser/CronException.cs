using System;

namespace Bau.Libraries.LibCronParser
{
	/// <summary>
	///		Excepción del intérprete de cron
	/// </summary>
	public class CronParseException : Exception
	{
		public CronParseException(string message) : base(message) {}

		public CronParseException(string message, Exception innerException) : base(message, innerException) {}
	}
}
