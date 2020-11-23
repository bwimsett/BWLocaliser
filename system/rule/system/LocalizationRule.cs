using System;
using System.Text.RegularExpressions;

namespace BWLocaliser.Rules {
	[Serializable]
	public abstract class LocalizationRule {

		protected string ruleIdentifier;

		public LocalizationRule() {
			SetRuleIdentifier();
		}
		
		/// <summary>
		/// Applies the rule to the given string.
		/// </summary>
		/// <param name="input"></param>
		public string Apply(string input) {
			// Extract rule identifier
			string inputRuleIdentifier = ExtractRuleIdentifier(input);
			
			// If the rule does not match this one, just return the input string
			if (!inputRuleIdentifier.Equals(ruleIdentifier)) {
				return input;
			}
			
			// Extract argument
			string arg = ExtractArgument(input);

			// If the argument type does not match this one, throw an exception
			if (!VerifyArgumentType(arg)) {
				throw new InvalidArgumentTypeException();
			}
			
			// Extract options
			string[] options = ExtractOptions(input) ;

			// Apply rule (must be implemented in subclasses)
			return ApplyRule(arg, options);
		}

		private string ExtractArgument(string input) {
			string regex = "^.*?(?=:)";
			string value = Regex.Match(input, regex).Value;
			return value;
		}

		private string ExtractRuleIdentifier(string input) {
			string regex = "(?<=:).*?(?=:)";
			string value = Regex.Match(input, regex).Value;
			return value;
		}

		private string[] ExtractOptions(string input) {
			// First remove the input rule identifer and argument from the rule
			string regex = ".*?:";
			input = Regex.Replace(input, regex, "");
			
			// Now split at each pipe (|)
			string[] options = input.Split('|');

			return options;
		}
		
		protected abstract bool VerifyArgumentType(string value);
		
		protected abstract void SetRuleIdentifier();
		
		protected abstract string ApplyRule(string value, string[] options);

	}
}

public class InvalidArgumentTypeException : Exception {
	
	public InvalidArgumentTypeException() {
	}
	
	public InvalidArgumentTypeException(string message) : base(message) {
	}

	public InvalidArgumentTypeException(string message, Exception inner) : base(message, inner) {
	}
	
}