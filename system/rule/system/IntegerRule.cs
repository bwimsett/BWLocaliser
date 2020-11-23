namespace BWLocaliser.Rules {
	
	public abstract class IntegerRule : LocalizationRule {
		protected override bool VerifyArgumentType(string value) {
			int outInt = 0;
			return int.TryParse(value, out outInt);
		}
	}
	
}