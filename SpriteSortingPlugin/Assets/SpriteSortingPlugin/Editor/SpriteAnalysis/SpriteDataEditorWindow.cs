using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SpriteAnalysis.Analyzer;
using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis
{
    public class SpriteDataEditorWindow : EditorWindow
    {
        private const int MinWidthRightContentBar = 200;
        private const float LineSpacing = 1.5f;
        private const float RightAreaOffset = 3f;
        private static readonly Array OutlinePrecisionTypes = Enum.GetValues(typeof(OutlinePrecision));

        private SerializedObject serializedObject;

        [SerializeField] private SpriteData spriteData;
        private Sprite selectedSprite;
        private float selectedSpriteAspectRatio;

        private string searchString = "";
        private bool hasLoadedSpriteDataAsset = true;
        private Color outlineColor = Color.green;

        private List<SpriteDataItem> spriteDataList;
        private ReorderableList reorderableSpriteList;
        private SearchField searchField;
        private Vector2 leftBarScrollPosition;
        private float lastHeight;

        private string assetPath = "Assets/SpriteSortingPlugin/SpriteAlphaData";
        private OutlineAnalysisType outlineAnalysisType = OutlineAnalysisType.All;
        private SpriteDataAnalysisType spriteDataAnalysisType = SpriteDataAnalysisType.Outline;
        private OutlinePrecision outlinePrecision;
        private SpriteDataAnalysisType[] spriteAnalyzerTypes;

        private SpriteDataItem selectedSpriteDataItem;

        private bool isDisplayingSpriteOutline;

        private SpriteDataAnalyzerContext spriteDataAnalyzerContext;
        private SpriteAnalyzeInputData spriteAnalyzeInputData;

        private SpriteOutlineAnalyzer spriteOutlineAnalyzer;
        private float outlineTolerance = 0.5f;
        private Dictionary<string, Vector2[]> originOutlines;
        private SimplifiedOutlineToleranceErrorAppearance simplifiedOutlineToleranceErrorAppearance;

        [MenuItem("Window/Sprite Alpha Analysis %e")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteDataEditorWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Alpha Analysis");
            ResetSpriteList();

            //TODO: remove
            SelectDefaultSpriteAlphaData();

            spriteAnalyzerTypes = (SpriteDataAnalysisType[]) Enum.GetValues(typeof(SpriteDataAnalysisType));
        }

        private void SelectDefaultSpriteAlphaData()
        {
            try
            {
                var guids = AssetDatabase.FindAssets("DefaultSpriteAlphaData");
                spriteData =
                    AssetDatabase.LoadAssetAtPath<SpriteData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            catch (Exception e)
            {
                Debug.Log("auto selection of SpriteAlphaData went wrong");
            }

            LoadSpriteDataList();
        }

        private void ResetSpriteList()
        {
            if (spriteDataList == null)
            {
                spriteDataList = new List<SpriteDataItem>();
            }
            else
            {
                spriteDataList.Clear();
            }
        }

        private void OnEnable()
        {
            // if (serializedObject == null)
            // {
            //     serializedObject = new SerializedObject(this);
            // }

            if (searchField == null)
            {
                searchField = new SearchField();
            }

            if (hasLoadedSpriteDataAsset)
            {
                if (reorderableSpriteList == null)
                {
                    InitReordableSpriteList();
                }
            }
        }

        private void OnGUI()
        {
            // serializedObject.Update();
            DrawToolbar();
            // serializedObject.ApplyModifiedProperties();

            if (!hasLoadedSpriteDataAsset)
            {
                return;
            }

            var leftBarWidth = position.width / 4;
            if (leftBarWidth < MinWidthRightContentBar)
            {
                leftBarWidth = MinWidthRightContentBar;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastHeight = GUILayoutUtility.GetLastRect().height;
            }

            DrawLeftContentBar(leftBarWidth);

            var rightAreaRect = new Rect(leftBarWidth + RightAreaOffset, lastHeight + RightAreaOffset,
                position.width - leftBarWidth - RightAreaOffset, position.height - lastHeight - RightAreaOffset);

            GUILayout.BeginArea(rightAreaRect, new GUIStyle {normal = {background = Texture2D.whiteTexture}});

            if (isDisplayingSpriteOutline)
            {
                if (selectedSpriteDataItem == null)
                {
                    GUILayout.EndArea();
                    return;
                }

                DrawOutlineContent(rightAreaRect);

                if (outlinePrecision == OutlinePrecision.PixelPerfect &&
                    !originOutlines.ContainsKey(selectedSpriteDataItem.AssetGuid))
                {
                    originOutlines.Add(selectedSpriteDataItem.AssetGuid, selectedSpriteDataItem.outlinePoints);
                }
            }
            else
            {
                DrawSpriteDetails(rightAreaRect);
            }

            GUILayout.EndArea();
        }

        private void DrawLeftContentBar(float leftBarWidth)
        {
            var leftAreaRect = new Rect(0, lastHeight, leftBarWidth, position.height);
            using (new GUILayout.AreaScope(leftAreaRect))
            {
                using (var scrollScope = new EditorGUILayout.ScrollViewScope(leftBarScrollPosition))
                {
                    leftBarScrollPosition = scrollScope.scrollPosition;
                    using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                    {
                        GUILayout.Label(new GUIContent("Edit Sprite Data", UITooltipConstants.SpriteDataEditModeTooltip),
                            Styling.CenteredStyle, GUILayout.ExpandWidth(true));

                        using (new EditorGUILayout.HorizontalScope(Styling.HelpBoxStyle))
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.Toggle(isDisplayingSpriteOutline, "Sprite outline", Styling.ButtonStyle);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isDisplayingSpriteOutline = true;
                            }

                            EditorGUI.BeginChangeCheck();
                            GUILayout.Toggle(!isDisplayingSpriteOutline, "Sprite Details", Styling.ButtonStyle);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isDisplayingSpriteOutline = false;
                            }
                        }

                        if (isDisplayingSpriteOutline)
                        {
                            EditorGUILayout.Space();

                            GUILayout.Label(new GUIContent("Preview Outline",
                                UITooltipConstants.SpriteDataPreviewOutlineTooltip));
                            foreach (OutlinePrecision outlinePrecisionType in OutlinePrecisionTypes)
                            {
                                EditorGUI.BeginChangeCheck();

                                GUILayout.Toggle(outlinePrecision == outlinePrecisionType,
                                    ObjectNames.NicifyVariableName(outlinePrecisionType.ToString()),
                                    Styling.ButtonStyle);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    outlinePrecision = outlinePrecisionType;
                                }
                            }

                            GUILayout.Label(new GUIContent("Outline Color", UITooltipConstants.SpriteDataOutlineColorTooltip));
                            outlineColor = EditorGUILayout.ColorField(outlineColor);
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.Label(
                        new GUIContent("Sprites", UITooltipConstants.SpriteDataSpriteListTooltip),
                        Styling.CenteredStyle, GUILayout.ExpandWidth(true));
                    EditorGUI.BeginChangeCheck();
                    searchString = searchField.OnGUI(searchString);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (spriteData != null && spriteData.spriteDataDictionary.Count > 0)
                        {
                            FilterSpriteDataList();
                        }
                    }

                    if (reorderableSpriteList == null)
                    {
                        InitReordableSpriteList();
                    }

                    reorderableSpriteList.DoLayoutList();
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    EditorGUIUtility.labelWidth = 110;
                    spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset"), spriteData,
                        typeof(SpriteData), false, GUILayout.MinWidth(290)) as SpriteData;
                    EditorGUIUtility.labelWidth = 0;

                    if (GUILayout.Button("Load"))
                    {
                        LoadSpriteDataList();
                    }
                }

                GUILayout.FlexibleSpace();

                //TODO add allowReimport function, add to current spriteData and analyze a given sprite only 
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUIUtility.labelWidth = 80;
                        EditorGUI.BeginChangeCheck();
                        spriteDataAnalysisType = (SpriteDataAnalysisType) EditorGUILayout.EnumFlagsField(
                            new GUIContent("Analysis Type", UITooltipConstants.SpriteDataAnalysisTypeTooltip),
                            spriteDataAnalysisType, GUILayout.MinWidth(200));
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (!spriteDataAnalysisType.HasFlag(SpriteDataAnalysisType.Outline))
                            {
                                spriteDataAnalysisType |= SpriteDataAnalysisType.Outline;
                            }
                        }

                        outlineAnalysisType = (OutlineAnalysisType) EditorGUILayout.EnumFlagsField(
                            new GUIContent("Outline Type", UITooltipConstants.SpriteDataOutlineAnalysisTypeTooltip),
                            outlineAnalysisType, GUILayout.MinWidth(200));

                        EditorGUIUtility.labelWidth = 0;
                    }

                    if (GUILayout.Button(new GUIContent("Analyze all sprites + create new SpriteData",
                        UITooltipConstants.SpriteDataAnalyzeAllTooltip)))
                    {
                        AnalyzeSpriteAlphas();
                    }
                }
            }
        }

        private void DrawSpriteDetails(Rect rightAreaRect)
        {
            var hasSelectedDataItem = selectedSpriteDataItem != null;

            using (new EditorGUILayout.VerticalScope(GUILayout.Height(210)))
            {
                EditorGUILayout.Space();
                EditorGUI.DrawTextureTransparent(new Rect(rightAreaRect.width / 2 - 75, rightAreaRect.y, 150, 150),
                    selectedSprite != null ? selectedSprite.texture : Texture2D.grayTexture, ScaleMode.ScaleToFit);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    var resolutionLabel = EditorGUILayout.GetControlRect();
                    var resolutionContentLabel = EditorGUI.PrefixLabel(resolutionLabel,
                        new GUIContent("Resolution", UITooltipConstants.SpriteDataResolutionTooltip));
                    var spriteSize = hasSelectedDataItem
                        ? new int[] {selectedSprite.texture.width, selectedSprite.texture.height}
                        : new int[] {0, 0};
                    EditorGUI.MultiIntField(resolutionContentLabel,
                        new GUIContent[] {new GUIContent("Width"), new GUIContent("height")}, spriteSize);
                }
            }

            GUILayout.Space(1.5f);

            var analyzeButtonWidth = GUILayout.Width(rightAreaRect.width / 8);

            using (new EditorGUI.DisabledGroupScope(!hasSelectedDataItem))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var sharpness = hasSelectedDataItem ? selectedSpriteDataItem.spriteAnalysisData.sharpness : 0;
                    sharpness = EditorGUILayout.DoubleField(new GUIContent("Sharpness", UITooltipConstants.SpriteDataSharpnessTooltip),
                        sharpness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedSpriteDataItem.spriteAnalysisData.sharpness = Math.Max(0, sharpness);
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                        AnalyzeSprite(SpriteAnalyzerType.Sharpness);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var perceivedLightness = hasSelectedDataItem
                        ? selectedSpriteDataItem.spriteAnalysisData.perceivedLightness
                        : 0;
                    perceivedLightness = EditorGUILayout.Slider(
                        new GUIContent("Perceived Lightness", UITooltipConstants.SpriteDataPerceivedLightnessTooltip),
                        perceivedLightness, 0, 100);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedSpriteDataItem.spriteAnalysisData.perceivedLightness = perceivedLightness;
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                        AnalyzeSprite(SpriteAnalyzerType.Lightness);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var averageAlpha = hasSelectedDataItem
                        ? selectedSpriteDataItem.spriteAnalysisData.averageAlpha
                        : 0;
                    averageAlpha = EditorGUILayout.Slider(
                        new GUIContent("Average alpha", UITooltipConstants.SpriteDataAverageAlphaTooltip),
                        averageAlpha, 0, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedSpriteDataItem.spriteAnalysisData.averageAlpha = averageAlpha;
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                        AnalyzeSprite(SpriteAnalyzerType.AverageAlpha);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var primaryColor = hasSelectedDataItem
                        ? selectedSpriteDataItem.spriteAnalysisData.primaryColor
                        : Color.black;
                    primaryColor = EditorGUILayout.ColorField(
                        new GUIContent("Primary Color", UITooltipConstants.SpriteDataPrimaryColorTooltip), primaryColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedSpriteDataItem.spriteAnalysisData.primaryColor = primaryColor;
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                        AnalyzeSprite(SpriteAnalyzerType.PrimaryColor);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("", "");
                    if (GUILayout.Button("Analyze All", analyzeButtonWidth))
                    {
                        spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                        AnalyzeSprite(SpriteAnalyzerType.Lightness, SpriteAnalyzerType.Sharpness,
                            SpriteAnalyzerType.PrimaryColor, SpriteAnalyzerType.AverageAlpha);
                    }
                }
            }
        }

        private void AnalyzeSprite(params SpriteAnalyzerType[] currentSpriteAnalyzerTypes)
        {
            if (spriteDataAnalyzerContext == null)
            {
                spriteDataAnalyzerContext = new SpriteDataAnalyzerContext();
            }

            spriteDataAnalyzerContext.ClearSpriteDataAnalyzers();
            foreach (var spriteAnalyzerType in currentSpriteAnalyzerTypes)
            {
                spriteDataAnalyzerContext.AddSpriteDataAnalyzer(spriteAnalyzerType);
            }

            spriteDataAnalyzerContext.Analyze(ref spriteData, spriteAnalyzeInputData);
        }

        private void DrawOutlineContent(Rect rightAreaRect)
        {
            var textureRect = new Rect(0, 0, rightAreaRect.width, rightAreaRect.height);
            EditorGUI.DrawTextureTransparent(textureRect, selectedSprite.texture, ScaleMode.ScaleToFit);
            var rectWithAlphaBorders = CalculateRectWithAlphaBorders(textureRect);
            Handles.color = outlineColor;
            switch (outlinePrecision)
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    DrawAABB(rectWithAlphaBorders);
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    DrawOOBB(rectWithAlphaBorders, rightAreaRect);
                    break;
                case OutlinePrecision.PixelPerfect:
                    DrawOutline(rectWithAlphaBorders, rightAreaRect);
                    break;
            }
        }

        private void DrawAABB(Rect rectWithAlphaBorders)
        {
            Handles.DrawWireCube(rectWithAlphaBorders.center, rectWithAlphaBorders.size);
            Handles.DrawWireCube(rectWithAlphaBorders.center,
                new Vector3(rectWithAlphaBorders.size.x - 1, rectWithAlphaBorders.size.y - 1));
        }

        private void DrawOutline(Rect rectWithAlphaBorders, Rect rightAreaRect)
        {
            if (!selectedSpriteDataItem.IsValidOutline())
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;
            var scale = new Vector2(scaleYFactor, -scaleXFactor);

            var lastPoint = selectedSpriteDataItem.outlinePoints[0] * scale + rectWithAlphaBorders.center;
            for (var i = 1; i < selectedSpriteDataItem.outlinePoints.Length; i++)
            {
                var nextPoint = selectedSpriteDataItem.outlinePoints[i] * scale + rectWithAlphaBorders.center;
                Handles.DrawLine(lastPoint, nextPoint);
                Handles.DrawLine(new Vector2(lastPoint.x + 1, lastPoint.y), new Vector2(nextPoint.x + 1, nextPoint.y));
                lastPoint = nextPoint;
            }

            using (new GUILayout.AreaScope(new Rect(0, position.height - 122, rightAreaRect.width, 75)))
            {
                var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 75);
                EditorGUI.DrawRect(alphaRectangleBorderRect, Styling.TransparentBackgroundColor);

                DrawOutlineBorderSettings();
            }
        }

        private void DrawOutlineBorderSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Outline Options");
                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Re-Analyze", GUILayout.Width(90)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "reanalyzed outline");
                    spriteAnalyzeInputData.assetGuid = selectedSpriteDataItem.AssetGuid;
                    spriteAnalyzeInputData.outlineAnalysisType = OutlineAnalysisType.PixelPerfect;
                    AnalyzeSprite(SpriteAnalyzerType.Outline);

                    simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
                }

                if (GUILayout.Button("Reset (last loaded Outline)", GUILayout.Width(160)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "Reset to last loaded outline");
                    var isContainingOutline =
                        originOutlines.TryGetValue(selectedSpriteDataItem.AssetGuid, out var originOutline);
                    if (isContainingOutline)
                    {
                        selectedSpriteDataItem.outlinePoints = originOutline;
                        simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                outlineTolerance = EditorGUILayout.FloatField("Outline Tolerance", outlineTolerance);
                if (EditorGUI.EndChangeCheck())
                {
                    if (outlineTolerance < 0)
                    {
                        outlineTolerance = 0;
                    }
                }

                if (GUILayout.Button("Simplify", GUILayout.Width(253)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "simplified outline");
                    SimplifyOutline();
                }
            }

            if (simplifiedOutlineToleranceErrorAppearance != SimplifiedOutlineToleranceErrorAppearance.Nothing)
            {
                EditorGUI.indentLevel++;
                var outlineErrorContent = new GUIContent();

                switch (simplifiedOutlineToleranceErrorAppearance)
                {
                    case SimplifiedOutlineToleranceErrorAppearance.NotValid:
                        outlineErrorContent.image = Styling.WarnIcon;
                        outlineErrorContent.text =
                            "Simplified Outline was not valid, please lower the outline tolerance";
                        break;
                    case SimplifiedOutlineToleranceErrorAppearance.ReplacedByOOBB:
                        outlineErrorContent.text =
                            "Simplified Outline was not valid and replaced by current OOBB. Please lower the outline tolerance next time.";
                        break;
                }

                GUILayout.Label(outlineErrorContent);
                EditorGUI.indentLevel--;
            }

            GUILayout.FlexibleSpace();
        }

        private void SimplifyOutline()
        {
            if (spriteOutlineAnalyzer == null)
            {
                spriteOutlineAnalyzer = new SpriteOutlineAnalyzer();
            }

            var isSimplified = spriteOutlineAnalyzer.Simplify(selectedSpriteDataItem.outlinePoints,
                outlineTolerance, out var simplifiedPoints);

            if (isSimplified)
            {
                selectedSpriteDataItem.outlinePoints = simplifiedPoints;
            }
            else if (selectedSpriteDataItem.IsValidOOBB())
            {
                var colliderPoints = new Vector2[5];
                var oobbLocalWorldPoints = selectedSpriteDataItem.objectOrientedBoundingBox.LocalWorldPoints;
                colliderPoints[4] = new Vector2(oobbLocalWorldPoints[0].x, oobbLocalWorldPoints[0].y);

                for (var i = 0; i < oobbLocalWorldPoints.Length; i++)
                {
                    var point = oobbLocalWorldPoints[i];
                    colliderPoints[i] = new Vector2(point.x, point.y);
                }

                selectedSpriteDataItem.outlinePoints = colliderPoints;
            }
            else
            {
                simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.NotValid;
            }
        }

        private void DrawOOBB(Rect rectWithAlphaBorders, Rect rightAreaRect)
        {
            if (!selectedSpriteDataItem.IsValidOOBB())
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;

            var newBoundsWidth = scaleXFactor *
                                 (selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteWidth -
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.leftBorder -
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.rightBorder) /
                                 selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.pixelPerUnit;

            var newBoundsHeight = scaleYFactor *
                                  (selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteHeight -
                                   selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.topBorder -
                                   selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.bottomBorder) /
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.pixelPerUnit;

            var scaledSize = new Vector2(newBoundsWidth, newBoundsHeight);

            var rectCenter = rectWithAlphaBorders.center - new Vector2(
                selectedSpriteDataItem.objectOrientedBoundingBox.BoundsCenterOffset.x * scaleXFactor,
                selectedSpriteDataItem.objectOrientedBoundingBox.BoundsCenterOffset.y * scaleYFactor);

            Handles.DrawWireCube(rectCenter, scaledSize);
            Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x + 1, scaledSize.y + 1));
            Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x - 1, scaledSize.y - 1));

            using (new GUILayout.AreaScope(new Rect(0, position.height - 147, rightAreaRect.width, 100)))
            {
                var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 125);
                EditorGUI.DrawRect(alphaRectangleBorderRect, Styling.TransparentBackgroundColor);

                EditorGUI.BeginChangeCheck();

                DrawAlphaRectangleBorderSettings(alphaRectangleBorderRect);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "changed OOBB size");
                    selectedSpriteDataItem.objectOrientedBoundingBox.UpdateBoxSizeWithBorder();
                }
            }
        }

        private Rect CalculateRectWithAlphaBorders(Rect textureRect)
        {
            var rectAspectRatio = textureRect.width / textureRect.height;

            Rect displayingTextureRect;

            // set rect and sprite in ratio and adjust bounding box according to the ScalingMode ScaleToFit, from UnityEngine.GUI.CalculateScaledTextureRects
            //https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Modules/IMGUI/GUI.cs#L262
            if (rectAspectRatio > selectedSpriteAspectRatio)
            {
                //rectangle is longer than sprite's width
                var num2 = selectedSpriteAspectRatio / rectAspectRatio;

                displayingTextureRect =
                    new Rect(textureRect.xMin + (float) ((double) textureRect.width * (1.0 - (double) num2) * 0.5),
                        textureRect.yMin, num2 * textureRect.width, textureRect.height);
            }
            else
            {
                var num3 = rectAspectRatio / selectedSpriteAspectRatio;

                displayingTextureRect = new Rect(textureRect.xMin,
                    textureRect.yMin + (float) ((double) textureRect.height * (1.0 - (double) num3) * 0.5),
                    textureRect.width, num3 * textureRect.height);
            }

            return displayingTextureRect;
        }

        private void DrawAlphaRectangleBorderSettings(Rect rect)
        {
            var intFieldLength = rect.width / 3f;
            var halfSpriteWidth = selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteWidth / 2;

            var halfSpriteHeight =
                selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteHeight / 2;

            EditorGUI.LabelField(new Rect(rect.width / 2 - 45, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Adjust Borders");
            if (GUI.Button(new Rect(rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                selectedSpriteDataItem.objectOrientedBoundingBox.ResetAlphaRectangleBorder();
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;
            var alphaBorder = selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder;
            alphaBorder.topBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.topBorder, 0, halfSpriteHeight);
            rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            {
                alphaBorder.leftBorder = EditorGUI.IntSlider(
                    new Rect(rect.x, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                    alphaBorder.leftBorder, 0, halfSpriteWidth);

                EditorGUI.LabelField(new Rect(rect.width / 2 - Styling.MoveIcon.width,
                    rect.y - (Styling.MoveIcon.height / 2f) + EditorGUIUtility.singleLineHeight / 2f,
                    Styling.MoveIcon.width,
                    EditorGUIUtility.singleLineHeight * 2), new GUIContent(Styling.MoveIcon));

                alphaBorder.rightBorder = EditorGUI.IntSlider(
                    new Rect(rect.x + 2 * intFieldLength, rect.y, intFieldLength,
                        EditorGUIUtility.singleLineHeight),
                    alphaBorder.rightBorder, 0, halfSpriteWidth);

                rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;
            }
            alphaBorder.bottomBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.bottomBorder, 0, halfSpriteHeight);
            selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder = alphaBorder;
        }

        private void FilterSpriteDataList()
        {
            searchString = searchString.Trim();
            spriteDataList.Clear();
            foreach (var spriteDataItem in spriteData.spriteDataDictionary.Values)
            {
                if (searchString.Length == 0 || spriteDataItem.AssetName.Contains(searchString))
                {
                    spriteDataList.Add(spriteDataItem);
                }
            }
        }

        private void LoadSpriteDataList()
        {
            if (spriteData == null)
            {
                return;
            }

            hasLoadedSpriteDataAsset = true;
            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
            FilterSpriteDataList();

            if (originOutlines == null)
            {
                originOutlines = new Dictionary<string, Vector2[]>();
            }

            originOutlines.Clear();
        }

        private void AnalyzeSpriteAlphas()
        {
            if (outlineAnalysisType == OutlineAnalysisType.Nothing &&
                spriteDataAnalysisType == SpriteDataAnalysisType.Nothing)
            {
                return;
            }

            reorderableSpriteList.index = -1;
            selectedSpriteDataItem = null;
            spriteDataList.Clear();
            spriteData = CreateInstance<SpriteData>();
            spriteAnalyzeInputData.assetGuid = null;
            spriteAnalyzeInputData.outlineAnalysisType = outlineAnalysisType;
            GenerateSpriteDataItems();
            var currentSpriteAnalyzerTypes = CreateSpriteAnalyzerTypeArray();
            AnalyzeSprite(currentSpriteAnalyzerTypes);
            LoadSpriteDataList();

            var assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + nameof(SpriteData) + ".asset");

            AssetDatabase.CreateAsset(spriteData, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
        }

        private SpriteAnalyzerType[] CreateSpriteAnalyzerTypeArray()
        {
            var typeList = new List<SpriteAnalyzerType>();
            if (spriteDataAnalysisType == SpriteDataAnalysisType.Nothing)
            {
                return typeList.ToArray();
            }

            foreach (var spriteAnalyzerType in spriteAnalyzerTypes)
            {
                if (spriteAnalyzerType == SpriteDataAnalysisType.Nothing ||
                    spriteAnalyzerType == SpriteDataAnalysisType.All ||
                    !spriteDataAnalysisType.HasFlag(spriteAnalyzerType))
                {
                    continue;
                }

                var type = SpriteAnalyzerType.Outline;

                switch (spriteAnalyzerType)
                {
                    case SpriteDataAnalysisType.Sharpness:
                        type = SpriteAnalyzerType.Sharpness;
                        break;
                    case SpriteDataAnalysisType.Lightness:
                        type = SpriteAnalyzerType.Lightness;
                        break;
                    case SpriteDataAnalysisType.PrimaryColor:
                        type = SpriteAnalyzerType.PrimaryColor;
                        break;
                    case SpriteDataAnalysisType.AverageAlpha:
                        type = SpriteAnalyzerType.AverageAlpha;
                        break;
                }

                typeList.Add(type);
            }

            return typeList.ToArray();
        }

        private void GenerateSpriteDataItems()
        {
            var spriteRenderers = FindObjectsOfType<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (!spriteRenderer.enabled || !spriteRenderer.gameObject.activeInHierarchy ||
                    spriteRenderer.sprite == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID());
                var guid = AssetDatabase.AssetPathToGUID(path);

                if (spriteData.spriteDataDictionary.ContainsKey(guid))
                {
                    continue;
                }

                var spriteDataItem = new SpriteDataItem(guid, spriteRenderer.sprite.name);
                spriteData.spriteDataDictionary.Add(guid, spriteDataItem);
            }
        }

        private void InitReordableSpriteList()
        {
            reorderableSpriteList = new ReorderableList(spriteDataList, typeof(string), false, false, false, false);
            reorderableSpriteList.onSelectCallback += OnSpriteSelected;
            reorderableSpriteList.drawElementCallback += DrawElementCallBack;
            reorderableSpriteList.drawElementBackgroundCallback += DrawElementBackgroundCallBack;
        }

        private void DrawElementBackgroundCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = Styling.ListElementActiveColor;
            }

            else if (isFocused)
            {
                color = Styling.ListElementFocussingColor;
            }

            else
            {
                color = index % 2 == 0
                    ? Styling.ListElementBackground1
                    : Styling.ListElementBackground2;
            }

            EditorGUI.DrawRect(rect, color);
        }

        private void DrawElementCallBack(Rect rect, int index, bool isactive, bool isfocused)
        {
            var oobb = spriteDataList[index];
            EditorGUI.LabelField(rect, oobb.AssetName);
        }

        private void OnSpriteSelected(ReorderableList list)
        {
            if (list.index < 0)
            {
                return;
            }

            selectedSpriteDataItem = spriteDataList[list.index];
            var path = AssetDatabase.GUIDToAssetPath(selectedSpriteDataItem.AssetGuid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            selectedSprite = sprite;
            selectedSpriteAspectRatio = (float) selectedSprite.texture.width / (float) selectedSprite.texture.height;

            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
        }

        private void OnDestroy()
        {
            reorderableSpriteList.onSelectCallback = null;
            reorderableSpriteList.drawElementCallback = null;
            reorderableSpriteList.drawElementBackgroundCallback = null;
            reorderableSpriteList = null;
        }
    }

    internal enum SimplifiedOutlineToleranceErrorAppearance
    {
        Nothing,
        ReplacedByOOBB,
        NotValid
    }
}