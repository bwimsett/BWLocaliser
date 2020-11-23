using UnityEngine;

namespace BWLocaliser {
	/// <summary>
	/// Should exist on an object somewhere in the first scene of the project, to initialise the localisation system.
	/// </summary>
	public class LocalisationInitializer : MonoBehaviour {

		public TextAsset gameText;
		public Locale defaultLocale;
		private Locale prevDefaultLocale;

		void Update() {
			if (prevDefaultLocale != defaultLocale) {
				LocalisationManager.SetLocale(defaultLocale);
				prevDefaultLocale = defaultLocale;
			}
		}
		
		private void Awake() {
			prevDefaultLocale = defaultLocale;
			LocalisationManager.Initialise(gameText, defaultLocale);
		}
	}
}