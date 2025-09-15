using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;

public class LocalizeStringAndFontComppnent_TMP
{
   [MenuItem("CONTEXT/TextMeshProUGUI/LocalizeStringAndFont")]
   static void LocalizeTMProTextAndFont(MenuCommand command)
   {
      var target = command.context as TextMeshProUGUI;
      SetupForStringAndFontLocalization(target);
   }
   
   public static MonoBehaviour SetupForStringAndFontLocalization(TextMeshProUGUI target)
   {
      if (target == null)
         return null;
      if (target.gameObject.GetComponent<LocalizeFontEvent>() != null)
         return null;
      var componentFont = Undo.AddComponent(target.gameObject,typeof(LocalizeFontEvent)) as LocalizeFontEvent;
      var setFontMethod = target.GetType().GetProperty("font").GetSetMethod();
      var fontMethodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<TMP_FontAsset>), target, setFontMethod)  as UnityAction<TMP_FontAsset>;
      UnityEventTools.AddPersistentListener(componentFont.OnUpdateAsset,fontMethodDelegate);
      componentFont.OnUpdateAsset.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
      
      var componentString = target.gameObject.GetComponent<LocalizeStringEvent>();
      if (componentString != null)
      {
         return null;
      }
      var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
      var stringMethodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
      UnityEventTools.AddPersistentListener(componentString.OnUpdateString,stringMethodDelegate);
      componentString.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
      return null;
   }
}
