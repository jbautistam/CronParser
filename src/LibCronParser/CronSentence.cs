using System;

namespace Bau.Libraries.LibCronParser
{
	/// <summary>
	///		Clase con los datos de una sentencia de Cron
	/// </summary>
    public class CronSentence
    {
		public CronSentence()
		{
			Parts = new CronPart[]
							{
								new CronPart(CronPart.Part.Seconds),
								new CronPart(CronPart.Part.Minutes),
								new CronPart(CronPart.Part.Hours),
								new CronPart(CronPart.Part.DayOfMonth),
								new CronPart(CronPart.Part.Month),
								new CronPart(CronPart.Part.DayofWeek),
								new CronPart(CronPart.Part.Year)
							};
		}

		/// <summary>
		///		Interpreta la cadena cron
		/// </summary>
		public void Parse(string cron)
		{
			// Guarda la cadena a interpretar
			CronParsed = cron;
			// Intepreta la cadena
			if (string.IsNullOrWhiteSpace(cron))
				throw new CronParseException("Empty sentence");
			else
			{
				string [] cronFragments = Normalize(cron.Trim()).Split(' ');

					if (cronFragments.Length < 6 || cronFragments.Length > 7)
						throw new CronParseException("Arguments error");
					else
					{
						// Interpreta los valores
						for (int index = 0; index < cronFragments.Length; index++)
							Parse(cronFragments[index], Parts[index]);
						// Si no se ha definido el año, se interpreta como si fuera un asterisco
						if (cronFragments.Length == 6)
							Parse("*", Parts[(int) CronPart.Part.Year]);
					}
			}
		}

		/// <summary>
		///		Normaliza una cadena cron (sin tabuladores, saltos de línea o espacios dobles)
		/// </summary>
		private string Normalize(string cron)
		{
			// Quita los caracteres de espacio
			cron = cron.Replace('\t', ' ');
			cron = cron.Replace('\r', ' ');
			cron = cron.Replace('\n', ' ');
			// Quita los espacios dobles
			while (cron.IndexOf("  ") >= 0)
				cron = cron.Replace("  ", " ");
			// Devuelve la cadena sin espacios
			return cron.Trim();
		}

		/// <summary>
		///		Intepreta una parte de la cadena cron
		/// </summary>
		private void Parse(string fragment, CronPart part)
		{
			part.Parse(fragment);
		}

		/// <summary>
		///		Comprueba si una fecha está en el rango
		/// </summary>
		public bool IsAtRange(DateTime date)
		{
			// Comprueba si una fecha está en el rango
			for (int index = (int) CronPart.Part.Seconds; index <= (int) CronPart.Part.Year; index++)
				if (!Parts[index].IsAtRange(date))
					return false;
			// Si ha llegado hasta aquí es porque está en el rango
			return true;
		}

		/// <summary>
		///		Obtiene las siguientes fechas
		/// </summary>
        public System.Collections.Generic.IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, int count)
        {
			int lastYear = Parts[(int) CronPart.Part.Year].GetLastValue();
			int index = 0;

				// Recorre los valores
				while (baseTime.Year < lastYear && index < count)
				{
					// Incrementa la fecha
					baseTime = baseTime.AddSeconds(1);
					// Si es un valor válido, lo devuelve
					if (IsAtRange(baseTime))
					{
						index++;
						yield return baseTime;
					}
				}
        }

		/// <summary>
		///		Obtiene la cadena de depuración
		/// </summary>
		public string Debug()
		{
			string result = $"{CronParsed}{Environment.NewLine}";

				// Obtiene las cadenas de depuración de las partes
				foreach (CronPart part in Parts)
					result += $"\t{part.Debug()}{Environment.NewLine}";
				// Devuelve el resultado
				return result;
		}

		/// <summary>
		///		Secciones de la sentencia
		/// </summary>
		internal CronPart [] Parts { get; }

		/// <summary>
		///		Cadena interpretada inicialmente
		/// </summary>
		private string CronParsed { get; set; }
    }
}
