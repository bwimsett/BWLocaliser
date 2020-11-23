using BWLocaliser.Rules;

namespace BWLocaliser.rule.rules {
	public class PluralRule : IntegerRule {
		
		protected override void SetRuleIdentifier() {
			ruleIdentifier = "plural";
		}

		protected override string ApplyRule(string value, string[] options) {
			int valueInt = int.Parse(value);

			if (valueInt == 1) {
				return options[0];
			}

			return options[1];
		}
		
	}
}