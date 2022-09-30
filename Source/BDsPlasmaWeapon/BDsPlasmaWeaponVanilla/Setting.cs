using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeaponVanilla
{
    public class Setting : ModSettings
    {
        public bool OverchargeDamageWeapon = true;

        public bool CustomPlayerProjectileColor = true;

        public bool useFactionColor = false;

        public bool discoLightMode = false;

        public bool isRGB = true;

        public float CustomPlayerProjectileColorR;
        public float CustomPlayerProjectileColorG;
        public float CustomPlayerProjectileColorB;

        public float CustomPlayerProjectileColorH;
        public float CustomPlayerProjectileColorS;
        public float CustomPlayerProjectileColorV;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref OverchargeDamageWeapon, "OverchargeDamageWeapon", defaultValue: true);
            Scribe_Values.Look(ref CustomPlayerProjectileColor, "CustomPlayerProjectileColor", defaultValue: true);
            Scribe_Values.Look(ref useFactionColor, "useFactionColor", defaultValue: false);
            Scribe_Values.Look(ref discoLightMode, "discoLightMode", defaultValue: false);
            Scribe_Values.Look(ref isRGB, "isRGB", defaultValue: true);
            Scribe_Values.Look(ref CustomPlayerProjectileColorR, "CustomPlayerProjectileColorR", defaultValue: 1);
            Scribe_Values.Look(ref CustomPlayerProjectileColorG, "CustomPlayerProjectileColorG", defaultValue: 1);
            Scribe_Values.Look(ref CustomPlayerProjectileColorB, "CustomPlayerProjectileColorB", defaultValue: 1);
            Scribe_Values.Look(ref CustomPlayerProjectileColorH, "CustomPlayerProjectileColorH", defaultValue: 1);
            Scribe_Values.Look(ref CustomPlayerProjectileColorS, "CustomPlayerProjectileColorS", defaultValue: 1);
            Scribe_Values.Look(ref CustomPlayerProjectileColorV, "CustomPlayerProjectileColorV", defaultValue: 1);
        }
    }

    public class BDPMod : Mod
    {
        public static Setting settings;

        public static bool CustomPlayerProjectileColor => settings.CustomPlayerProjectileColor;

        public static bool OverchargeDamageWeapon => settings.OverchargeDamageWeapon;

        public static bool useFactionColor => settings.useFactionColor;

        public static bool discoLightMode => settings.discoLightMode;

        public static float CustomPlayerProjectileColorR => settings.CustomPlayerProjectileColorR;

        public static float CustomPlayerProjectileColorG => settings.CustomPlayerProjectileColorG;

        public static float CustomPlayerProjectileColorB => settings.CustomPlayerProjectileColorB;

        public static float CustomPlayerProjectileColorH => settings.CustomPlayerProjectileColorH;

        public static float CustomPlayerProjectileColorS => settings.CustomPlayerProjectileColorS;

        public static float CustomPlayerProjectileColorV => settings.CustomPlayerProjectileColorV;

        public static bool isRGB => settings.isRGB;

        public BDPMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("BDP_OverchargeDamageWeapon_Title".Translate(), ref settings.OverchargeDamageWeapon, "BDP_OverchargeDamageWeapon_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_CustomPlayerProjectileColor_Title".Translate(), ref settings.CustomPlayerProjectileColor, "BDP_CustomPlayerProjectileColor_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_useFactionColor_Title".Translate(), ref settings.useFactionColor, "BDP_useFactionColor_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_discoLightMode_Title".Translate(), ref settings.discoLightMode, "BDP_discoLightMode_Desc".Translate());
            if (CustomPlayerProjectileColor && !useFactionColor)
            {
                listing_Standard.Gap();
                if (listing_Standard.RadioButton("BDP_SettingRGB", isRGB))
                {
                    settings.isRGB = true;
                }
                if (listing_Standard.RadioButton("BDP_SettingHSV", !isRGB))
                {
                    settings.isRGB = false;
                }
                if (isRGB)
                {
                    listing_Standard.Label("R: " + Math.Round(CustomPlayerProjectileColorR * 255).ToString());
                    settings.CustomPlayerProjectileColorR = listing_Standard.Slider(settings.CustomPlayerProjectileColorR, 0, 1);
                    listing_Standard.Label("G: " + Math.Round(CustomPlayerProjectileColorG * 255).ToString());
                    settings.CustomPlayerProjectileColorG = listing_Standard.Slider(settings.CustomPlayerProjectileColorG, 0, 1);
                    listing_Standard.Label("B: " + Math.Round(CustomPlayerProjectileColorB * 255).ToString());
                    settings.CustomPlayerProjectileColorB = listing_Standard.Slider(settings.CustomPlayerProjectileColorB, 0, 1);
                    Rect ColorPreview = new Rect(0, 500, 100, 100);
                    Texture2D bullet = ContentFinder<Texture2D>.Get("Things/Projectile/Bullet_Small", false);
                    Color color = new Color(settings.CustomPlayerProjectileColorR, settings.CustomPlayerProjectileColorG, settings.CustomPlayerProjectileColorB);
                    Color.RGBToHSV(color, out settings.CustomPlayerProjectileColorH, out settings.CustomPlayerProjectileColorS, out settings.CustomPlayerProjectileColorV);
                    GUI.DrawTexture(ColorPreview, bullet, ScaleMode.ScaleToFit, true, 0, color, 0, 0);
                }
                else
                {
                    listing_Standard.Label("H: " + Math.Round(CustomPlayerProjectileColorH * 360).ToString());
                    settings.CustomPlayerProjectileColorH = listing_Standard.Slider(settings.CustomPlayerProjectileColorH, 0, 1);
                    listing_Standard.Label("S: " + Math.Round(CustomPlayerProjectileColorS * 100).ToString() + "%");
                    settings.CustomPlayerProjectileColorS = listing_Standard.Slider(settings.CustomPlayerProjectileColorS, 0, 1);
                    listing_Standard.Label("V: " + Math.Round(CustomPlayerProjectileColorV * 100).ToString() + "%");
                    settings.CustomPlayerProjectileColorV = listing_Standard.Slider(settings.CustomPlayerProjectileColorV, 0, 1);
                    Rect ColorPreview = new Rect(0, 500, 100, 100);
                    Texture2D bullet = ContentFinder<Texture2D>.Get("Things/Projectile/Bullet_Small", false);
                    Color color = Color.HSVToRGB(settings.CustomPlayerProjectileColorH, settings.CustomPlayerProjectileColorS, settings.CustomPlayerProjectileColorV);
                    settings.CustomPlayerProjectileColorR = color.r;
                    settings.CustomPlayerProjectileColorG = color.g;
                    settings.CustomPlayerProjectileColorB = color.b;
                    GUI.DrawTexture(ColorPreview, bullet, ScaleMode.ScaleToFit, true, 0, color, 0, 0);
                }

            }
            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "BDP_Setting".Translate();
        }
    }
}
