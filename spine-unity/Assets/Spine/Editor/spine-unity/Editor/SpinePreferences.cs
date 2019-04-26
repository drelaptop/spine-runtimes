/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2019, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
#endif

using UnityEngine;
using UnityEditor;

namespace Spine.Unity.Editor {

	public class SpinePreferences : ScriptableObject {

		public const string SPINE_SETTINGS_ASSET_PATH = "Assets/Editor/SpineSettings.asset";

		#if SPINE_TK2D
		const float DEFAULT_DEFAULT_SCALE = 1f;
		#else
		internal const float DEFAULT_DEFAULT_SCALE = 0.01f;
		#endif
		public float defaultScale = DEFAULT_DEFAULT_SCALE;

		internal const float DEFAULT_DEFAULT_MIX = 0.2f;
		public float defaultMix = DEFAULT_DEFAULT_MIX;

		internal const string DEFAULT_DEFAULT_SHADER = "Spine/Skeleton";
		public string defaultShader = DEFAULT_DEFAULT_SHADER;

		internal const float DEFAULT_DEFAULT_ZSPACING = 0f;
		public float defaultZSpacing = DEFAULT_DEFAULT_ZSPACING;

		internal const bool DEFAULT_DEFAULT_INSTANTIATE_LOOP = true;
		public bool defaultInstantiateLoop = DEFAULT_DEFAULT_INSTANTIATE_LOOP;

		internal const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
		public bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

		internal const bool DEFAULT_SET_TEXTUREIMPORTER_SETTINGS = true;
		public bool setTextureImporterSettings = DEFAULT_SET_TEXTUREIMPORTER_SETTINGS;

		internal const bool DEFAULT_ATLASTXT_WARNING = true;
		public bool atlasTxtImportWarning = DEFAULT_ATLASTXT_WARNING;

		internal const bool DEFAULT_TEXTUREIMPORTER_WARNING = true;
		public bool textureImporterWarning = DEFAULT_TEXTUREIMPORTER_WARNING;

		public const float DEFAULT_MIPMAPBIAS = -0.5f;

		public const bool DEFAULT_AUTO_RELOAD_SCENESKELETONS = true;
		public bool autoReloadSceneSkeletons = DEFAULT_AUTO_RELOAD_SCENESKELETONS;

		internal const float DEFAULT_SCENE_ICONS_SCALE = 1f;
		public const string SCENE_ICONS_SCALE_KEY = "SPINE_SCENE_ICONS_SCALE";

	#if NEW_PREFERENCES_SETTINGS_PROVIDER
		public static void Load () {
			SpineHandles.handleScale = EditorPrefs.GetFloat(SCENE_ICONS_SCALE_KEY, DEFAULT_SCENE_ICONS_SCALE);
			GetOrCreateSettings();
		}

		internal static SpinePreferences GetOrCreateSettings () {
			var settings = AssetDatabase.LoadAssetAtPath<SpinePreferences>(SPINE_SETTINGS_ASSET_PATH);
			if (settings == null)
			{
				settings = ScriptableObject.CreateInstance<SpinePreferences>();
				SpineEditorUtilities.OldPreferences.CopyOldToNewPreferences(ref settings);
				if (!AssetDatabase.IsValidFolder("Assets/Editor"))
					AssetDatabase.CreateFolder("Assets", "Editor");
				AssetDatabase.CreateAsset(settings, SPINE_SETTINGS_ASSET_PATH);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		internal static SerializedObject GetSerializedSettings () {
			return new SerializedObject(GetOrCreateSettings());
		}

		public static void HandlePreferencesGUI (SerializedObject settings) {
			
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 250;
			
			using (new EditorGUI.IndentLevelScope()) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(settings.FindProperty("showHierarchyIcons"), new GUIContent("Show Hierarchy Icons", "Show relevant icons on GameObjects with Spine Components on them. Disable this if you have large, complex scenes."));
				if (EditorGUI.EndChangeCheck()) {
					#if NEWPLAYMODECALLBACKS
					SpineEditorUtilities.HierarchyHandler.IconsOnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
					#else
					SpineEditorUtilities.HierarchyHandler.IconsOnPlaymodeStateChanged();
					#endif
				}

				EditorGUILayout.PropertyField(settings.FindProperty("autoReloadSceneSkeletons"), new GUIContent("Auto-reload scene components", "Reloads Skeleton components in the scene whenever their SkeletonDataAsset is modified. This makes it so changes in the SkeletonDataAsset inspector are immediately reflected. This may be slow when your scenes have large numbers of SkeletonRenderers or SkeletonGraphic."));

				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultMix"), new GUIContent("Default Mix", "The Default Mix Duration for newly imported SkeletonDataAssets."), min: 0f);
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultScale"), new GUIContent("Default SkeletonData Scale", "The Default skeleton import scale for newly imported SkeletonDataAssets."), min: 0.0000001f);
					
					SpineEditorUtilities.ShaderPropertyField(settings.FindProperty("defaultShader"), new GUIContent("Default Shader"), SpinePreferences.DEFAULT_DEFAULT_SHADER);

					EditorGUILayout.PropertyField(settings.FindProperty("setTextureImporterSettings"), new GUIContent("Apply Atlas Texture Settings", "Apply the recommended settings for Texture Importers."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("atlasTxtImportWarning"), new GUIContent("Atlas Extension Warning", "Log a warning and recommendation whenever a `.atlas` file is found."));
					EditorGUILayout.PropertyField(settings.FindProperty("textureImporterWarning"), new GUIContent("Texture Settings Warning", "Log a warning and recommendation whenever Texture Import Settings are detected that could lead to undesired effects, e.g. white border artifacts."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Editor Instantiation", EditorStyles.boldLabel);
				{
					EditorGUILayout.Slider(settings.FindProperty("defaultZSpacing"), -0.1f, 0f, new GUIContent("Default Slot Z-Spacing"));
					EditorGUILayout.PropertyField(settings.FindProperty("defaultInstantiateLoop"), new GUIContent("Default Loop", "Spawn Spine GameObjects with loop enabled."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Handles and Gizmos", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					SpineHandles.handleScale = EditorGUILayout.Slider("Editor Bone Scale", SpineHandles.handleScale, 0.01f, 2f);
					SpineHandles.handleScale = Mathf.Max(0.01f, SpineHandles.handleScale);
					if (EditorGUI.EndChangeCheck()) {
						EditorPrefs.SetFloat(SpinePreferences.SCENE_ICONS_SCALE_KEY, SpineHandles.handleScale);
						SceneView.RepaintAll();
					}
				}
				
				GUILayout.Space(20);
				EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel("Define TK2D");
					if (GUILayout.Button("Enable", GUILayout.Width(64)))
						SpineEditorUtilities.SpineTK2DEditorUtility.EnableTK2D();
					if (GUILayout.Button("Disable", GUILayout.Width(64)))
						SpineEditorUtilities.SpineTK2DEditorUtility.DisableTK2D();
				}
			}
			EditorGUIUtility.labelWidth = prevLabelWidth;
		}
	#endif // NEW_PREFERENCES_SETTINGS_PROVIDER
	}
}
