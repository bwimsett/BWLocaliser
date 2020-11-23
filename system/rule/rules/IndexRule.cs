using BWLocaliser.Rules;

namespace BWLocaliser.rule.rules {
	public class IndexRule : IntegerRule {
		
		protected override void SetRuleIdentifier() {
			ruleIdentifier = "index";
		}

		protected override string ApplyRule(string value, string[] options) {
			int index = int.Parse(value);

			if (index > options.Length) {
				return options[options.Length - 1];
			}

			return options[index];
		}
		
	}
}