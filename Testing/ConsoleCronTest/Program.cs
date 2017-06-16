using System;

using Bau.Libraries.LibCronParser;

namespace ConsoleCronTest
{
	class Program
	{
		static void Main(string [] args)
		{
			string [] tests = new string [] 
									{
										// S M H D M W Y
										"  * * * * * *", 
										// S  M  H D M W Y
										"  30 15 1 1 1 3 2015",
										// S  M  H D M W Y
										"  30/5 15-19 1 1 1 3 2015",
										// S    M     H     D M W Y
										"  30/5 15-19 1,3,7 1 1 3 2015",
										// S    M     H        D M W Y
										"  30/5 15-19 1,3,7-10 1 1 3 2015",
										// S    M     H             D M W Y
										"  30/5 15-19 1,3,7-10,18/2 1 1 3 2015",
										// S    M     H             D    M W   Y
										"  30/5 15-19 1,3,7-10,18/2 *    1 MON 2015",
										// S M   H     D M           W       Y
										"  0 0/5 14,18 ? JAN,MAR,SEP MON-FRI 2002-2010",
										// S M   H     D M     W       Y
										"  0 0/5 14,18 ? JAN/2 MON-FRI 2002-2010",
										// S M   H     D   M       W       Y
										"  0 0/5 14,18 1/7 JAN-SEP MON/2   2002-2010",
										// S M   H     D   M       W       Y
										"  0 0/5 14,18 1L  JAN-SEP MON/2   2002-2010",
										// S M   H     D   M       W       Y
										"  0 0/5 14,18 1L  JAN-SEP 3L      2002-2010",
										// S M   H     D   M       W       Y
										"  0 0/5 14,18 1L  JAN-SEP 3L      2002/3",
										// S M   H     D   M       W       Y
										"  0 15 10     ?   *       6#3      ",
										// S M      H     D   M       W       Y
										"  0 0-20/5 14,18 1/7 JAN-SEP MON/2   2002-2010",
										// S M      H     D   M         W       Y
										"  0 0-20/5 14,18 1/7 JAN-SEP/4 MON/2   2002-2010",
										// S M      H     D   M         W       Y
										"  0 0-20/5 14,18 1/7 JAN/4     MON/2   2002-2010",
									};

				// Interpreta las cadenas de pruebas
				for (int index = 0; index < tests.Length; index++)
				{
					TestParse(index, tests[index]);
					TestValues(index, tests[index]);
				}
				// Interpreta cadenas con error
				ParseExceptions();
				// Espera
				Console.ReadKey();
		}

		/// <summary>
		///		Interpreta las cadenas cron
		/// </summary>
		private static void TestParse(int index, string cron)
		{
			CronSentence sentence = new CronSentence();

				sentence.Parse(cron);
				Log($"Prueba {index}", sentence);
		}

		/// <summary>
		///		Interpreta los posibles valores de una cadena cron
		/// </summary>
		private static void TestValues(int index, string cron)
		{
			CronSentence sentence = new CronSentence();

				sentence.Parse(cron);
				Log($"Prueba {index}", sentence);
				foreach (DateTime value in sentence.GetNextOccurrences(DateTime.Now, 10))
					Console.WriteLine($"\t{value:yyyy-MM-dd HH:mm:ss}");
		}

		/// <summary>
		///		Interpreta cadenas cron con errores
		/// </summary>
		private static void ParseExceptions()
		{
			string[] errorTests = new string []
									{
										// S  M  H D M W Y
										"  30 15 1 1 1 3 205"
									};

				// Interpreta los errores
				for (int index = 0; index < errorTests.Length; index++)
					try
					{
						CronSentence sentence = new CronSentence();

							sentence.Parse(errorTests[index]);
					}
					catch (CronParseException exception)
					{
						Console.WriteLine($"Cron: {errorTests[index]}{Environment.NewLine}\tExcepción: {exception.Message}");
						Console.WriteLine(new string('-', 80));
					}
		}

		/// <summary>
		///		Log
		/// </summary>
		private static void Log(string title, CronSentence sentence)
		{
			Console.WriteLine(title);
			Console.WriteLine(sentence.Debug());
			Console.WriteLine(new string('-', 80));
		}
	}
}
