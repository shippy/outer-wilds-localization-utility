﻿using HarmonyLib;
using System;
using System.Reflection;
using System.Xml;

namespace LocalizationUtility
{
    [HarmonyPatch]
    public static class TextTranslationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.SetLanguage))]
        public static bool TextTranslation_SetLanguage(TextTranslation.Language lang, TextTranslation __instance)
        {
            if (lang > TextTranslation.Language.TURKISH) lang = LocalizationUtility.languageToReplace;

            if (lang != LocalizationUtility.languageToReplace) return true;

            __instance.m_language = lang;

            var language = LocalizationUtility.Instance.GetLanguage();

            var path = language.TranslationPath;

            LocalizationUtility.WriteLine($"Loading translation from {path}");

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                var translationTableNode = xmlDoc.SelectSingleNode("TranslationTable_XML");
                var translationTable_XML = new TextTranslation.TranslationTable_XML();

                // Add regular text to the table
                foreach (XmlNode node in translationTableNode.SelectNodes("entry"))
                {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table.Add(new TextTranslation.TranslationTableEntry(key, value));
                }

                // Add ship log entries
                foreach (XmlNode node in translationTableNode.SelectSingleNode("table_shipLog").SelectNodes("TranslationTableEntry"))
                {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table_shipLog.Add(new TextTranslation.TranslationTableEntry(key, value));
                }

                // Add UI
                foreach (XmlNode node in translationTableNode.SelectSingleNode("table_ui").SelectNodes("TranslationTableEntryUI"))
                {
                    // Keys for UI are all ints
                    var key = int.Parse(node.SelectSingleNode("key").InnerText);
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table_ui.Add(new TextTranslation.TranslationTableEntryUI(key, value));
                }

                __instance.m_table = new TextTranslation.TranslationTable(translationTable_XML);

                // Goofy stuff to envoke event
                var onLanguageChanged = (MulticastDelegate)__instance.GetType().GetField("OnLanguageChanged", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (onLanguageChanged != null)
                {
                    onLanguageChanged.DynamicInvoke();
                }
            }
            catch (Exception e)
            {
                LocalizationUtility.WriteError($"Couldn't load translation: {e.Message}{e.StackTrace}");
                return true;
            }

            return false;
        }
    }
}
