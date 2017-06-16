using System;
using System.Collections;
using System.Collections.Generic;

namespace Bau.Libraries.LibCronParser
{
	/// <summary>
	///		Clase con los datos de una parte interpretada de una cdena cron
	/// </summary>
    internal class CronPart
    {
		/// <summary>
		///		Parte de una cadena cron
		/// </summary>
		internal enum Part
		{
			/// <summary>Segundos</summary>
			Seconds,
			/// <summary>Minutos</summary>
			Minutes,
			/// <summary>Horas</summary>
			Hours,
			/// <summary>Día del mes</summary>
			DayOfMonth,
			/// <summary>Mes</summary>
			Month,
			/// <summary>Día de la semana</summary>
			DayofWeek,
			/// <summary>Año (opcional)</summary>
			Year
		}
		/// <summary>
		///		Modificadores
		/// </summary>
		internal enum ValueModifier
		{
			/// <summary>Ningún modificador</summary>
			None,
			/// <summary>Para los datos actuales</summary>
			Actual,
			/// <summary>El último</summary>
			Last,
			/// <summary>Intervalo de días de la semana</summary>
			DayOfWeekInterval
		}

		// Variables privadas
		private BitArray _posibleValues;
		private int _validStart, _validEnd;

		internal CronPart(Part type)
		{
			PartType = type;
			InitLimits(type);
		}

		/// <summary>
		///		Inicializa los límites
		/// </summary>
		private void InitLimits(Part type)
		{
			// Inicializa los límites
			switch (type)
			{
				case Part.Seconds:
				case Part.Minutes:
						InitValid(0, 59);
					break;
				case Part.Hours:
						InitValid(0, 23);
					break;
				case Part.Month:
						InitValid(0, 11);
						Values = new List<string> 
											{ 
												"January", "February", "March", "April", "May", "June", 
												"July", "August", "September", "October", "November", "December" 
											};
					break;
				case Part.DayOfMonth:
						InitValid(1, 31);
					break;
				case Part.DayofWeek:
						InitValid(1, 7);
						Values = new List<string> 
											{ 
												"Sunday", "Monday", "Tuesday", "Wednesday", 
												"Thursday", "Friday", "Saturday" 
											};
					break;
				case Part.Year:
						InitValid(DateTime.Now.Year - 100, DateTime.Now.Year + 100);
					break;
			}
		}

		/// <summary>
		///		Inicializa los datos válidos
		/// </summary>
		private void InitValid(int start, int end)
		{
			_validStart = start;
			_validEnd = end;
		}

		/// <summary>
		///		Interpreta una parte de la cadena cron
		/// </summary>
		internal void Parse(string cron)
		{
			// Guarda la cadena interpretada
			CronParsed = cron;
			// Interpreta la cadena
			if (string.IsNullOrWhiteSpace(cron))
				ThrowException($"Empty part");
			else
			{
				// Inicializa los valores posibles
				_posibleValues = new BitArray(_validEnd - _validStart + 1);
				Modifier = ValueModifier.None;
				// Interpreta recursivamente la cadena
				ParsePart(cron, true);
			}
			// Comprueba las excepciones
			if (Modifier == ValueModifier.Last && PartType != Part.DayOfMonth && PartType != Part.DayofWeek)
				ThrowException("The modifier L only can apply on day of month or week");
		}

		/// <summary>
		///		Obtiene el último valor
		/// </summary>
		internal int GetLastValue()
		{
			// Busca el último valor
			for (int index = _posibleValues.Length - 1; index >= 0; index--)
				if (_posibleValues[index])
					return index + _validStart;
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return -1;
		}

		/// <summary>
		///		Interpreta una parte de la cadena
		/// </summary>
		private void ParsePart(string cron, bool first)
		{
			// Quita los espacios
			cron = cron.Trim();
			// Recoge los valores
			if (cron == "*")
			{
				if (!first)
					ThrowException($"Parse error");
				else
					FillPosibleValues(_validStart, _validEnd);
			}
			else if (cron == "?")
			{
				if (!first)
					ThrowException($"Parse error");
				else
				{
					Modifier = ValueModifier.Actual;
					FillPosibleValues(_validStart, _validEnd);
				}
			}
			else if (cron.IndexOf(',') >= 0) //? ... la enumeración se debe evaluar antes que el rango
				ParseEnumeration(cron);
			else if (cron.IndexOf('-') >= 0)
				ParseRange(cron);
			else if (cron.IndexOf('/') >= 0)
				ParseEvery(cron);
			else if (cron.IndexOf('#') >= 0)
				ParseDayInterval(cron);
			else
				AssignValue(ParseValue(cron));
		}

