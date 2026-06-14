#!/usr/bin/env python3
"""Convert MainMenu hybrid SpriteRenderer objects to UI Image."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "Assets/_Sources/Scenes/MainMenu/MainMenu.unity"

IMAGE_TEMPLATE = """--- !u!222 &{cr_id}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_CullTransparentMesh: 1
--- !u!114 &{img_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: {raycast}
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {sprite}
  m_Type: 0
  m_PreserveAspect: {preserve}
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
"""


def remove_block(text: str, marker: str, next_marker: str) -> str:
    start = text.index(marker)
    end = text.index(next_marker, start)
    return text[:start] + text[end:]


def main() -> None:
    text = SCENE.read_text(encoding="utf-8")

    # --- BackgroundImage ---
    text = text.replace(
        "  - component: {fileID: 1291019167}\n  - component: {fileID: 1291019166}\n",
        "  - component: {fileID: 1291019167}\n  - component: {fileID: 1291019168}\n  - component: {fileID: 1291019169}\n",
    )
    text = text.replace("  m_Layer: 0\n  m_Name: BackgroundImage", "  m_Layer: 5\n  m_Name: BackgroundImage")
    text = remove_block(text, "--- !u!212 &1291019166\n", "--- !u!224 &1291019167\n")
    bg_image = IMAGE_TEMPLATE.format(
        cr_id=1291019168,
        img_id=1291019169,
        go_id=1291019165,
        raycast=0,
        preserve=0,
        sprite="{fileID: -1681831283906654698, guid: ae264696a9a25ae44b5bcfa1b684f40b, type: 3}",
    )
    text = text.replace(
        "--- !u!224 &1291019167\n",
        bg_image + "--- !u!224 &1291019167\n",
    )

    # --- logo_0 ---
    text = text.replace(
        "  - component: {fileID: 1792649488}\n  - component: {fileID: 1792649487}\n",
        "  - component: {fileID: 1792649488}\n  - component: {fileID: 1792649490}\n  - component: {fileID: 1792649491}\n",
    )
    text = remove_block(text, "--- !u!212 &1792649487\n", "--- !u!4 &1792649488\n")
    logo_image = IMAGE_TEMPLATE.format(
        cr_id=1792649490,
        img_id=1792649491,
        go_id=1792649486,
        raycast=0,
        preserve=1,
        sprite="{fileID: -4320295261928297051, guid: 3cd49006d52cc214ebae54b0c07ac337, type: 3}",
    )
    text = text.replace(
        "--- !u!4 &1792649488\nTransform:",
        logo_image + "--- !u!224 &1792649488\nRectTransform:",
    )
    text = text.replace(
        """  m_GameObject: {fileID: 1792649486}
  serializedVersion: 2
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: -415, y: 295, z: 0}
  m_LocalScale: {x: 46.39938, y: 46.39938, z: 46.39938}
  m_ConstrainProportionsScale: 1
  m_Children: []
  m_Father: {fileID: 1609835251}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}""",
        """  m_GameObject: {fileID: 1792649486}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 1609835251}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 1}
  m_AnchorMax: {x: 0, y: 1}
  m_AnchoredPosition: {x: 140, y: -155}
  m_SizeDelta: {x: 952, y: 307}
  m_Pivot: {x: 0, y: 1}""",
    )

    # --- folinha_0 ---
    text = text.replace(
        "  - component: {fileID: 1977694724}\n  - component: {fileID: 1977694723}\n  - component: {fileID: 1977694725}\n",
        "  - component: {fileID: 1977694724}\n  - component: {fileID: 1977694726}\n  - component: {fileID: 1977694727}\n  - component: {fileID: 1977694725}\n",
    )
    text = text.replace("  m_Layer: 0\n  m_Name: folinha_0", "  m_Layer: 5\n  m_Name: folinha_0")
    text = remove_block(text, "--- !u!212 &1977694723\n", "--- !u!4 &1977694724\n")
    folha_image = IMAGE_TEMPLATE.format(
        cr_id=1977694726,
        img_id=1977694727,
        go_id=1977694722,
        raycast=0,
        preserve=1,
        sprite="{fileID: 5647309494413895500, guid: 4e3c467fda5559141b66f60093ad65e1, type: 3}",
    )
    text = text.replace(
        "--- !u!4 &1977694724\nTransform:",
        folha_image + "--- !u!224 &1977694724\nRectTransform:",
    )
    text = text.replace(
        """  m_GameObject: {fileID: 1977694722}
  serializedVersion: 2
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: -377, y: -616, z: 0}
  m_LocalScale: {x: 56.395393, y: 56.395393, z: 56.395393}
  m_ConstrainProportionsScale: 1
  m_Children: []
  m_Father: {fileID: 1558379657}
  m_LocalEulerAnglesHint: {x: 0, y: -0.022077858, z: -424.08}""",
        """  m_GameObject: {fileID: 1977694722}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 1558379657}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 0, y: 0}
  m_AnchoredPosition: {x: 95, y: 95}
  m_SizeDelta: {x: 89, y: 89}
  m_Pivot: {x: 0.5, y: 0.5}""",
    )

    # --- adesivo_0 (StartMenu child) ---
    text = text.replace(
        "  - component: {fileID: 369018801}\n  - component: {fileID: 369018800}\n",
        "  - component: {fileID: 369018801}\n  - component: {fileID: 369018802}\n  - component: {fileID: 369018803}\n",
    )
    text = text.replace("  m_Layer: 6\n  m_Name: adesivo_0", "  m_Layer: 5\n  m_Name: adesivo_0")
    text = remove_block(text, "--- !u!212 &369018800\n", "--- !u!4 &369018801\n")
    adesivo_image = IMAGE_TEMPLATE.format(
        cr_id=369018802,
        img_id=369018803,
        go_id=369018799,
        raycast=0,
        preserve=1,
        sprite="{fileID: 4214622727716992106, guid: 028dae8ccc5267749b0e3dda468745da, type: 3}",
    )
    text = text.replace(
        "--- !u!4 &369018801\nTransform:",
        adesivo_image + "--- !u!224 &369018801\nRectTransform:",
    )
    text = text.replace(
        """  m_GameObject: {fileID: 369018799}
  serializedVersion: 2
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: 489, y: -75.99998, z: 0}
  m_LocalScale: {x: 116.1228, y: 116.1228, z: 116.1228}
  m_ConstrainProportionsScale: 1
  m_Children: []
  m_Father: {fileID: 1861549338}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}""",
        """  m_GameObject: {fileID: 369018799}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 1861549338}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 1, y: 0.5}
  m_AnchorMax: {x: 1, y: 0.5}
  m_AnchoredPosition: {x: -40, y: -20}
  m_SizeDelta: {x: 146, y: 146}
  m_Pivot: {x: 0.5, y: 0.5}""",
    )

    # --- adesivo2_0 (OptionsMenu child) ---
    text = text.replace(
        "  - component: {fileID: 2026613091}\n  - component: {fileID: 2026613090}\n",
        "  - component: {fileID: 2026613091}\n  - component: {fileID: 2026613092}\n  - component: {fileID: 2026613093}\n",
    )
    text = text.replace("  m_Layer: 6\n  m_Name: adesivo2_0", "  m_Layer: 5\n  m_Name: adesivo2_0")
    text = remove_block(text, "--- !u!212 &2026613090\n", "--- !u!4 &2026613091\n")
    adesivo2_image = IMAGE_TEMPLATE.format(
        cr_id=2026613092,
        img_id=2026613093,
        go_id=2026613089,
        raycast=0,
        preserve=1,
        sprite="{fileID: 63777309648598959, guid: 57241a8c8d19bf244a6e44a7222182f6, type: 3}",
    )
    text = text.replace(
        "--- !u!4 &2026613091\nTransform:",
        adesivo2_image + "--- !u!224 &2026613091\nRectTransform:",
    )
    text = text.replace(
        """  m_GameObject: {fileID: 2026613089}
  serializedVersion: 2
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: -470.2863, y: -47.454556, z: 0}
  m_LocalScale: {x: 120.59998, y: 120.59998, z: 120.59998}
  m_ConstrainProportionsScale: 1
  m_Children: []
  m_Father: {fileID: 2039679631}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}""",
        """  m_GameObject: {fileID: 2026613089}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 2039679631}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0.5}
  m_AnchorMax: {x: 0, y: 0.5}
  m_AnchoredPosition: {x: 35, y: -15}
  m_SizeDelta: {x: 152, y: 152}
  m_Pivot: {x: 0.5, y: 0.5}""",
    )

    # --- StartMenu: SpriteRenderer -> Image ---
    text = text.replace(
        "  - component: {fileID: 1861549343}\n",
        "  - component: {fileID: 1861549346}\n",
        1,
    )
    start_image = IMAGE_TEMPLATE.format(
        cr_id=1861549341,
        img_id=1861549346,
        go_id=1861549337,
        raycast=1,
        preserve=1,
        sprite="{fileID: -8400806309250327626, guid: 5e75f21e2865fb34e8cad745ddd22b0c, type: 3}",
    )
    start_image_only = start_image.split("--- !u!222 &1861549341\n", 1)[1]
    text = remove_block(text, "--- !u!212 &1861549343\n", "--- !u!82 &1861549344\n")
    text = text.replace(
        "--- !u!82 &1861549344\n",
        start_image_only + "--- !u!82 &1861549344\n",
    )

    text = text.replace(
        "  m_LocalScale: {x: 0.55, y: 0.55, z: 0.55}\n  m_ConstrainProportionsScale: 1\n  m_Children:\n  - {fileID: 1312868219}\n  - {fileID: 369018801}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 294, y: -109.2}\n  m_SizeDelta: {x: 1186, y: 312}",
        "  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children:\n  - {fileID: 1312868219}\n  - {fileID: 369018801}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 294, y: -109.2}\n  m_SizeDelta: {x: 652, y: 172}",
    )
    text = text.replace("  m_TargetGraphic: {fileID: 0}\n  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n      - m_Target: {fileID: 1609835252}\n        m_TargetAssemblyTypeName: StartMenuController, Assembly-CSharp\n        m_MethodName: OnStartClick",
        "  m_TargetGraphic: {fileID: 1861549346}\n  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n      - m_Target: {fileID: 1609835252}\n        m_TargetAssemblyTypeName: StartMenuController, Assembly-CSharp\n        m_MethodName: OnStartClick",
    )

    # --- OptionsMenu ---
    text = text.replace(
        "  - component: {fileID: 2039679636}\n",
        "  - component: {fileID: 2039679642}\n",
        1,
    )
    options_image = IMAGE_TEMPLATE.format(
        cr_id=2039679634,
        img_id=2039679642,
        go_id=2039679630,
        raycast=1,
        preserve=1,
        sprite="{fileID: 3479481073323317570, guid: bac94ec783640544ab123cf963b540d7, type: 3}",
    )
    options_image_only = options_image.split("--- !u!222 &2039679634\n", 1)[1]
    text = remove_block(text, "--- !u!212 &2039679636\n", "--- !u!95 &2039679637\n")
    text = text.replace(
        "--- !u!95 &2039679637\n",
        options_image_only + "--- !u!95 &2039679637\n",
    )

    text = text.replace(
        "  m_LocalScale: {x: 0.55, y: 0.55, z: 0.55}\n  m_ConstrainProportionsScale: 1\n  m_Children:\n  - {fileID: 1508675366}\n  - {fileID: 2026613091}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 296.5, y: -307}\n  m_SizeDelta: {x: 1186, y: 211.7}",
        "  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children:\n  - {fileID: 1508675366}\n  - {fileID: 2026613091}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 296.5, y: -307}\n  m_SizeDelta: {x: 652, y: 116}",
    )
    text = text.replace(
        "  m_TargetGraphic: {fileID: 0}\n  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n      - m_Target: {fileID: 1609835252}\n        m_TargetAssemblyTypeName: StartMenuController, Assembly-CSharp\n        m_MethodName: onOptionsClick",
        "  m_TargetGraphic: {fileID: 2039679642}\n  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n      - m_Target: {fileID: 1609835252}\n        m_TargetAssemblyTypeName: StartMenuController, Assembly-CSharp\n        m_MethodName: onOptionsClick",
    )

    # --- ExitMenu ---
    text = text.replace(
        "  m_LocalScale: {x: 5.96, y: 5.96, z: 5.96}\n  m_ConstrainProportionsScale: 1\n  m_Children:\n  - {fileID: 1813159773}\n  - {fileID: 140859249}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 307.9532, y: -516}\n  m_SizeDelta: {x: 103.34, y: 27.4}",
        "  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children:\n  - {fileID: 1813159773}\n  - {fileID: 140859249}\n  m_Father: {fileID: 1036004884}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 1}\n  m_AnchorMax: {x: 0, y: 1}\n  m_AnchoredPosition: {x: 308, y: -516}\n  m_SizeDelta: {x: 616, y: 163}",
    )

    SCENE.write_text(text, encoding="utf-8")
    print(f"Updated {SCENE}")

    # Animator clip scale fixes
    start_ctrl = ROOT / "Assets/Resources/Controllers/StartMenu.controller"
    s = start_ctrl.read_text(encoding="utf-8")
    s = s.replace("value: 0.55", "value: 1")
    s = s.replace("value: 0.58", "value: 1.055")
    start_ctrl.write_text(s, encoding="utf-8")
    print(f"Updated {start_ctrl}")

    opt_ctrl = ROOT / "Assets/Resources/Controllers/OptionsMenu.controller"
    o = opt_ctrl.read_text(encoding="utf-8")
    o = o.replace("value: 0.90099996", "value: 1")
    o = o.replace("value: 0.9", "value: 1")
    opt_ctrl.write_text(o, encoding="utf-8")
    print(f"Updated {opt_ctrl}")

    exit_ctrl = ROOT / "Assets/Resources/Controllers/ExitMenu.controller"
    e = exit_ctrl.read_text(encoding="utf-8")
    e = e.replace("value: 5.96", "value: 1")
    e = e.replace("value: {x: 5.96, y: 5.96, z: 5.96}", "value: {x: 1, y: 1, z: 1}")
    exit_ctrl.write_text(e, encoding="utf-8")
    print(f"Updated {exit_ctrl}")


if __name__ == "__main__":
    main()
