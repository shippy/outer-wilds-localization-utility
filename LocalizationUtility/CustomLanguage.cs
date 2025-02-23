﻿using OWML.ModHelper;
using System;
using UnityEngine;

namespace LocalizationUtility
{
    public class CustomLanguage
    {
        public string Name { get; private set; }
        public string TranslationPath { get; private set; }
        public Font Font { get; private set; }
        public Func<string, string> Fixer { get; private set; }
        public TextTranslation.Language LanguageToReplace { get; private set; }

        public CustomLanguage(string name, string translationPath, ModBehaviour mod, TextTranslation.Language languageToReplace)
        {
            Name = name;
            TranslationPath = mod.ModHelper.Manifest.ModFolderPath + translationPath;
            LanguageToReplace = languageToReplace;
        }

        public void AddFont(string assetBundlePath, string fontPath, ModBehaviour mod)
        {
            try
            {
                Font = AssetBundle.LoadFromFile(mod.ModHelper.Manifest.ModFolderPath + assetBundlePath).LoadAsset<Font>(fontPath);
            }
            catch (Exception) { };

            if (Font == null) LocalizationUtility.WriteError($"Couldn't load font at {assetBundlePath} in bundle {fontPath}");
        }

        public void AddFixer(Func<string, string> fixer)
        {
            Fixer = fixer;
        }
    }
}
