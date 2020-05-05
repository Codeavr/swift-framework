﻿using SwiftFramework.Core.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SwiftFramework.Editor
{
	internal class SymbolCatalog : ScriptableEditorSettings<SymbolCatalog>
    {
        public List<Symbol> list = new List<Symbol>()
        {
            new Symbol() { name = "ENABLE_ADMOB", enabled = false, description = "Admob Ads" },
            new Symbol() { name = "ENABLE_PLAYFAB", enabled = false, description = "Playfab" },
            new Symbol() { name = "ENABLE_NOTIFICATIONS", enabled = false, description = "Mobile Notifications" },
            new Symbol() { name = "ENABLE_FACEBOOK", enabled = false, description = "Facebook" },
            new Symbol() { name = "ENABLE_IAP", enabled = false, description = "Unity Purchasing" },
            new Symbol() { name = "ENABLE_APPSFLYER", enabled = false, description = "AppFlyer" },
            new Symbol() { name = "ENABLE_FIREBASE", enabled = false, description = "Firebase" },
            new Symbol() { name = "ENABLE_PLAYFABADMIN_API", enabled = false, description = "Playfab Admin API" },
            new Symbol() { name = "ENABLE_ECS", enabled = false, description = "ECS" },
            new Symbol() { name = "ENABLE_ADJUST", enabled = false, description = "Adjust SDK" },
            new Symbol() { name = "ENABLE_APPLOVIN_MAX", enabled = false, description = "Applovin Max ads" },
			new Symbol() { name = "ENABLE_APPMETRICA", enabled = false, description = "Yandex AppMetrica analytics" }
		};

		public void Apply()
		{
			List<string> defines = list
				.Where(x => x.style == SymbolStyle.Symbol && !string.IsNullOrEmpty(x.name) && x.enabled)
				.Select(x => x.name)
				.Distinct()
				.ToList();
		
			string defineSymbols = defines.Any() ? defines.Aggregate((a, b) => a + ";" + b) : string.Empty;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbols);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbols);
        }

		public void Revert()
		{
			string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

			IEnumerable<string> currentDefines = define.Replace(" ", "")
				.Split(new char[] { ';' })
				.Where(x => !string.IsNullOrEmpty(x));

			list.ForEach(symbol => symbol.enabled = currentDefines.Contains(symbol.name));

			foreach (string symbolName in currentDefines.Where(x => list.All(y => y.name != x)))
			{
				list.Add(new Symbol() { enabled = true, name = symbolName });
			}
		}

		public enum SymbolStyle
		{
			Symbol = 1,
			Separator = 10,
			Header,
		}

		[System.Serializable]
		public class Symbol
		{
			public SymbolStyle style = SymbolStyle.Symbol;

			public bool enabled { get; set; }

			public string name = "";

			public string description = "";
		}
	}

	internal class SymbolCatalogEditor : EditorWindow
	{
		private SymbolCatalog catalog;
		private string currentDefine;
		private string focus;
		private Vector2 scrollPosition;

		private static ReorderableList ro;
		private static GUIStyle styleTitle;
		private static GUIStyle styleHeader;
		private static GUIStyle styleName;
		private static GUIStyle styleDescription;
		private static readonly Color EnableStyleColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		private static readonly Color EnableTextColor = Color.white;
		private static readonly Color DisableStyleColor = Color.white;
		private static readonly Color DisableTextColor = new Color(1, 1, 1, 0.8f);

        private void Initialize()
		{
			if (styleDescription != null)
				return;

			styleTitle = new GUIStyle("IN BigTitle");
			styleTitle.alignment = TextAnchor.UpperLeft;
			styleTitle.fontSize = 12;
			styleTitle.stretchWidth = true;
			styleTitle.margin = new RectOffset();

			styleName = new GUIStyle(EditorStyles.label);
			styleName.active.textColor =
				styleName.normal.textColor =
				styleName.focused.textColor =
				styleName.hover.textColor = Color.white;

			styleDescription = new GUIStyle("HelpBox");
			styleDescription.richText = true;
			styleDescription.padding = new RectOffset(3, 3, 5, 1);
			styleDescription.fontSize = 10;

			styleHeader = new GUIStyle("VCS_StickyNote");
			styleHeader.richText = true;
			styleHeader.fontSize = 12;
			styleHeader.fontStyle = FontStyle.Bold;
			styleHeader.padding = new RectOffset(25, 3, 2, 2);
			styleHeader.alignment = TextAnchor.MiddleLeft;
			styleHeader.wordWrap = false;

			ro = new ReorderableList(new List<SymbolCatalog>(), typeof(SymbolCatalog));
			ro.drawElementCallback = DrawSymbol;
			ro.headerHeight = 0;
			ro.onAddDropdownCallback = (rect, list) =>
			{
				var gm = new GenericMenu();
				gm.AddItem(new GUIContent("Symbol"), false, () => AddSymbol(SymbolCatalog.SymbolStyle.Symbol));
				gm.AddItem(new GUIContent("Header"), false, () => AddSymbol(SymbolCatalog.SymbolStyle.Header));
				gm.AddItem(new GUIContent("Separator"), false, () => AddSymbol(SymbolCatalog.SymbolStyle.Separator));
				gm.DropDown(rect);
			};
			ro.onRemoveCallback = list => RemoveSymbol(catalog.list[list.index]);
			ro.onCanRemoveCallback = list => (0 <= list.index && list.index < catalog.list.Count);
			ro.elementHeight = 44;
			ro.onSelectCallback = (list) => EditorGUIUtility.keyboardControl = 0;

			minSize = new Vector2(300, 300);
		}

		[MenuItem("SwiftFramework/Define Symbols")]
        private static void OnOpenFromMenu()
		{
			EditorWindow.GetWindow<SymbolCatalogEditor>("Symbol Catalog");
		}

	 	private void OnGUI()
		{
			Initialize();

            catalog = SymbolCatalog.Instance;

			string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			if (currentDefine != define)
			{
				currentDefine = define;
				catalog.Revert();
			}

			using (var svs = new EditorGUILayout.ScrollViewScope(scrollPosition))
			{
				scrollPosition = svs.scrollPosition;
				EditorGUI.BeginChangeCheck();

				GUILayout.Label(new GUIContent("   Available Scripting Define Symbols", EditorGUIUtility.ObjectContent(catalog, typeof(SymbolCatalog)).image), styleTitle);

				ro.list = catalog.list;
				ro.DoLayoutList();

				using (new EditorGUI.DisabledGroupScope(EditorApplication.isCompiling))
				{
					using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
					{
						if (GUILayout.Button("Apply"))
						{
							catalog.Apply();
						}
					}
				}

				if (EditorGUI.EndChangeCheck())
					EditorUtility.SetDirty(catalog);
			}

			if (EditorApplication.isCompiling)
			{
				Repaint();
			}

			if (!string.IsNullOrEmpty(focus))
			{
				EditorGUI.FocusTextInControl(focus);
				focus = null;
			}
		}

        private void AddSymbol(SymbolCatalog.SymbolStyle style)
		{
			SymbolCatalog.Symbol symbol = new SymbolCatalog.Symbol(){ style = style };
			switch (style)
			{
				case SymbolCatalog.SymbolStyle.Symbol:
					symbol.name = "SYMBOL_NAME";
					symbol.description = "symbol description(<i>ritch-text is available</i>)";
					break;
				case SymbolCatalog.SymbolStyle.Header:
					symbol.name = "Header(<i>ritch-text is available</i>)";
					break;
				case SymbolCatalog.SymbolStyle.Separator:
					break;
			}

			catalog.list.Add(symbol);

			focus = string.Format("symbol neme {0}", catalog.list.IndexOf(symbol));

			EditorUtility.SetDirty(catalog);
		}

        private void RemoveSymbol(SymbolCatalog.Symbol symbol)
		{
			EditorApplication.delayCall += () =>
			{
				catalog.list.Remove(symbol);
				ro.index = Mathf.Clamp(ro.index, 0, catalog.list.Count - 1);
				EditorUtility.SetDirty(catalog);
				Repaint();
			};
		}

        private void DrawSymbol(Rect rect, int index, bool isActive, bool isFocused)
		{
			SymbolCatalog.Symbol symbol = ro.list[index] as SymbolCatalog.Symbol;

			switch (symbol.style)
			{
				case SymbolCatalog.SymbolStyle.Symbol:
					DrawDefaultSymbol(rect, symbol);
					break;
				case SymbolCatalog.SymbolStyle.Separator:
					GUI.Label(new Rect(rect.x + 10, rect.y + 24, rect.width - 20, 16), GUIContent.none, "sv_iconselector_sep");
					break;
				case SymbolCatalog.SymbolStyle.Header:
					DrawHeader(rect, symbol);
					break;
			}
			
			GUI.color = Color.white;
			GUI.contentColor = Color.white;
		}

        private void DrawHeader(Rect rect, SymbolCatalog.Symbol symbol)
		{
			int index = catalog.list.IndexOf(symbol);

			GUI.contentColor = Color.black;
			string symbolNameId = string.Format("symbol neme {0}", index);
			GUI.SetNextControlName(symbolNameId);
			styleHeader.richText = GUI.GetNameOfFocusedControl() != symbolNameId;
			symbol.name = GUI.TextField(new Rect(rect.x - 19, rect.y + rect.height - 24, rect.width + 23, 20), symbol.name, styleHeader);
			GUI.contentColor = Color.white;
		}

        private void DrawDefaultSymbol(Rect rect, SymbolCatalog.Symbol symbol)
		{
			int index = catalog.list.IndexOf(symbol);

			string symbolDescriptionId = string.Format("symbol desctription {0}", index);
			GUI.SetNextControlName(symbolDescriptionId);
			styleDescription.richText = GUI.GetNameOfFocusedControl() != symbolDescriptionId;
			symbol.description = GUI.TextArea(new Rect(rect.x, rect.y + 12, rect.width, rect.height - 13), symbol.description, styleDescription);

			GUI.color = symbol.enabled ? EnableStyleColor : DisableStyleColor;
			GUI.Label(new Rect(rect.x, rect.y, rect.width, 16), GUIContent.none, "ShurikenEffectBg");//"flow node flow" + (int)symbol.style);
			GUI.color = Color.white;

			symbol.enabled = GUI.Toggle(new Rect(rect.x + 5, rect.y, 15, 16), symbol.enabled, GUIContent.none);

			string symbolNameId = string.Format("symbol neme {0}", index);
			GUI.SetNextControlName(symbolNameId);
			GUI.color = symbol.enabled ? EnableTextColor : DisableTextColor;
			styleName.fontStyle = GUI.GetNameOfFocusedControl() != symbolNameId ? FontStyle.Bold : FontStyle.Normal;
			symbol.name = GUI.TextField(new Rect(rect.x + 20, rect.y, rect.width - 40, 16), symbol.name, styleName);
			GUI.color = Color.white;

			if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, 20), EditorGUIUtility.FindTexture("treeeditor.trash"), EditorStyles.label))
			{
				RemoveSymbol(symbol);
			}
		}
	}
}