		/// <summary>
		///		Rellean los valores posible
		/// </summary>
		private void FillPosibleValues(int start, int end, int interval = 1)
		{
			if (interval < 1)
				ThrowException("Out of range");
			for (int index = start; index <= end; index += interval)
				AssignValue(index);
		}

		/// <summary>
		///		Asigna un valor posible
		/// </summary>
		private void AssignValue(int value)
		{
			if (value == -1 && Modifier == ValueModifier.Last)
				return;
			if (value < _validStart || value > _validEnd)
				ThrowException("Value out of range");
			_posibleValues[GetPossibleValueIndex(value)] = true;
		}

		/// <summary>
		///		Obtiene el índice de un valor en el array de bytes de valores posibles
		/// </summary>
		private int GetPossibleValueIndex(int index)
		{
			return index - _validStart;
		}

		/// <summary>
		///		Interpreta un valor
		/// </summary>
		private int ParseValue(string cron)
		{
			int value;
			
				// Interpreta el último valor
				if (cron.EndsWith("L", StringComparison.CurrentCultureIgnoreCase))
				{
					Modifier = ValueModifier.Last;
					if (cron.Length > 0)
						cron = cron.Substring(0, cron.Length - 1);
				}
				// Interpreta el valor de la cadena (ENE ó MON)
				value = ParseValueString(cron);
				// Si no se trataba de una cadena, se interpreta el valor numérico
				if (value < 0)
				{
					// Quita los espacios
					cron = cron.Trim();
					// Interpreta el valor numérico
					if (!IsNumeric(cron) || !int.TryParse(cron, out value))
						ThrowException($"Non numeric argument");
				}
				// Devuelve el valor
				return value;
		}

		/// <summary>
		///		Comprueba si un valor es numérico
		/// </summary>
		private bool IsNumeric(string cron)
		{
			// Comprueba cada uno de los caracteres
			if (string.IsNullOrWhiteSpace(cron))
				return false;
			else
				foreach (char chr in cron)
					if (!char.IsDigit(chr))
						return false;
			// Si ha llegado hasta aquí es porque todos eran numéricos
			return true;
		}

		/// <summary>
		///		Interpreta los valores de cadena
		/// </summary>
		private int ParseValueString(string cron)
		{
			// Compara los valores
			foreach (string value in Values)
				if (cron.Equals(value, StringComparison.CurrentCultureIgnoreCase) ||
					cron.Equals(value.Substring(0, 3), StringComparison.CurrentCultureIgnoreCase))
				{
					int index = Values.IndexOf(value);

						// Los días de la semana comienzan por 1
						if (PartType == Part.DayofWeek)
							index++;
						// Devuelve el índice
						return index;
				}
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return -1;
		}

		/// <summary>
		///		Interpreta un rango
		/// </summary>
		private void ParseRange(string cron)
		{
			string[] fragments = cron.Split('-');

				if (fragments.Length != 2)
					ThrowException("A range must have two parts");
				else if (fragments[1].IndexOf('/') >= 0)
				{
					string[] parts = fragments[1].Split('/');

						FillPosibleValues(ParseValue(fragments[0]), ParseValue(parts[0]), ParseValue(parts[1]));
				}
				else
					FillPosibleValues(ParseValue(fragments[0]), ParseValue(fragments[1]));
		}

		/// <summary>
		///		Interpreta un rango cada cierto tiempo: 1/5 desde 1 cada 5 (1, 6, 11...)
		/// </summary>
		private void ParseEvery(string cron)
		{
			string[] fragments = cron.Split('/');

				if (fragments.Length != 2)
					ThrowException("A interval must have two parts");
				else
					FillPosibleValues(ParseValue(fragments[0]), _validEnd, ParseValue(fragments[1]));
		}

