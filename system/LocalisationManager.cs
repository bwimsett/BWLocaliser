using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BWLocaliser;
using UnityEngine;

public class LocalisationManager {
	
	private static TextAsset gameText;

	private static Dictionary<string, string>[] dictionaries;
	private static Dictionary<string, Dictionary<string, string>> localisedPlaceholderDictionary;
	
	private static Locale locale = Locale.en;
	private static int keyColumn = 0;
	private static int languageStartColumn = 2; // Column index where

	private static bool initialised;

	public static event EventHandler LocaleChangedEvent;

	public static void Initialise (TextAsset gameText, Locale defaultLocale) {
		if (initialised) {
			return;
		}

		LocalisationManager.gameText = gameText;
		
		if (gameText == null) {
			Debug.LogError($"Localisation: gametext.txt is null. Attach it to the LocalisationInitialiser");
			return;
		}
		
		GenerateDictionaries();
		SetLocale(defaultLocale);
		
		initialised = true;
	}
	
	// -- RETRIEVAL -- //
	public static string GetString(string key) {
		key = key.ToLower();
			
		Dictionary<string, string> currentDictionary = dictionaries[(int) locale];
			
		string outString = "";

		bool success = currentDictionary.TryGetValue(key, out outString);

		if (!success) {
			Debug.Log($"!! Couldn't find string with key {key} !!");
		}
			
		return outString;
	}
	
	public static Dictionary<string, string> GetLocalisedStringDictionary(string key) {
		Dictionary<string, string> output = new Dictionary<string, string>();
		bool success = localisedPlaceholderDictionary.TryGetValue(key, out output);

		if (!success) {
			Debug.LogError($"No localised string in dictionary with key {key}");
		}

		return output;
	}
	
	// -- GENERATION -- //
	private static void GenerateDictionaries() {
		string[,] dataTable = ParseCSV(gameText);
			
		int width = dataTable.GetLength(0);
		int height = dataTable.GetLength(1);
			
		dictionaries = new Dictionary<string, string>[width];
		int dictionaryIndex = 0;
			
		// Loop through language columns, creating dictionary entries
		for (int x = languageStartColumn; x < width; x++){
				
			dictionaries[dictionaryIndex] = new Dictionary<string, string>();
				
			for (int y = 0; y < height; y++) {
				string key = dataTable[keyColumn, y];
				if (string.IsNullOrEmpty(key)) {
					continue;
				}
				dictionaries[dictionaryIndex].Add(dataTable[keyColumn, y], dataTable[x, y]);
			}

			dictionaryIndex++;
		}
	}

	private static Dictionary<string,string> GetCurrentDictionary() {
		return dictionaries[(int) locale];
	}
	
	private static string[,] ParseCSV(TextAsset textAsset) {
		string text = textAsset.text;

		string[] lines = text.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);

		// Convert the first line to calculate width
		//text = text.Replace('\n', '\r');
		string[] firstLine = lines[0].Split(',');
			
		// Create 2d array
		int width = firstLine.Length;
		int height = lines.Length;
			
		//Debug.Log($"Language CSV contains: {width*height} cells. {width} columns, {height} rows.");
			
		string[,] table = new string[width, height - 1];

		// Start at the second row (ignore title row)
		for (int y = 1; y < height; y++) {
			// Ignore any commas surrounded by speech marks
			string[] line = Regex.Split(lines[y], "(?<!\"\"\"),|,(?!\"\"\")");
			for (int x = 0; x < width; x++) {
				// Remove any speech marks surrounding commas
				line[x] = line[x].Replace("\"\"\",\"\"\"", ",");
				table[x, y-1] = line[x];
			}
		}

		return table;
	}
	
	private static void RefreshLocalisedPlaceholderDictionary() {

		/*
		 * Create a dictionary, where each entry has the string ID as key.
		 * Each value is a dictionary, where the keys are tag names, and the values are tag values.
		 * This means the dictionary can easily be looked up, first by string key, then by tag.
		 *
		 * Every placeholder has a "value" entry, which is used by default when calling @{placeholder} without a '.'
		 */
		
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

		Dictionary<string, string> currentDictionary = GetCurrentDictionary();
		
		// Loop through each localised argument and get the corresponding CSV entry
		foreach (KeyValuePair<string,string> pair in currentDictionary) {

			string locString = pair.Value;
			
			// Pattern match the entry to find tags first
			string regex = "\\[.*?=.*?\\]";
			MatchCollection matches = Regex.Matches(locString, regex);
			
			Dictionary<string, string> dictionaryValue = new Dictionary<string, string>();

			string value = "";
			
			// Loop through tags and extract the key and value
			foreach (Match m in matches) {
				regex = "(?<=\\[).*?(?=\\=)";
				string key = Regex.Match(m.Value, regex).Value;
				regex = "(?<==).*?(?=\\])";
				value = Regex.Match(m.Value, regex).Value;
				dictionaryValue.Add(key, value);
			}

			// Extract the value from the entry
			regex = "\\[.*?\\]";
			value = Regex.Replace(locString, regex, "");
			
			// Add the value to the dictionary
			dictionaryValue.Add("value", value);
			
			// Add the dictionary for this placeholder to the dictionary list
			dictionary.Add(pair.Key, dictionaryValue);
		}

		localisedPlaceholderDictionary = dictionary;
	}

	// -- SETTINGS -- //
	public static void SetLocale(Locale language) {
		locale = language;
		RefreshLocalisedPlaceholderDictionary();
		LocaleChangedEvent?.Invoke(null, EventArgs.Empty);
	}
}


