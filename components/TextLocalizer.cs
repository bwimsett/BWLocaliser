using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BWLocaliser.rule.rules;
using BWLocaliser.Rules;
using TMPro;
using UnityEngine;

/// <summary>
/// Component to automatically update and localise text fields.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocalizer : MonoBehaviour {

	private TextMeshProUGUI text;
	private string key;
	private Dictionary<string, object> args;
	private Dictionary<string, string> locArgIds;
	private Dictionary<string, Dictionary<string, string>> locArgsDictionary; // Stores a dictionary of tags and values for each localised placeholder

	private LocalizationRule[] rules;
	
	void Awake() {
		text = GetComponent<TextMeshProUGUI>();
		InitialiseRules();
		// Trigger refresh string event every time the locale changes
		LocalisationManager.LocaleChangedEvent += LocaleChangedEvent;
	}

	private void InitialiseRules() {
		rules = new LocalizationRule[] {
				// RULES GO HERE
				//--------------
				new IndexRule(), 
				new PluralRule()
				//--------------
		};
	}
	
	public void SetString(string key, Dictionary<string, object> args = null, Dictionary<string, string> locArgIds = null) {
		this.key = key;
		this.args = args;
		this.locArgIds = locArgIds;

		RefreshString();
	}

	public void RefreshString() {
		
		// Don't attempt if there is no key
		if (key == null) {
			return;
		}
		
		// Get the localised template from the CSV
		string template = LocalisationManager.GetString(key);

		// Fill the placeholders first
		template = FillPlaceholders(template);

		// Then fill the localised placeholders
		template = FillLocalisedPlaceholders(template);

		// Finally, apply rules
		template = ApplyRules(template);

		text.text = template;
	}

	private string FillPlaceholders(string input) {
		// Identify placeholders with regex

		/*
		 * Match any pattern that doesn't begin with an @, begins with {, followed by any number of characters
		 * except for {, and closes with }
		 */
		string regex = "(?<!@{)(?<={)[^{]*?(?=})";
		MatchCollection matches = Regex.Matches(input, regex);

		if (matches.Count == 0) {
			return input;
		}
		
		if (matches.Count > 0 && args == null) {
			Debug.LogError($"No arguments supplied for string: {key}", this);
			return input;
		}
		
		string output = input;
		
		// Loop through matches and fill gaps
		foreach (Match m in matches) {
			string matchValue = m.Value;
			
			object value = null;
			bool success = args.TryGetValue(matchValue, out value);

			if (!success) {
				Debug.LogError($"No argument given for {matchValue} in string {input}", this);
				continue;
			}

			regex = $"{{{matchValue}}}";
			output = Regex.Replace(output, regex, value.ToString());
		}

		return output;
	}
	
	private string FillLocalisedPlaceholders(string input) {

		string output = input;
		
		// First match any placeholders
		string regex = "(?<=@{).*?(?=})";
		MatchCollection matches = Regex.Matches(input, regex);

		if (matches.Count == 0) {
			return input;
		}
		
		if (matches.Count > 0 && locArgIds == null) {
			Debug.LogError($"No localised arguments supplied for string: {key}", this);
			return input;
		}
		
		Dictionary<string, Dictionary<string,string>> dictionaryCache = new Dictionary<string, Dictionary<string, string>>();
		
		// Loop through and replace matches
		foreach (Match m in matches) {
			string value = m.Value;
			// extract placeholder name and tag
			regex = "\\.(.*)";
			string placeholderName = Regex.Replace(value, regex, "");
			regex = "(?<=\\.).*";
			string tag = Regex.Match(value, regex).Value;
			
			// Now get the ID for the placeholder value
			string placeholderKey = "";
			bool success = locArgIds.TryGetValue(placeholderName, out placeholderKey);
			
			if (!success) {
				Debug.LogError($"No ID given for localised placeholder \"{placeholderName}\" in string: \"{key}\"", this);
				continue;
			}
			
			// Check the dictionary hasn't already been found
			Dictionary<string, string> placeholderDictionary = new Dictionary<string,string>();
			success = dictionaryCache.TryGetValue(placeholderName, out placeholderDictionary);
			
			// If it hasn't been cached, get it from the localisation settings
			if (!success) {
				placeholderDictionary = LocalisationManager.GetLocalisedStringDictionary(placeholderKey);
				dictionaryCache.Add(placeholderName, placeholderDictionary);
			}

			string replacement = "";

			if (placeholderDictionary == null) {
				continue;
			}

			bool noTag = String.IsNullOrEmpty(tag);
			// if the tag is null, use the "value" entry
			if (noTag) {
				success = placeholderDictionary.TryGetValue("value", out replacement);
				if (!success) {
					Debug.LogError($"No value in CSV entry for: {placeholderKey}", this);
				}
			}
			else {
				success = placeholderDictionary.TryGetValue(tag, out replacement);
				if (!success) {
					Debug.LogError($"No tag labelled as {tag} in CSV entry for: {placeholderKey}", this);
				}
			}
			
			// Now replace
			if (noTag) {
				regex = $"@{{{placeholderName}}}";
			}
			else {
				regex = $"@{{{placeholderName}.{tag}}}";
			}
			
			output = Regex.Replace(output, regex, replacement);
		}

		return output;
	}

	private string ApplyRules(string input) {
		// Match rules in the text
		string regex = "(?<={).*?(?=})";
		MatchCollection matches = Regex.Matches(input, regex);

		string output = input;
		
		// Loop through matches and apply all rules
		foreach (Match m in matches) {
			string value = m.Value;
			string outValue = value;
			
			// Apply each rule until one alters the input
			foreach (LocalizationRule rule in rules) {
				outValue = rule.Apply(value);
				if (outValue != value) {
					break;
				}
			}
			
			if (outValue.Equals(value)) {
				continue;
			}
			
			// Replace text
			string regexValue = value.Replace("|", "\\|");
			
			regex = $"{{{regexValue}}}";
			output = Regex.Replace(output, regex, outValue);
		}
		
		return output;
	}
	
	private void LocaleChangedEvent(object eventObject, EventArgs args) {
		RefreshString();
	}
	
}