		/// <summary>
		///		Interpreta un enumerado
		/// </summary>
		private void ParseEnumeration(string cron)
		{
			string [] fragments = cron.Split(',');

				if (fragments.Length < 2)
					ThrowException("A enumeration must have at least two values");
				else
					foreach (string fragment in fragments)
						if (string.IsNullOrWhiteSpace(fragment))
							ThrowException("Argument missing");
						else if (fragment.IndexOf('-') >= 0)
							ParseRange(fragment);
						else if (fragment.IndexOf('/') >= 0)
							ParseEvery(fragment);
						else
							AssignValue(ParseValue(fragment));
		}

		/// <summary>
		///		Interpreta un intervalo de días
		/// </summary>
		private void ParseDayInterval(string cron)
		{
			if (PartType != Part.DayofWeek)
				ThrowException("The modifier # only is valid at day of week");
			else
			{
				string [] parts = cron.Split('#');

					if (parts.Length != 2)
						ThrowException("Invalid arguments");
					else
					{
						// Inicializa los valores
						DayOfWeekStart = ParseValue(parts[0]);
						DayOfWeekInterval = ParseValue(parts[1]);
						// Comprueba los datos
						if (DayOfWeekStart < 1 || DayOfWeekStart > 7 || DayOfWeekInterval < 1 || DayOfWeekInterval > 5)
							ThrowException("Out of range");
					}
			}
		}

		/// <summary>
		///		Lanza la excepción
		/// </summary>
		private void ThrowException(string message)
		{
			throw new CronParseException($"{message}. Parsed: {CronParsed}. Section: {PartType}");
		}

		/// <summary>
		///		Comprueba si una fecha está en el rango
		/// </summary>
		internal bool IsAtRange(DateTime date)
		{
			switch (PartType)
			{
				case Part.Seconds:
					return IsAtRange(date.Second);
				case Part.Minutes:
					return IsAtRange(date.Minute);
				case Part.Hours:
					return IsAtRange(date.Hour);
				case Part.DayOfMonth:
					return IsAtRange(date.Day);
				case Part.Month:
					return IsAtRange(date.Month - 1);
				case Part.DayofWeek:
					return IsAtRange(((int) date.DayOfWeek) + 1);
				case Part.Year:
					return IsAtRange(date.Year);
				default:
					return false;
			}
		}

		/// <summary>
		///		Comprueba si un valor está en el rango
		/// </summary>
		private bool IsAtRange(int value)
		{
			return _posibleValues[GetPossibleValueIndex(value)];
		}

		/// <summary>
		///		Obtiene la cadena de interpretación
		/// </summary>
		public string Debug()
		{
			string debug = "";

				// Añade los valores 
				if (DayOfWeekStart != -1)
					debug += $"{Values[DayOfWeekStart - 1]}#{DayOfWeekInterval}";
				else
					for (int index = 0; index < _posibleValues.Length; index++)
						if (_posibleValues[index])
						{	
							if (!string.IsNullOrEmpty(debug))
								debug += ", ";
							debug += $"{_validStart + index}";
							if (Modifier == ValueModifier.Last)
								debug += " (LAST)";
						}
				// Devuelve la cadena de depuración
				return $"- Parsed: {CronParsed} - Part: {PartType} - Values: {debug}";
		}

		/// <summary>
		///		Parte a la que corresponde
		/// </summary>
		internal Part PartType { get; }

		/// <summary>
		///		Lista de valores posibles (días de la semana, meses)
		/// </summary>
		internal List<string> Values { get; private set; } = new List<string>();

		/// <summary>
		///		Modificador
		/// </summary>
		internal ValueModifier Modifier { get; private set; } = ValueModifier.None;

		/// <summary>
		///		Cadena cron interpretada
		/// </summary>
		internal string CronParsed { get; private set; }

		/// <summary>
		/// 	Inicio de un intervalo de días de la semana
		/// </summary>
		internal int DayOfWeekStart { get; private set; } = -1;

		/// <summary>
		/// 	Intervalo de días de la semana
		/// </summary>
		internal int DayOfWeekInterval { get; private set; } = -1;
    }
}
