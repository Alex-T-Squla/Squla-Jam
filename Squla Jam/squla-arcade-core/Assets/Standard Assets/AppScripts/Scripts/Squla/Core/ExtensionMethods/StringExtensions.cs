using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace Squla.Core.ExtensionMethods
{
	public class AnswerString : IEquatable<string>
	{
		private string answer;

		public AnswerString (string answer)
		{
			this.answer = Normalise(answer);
		}

		public static implicit operator AnswerString (string answer)
		{
			if (answer == null)
				return null;

			return new AnswerString (answer);
		}

		public bool Equals (string other)
		{
			return this.answer.Equals (Normalise(other));
		}

		public bool Equals (string other, StringComparison comparisonType)
		{
			return this.answer.Equals (Normalise(other), comparisonType);
		}

		private string Normalise(string s)
		{
			s = Regex.Replace (s, @"\u00A0", " ");
			s = Regex.Replace (s, @"‘", "'"); 
			s = Regex.Replace (s, @"’", "'");
			return s;
		}
	}
}
