using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;
namespace ChirpBanner
{
   // consists of a checkbox followed by a descriptive label
   class UICheckSubPanel : UIPanel
   {
      public BannerConfiguration ParentBannerConfig;

      public UIButton Checkbox; // UICheckBox doesn't seem to work quite right
      public UILabel Description;
      
      public bool Checked
      {
         get
         {
            if (Checkbox != null)
            {
               return Checkbox.normalFgSprite == "check-checked";
            }

            return false;
         }
         set
         {
            if (Checkbox == null)
            {
               return;
            }

            if (value == Checked)
            {
               return;
            }

            Checkbox.normalFgSprite = value ? "check-checked" : "check-unchecked";
         }
      }
      
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet

         Checkbox = AddUIComponent<UIButton>();
         Description = AddUIComponent<UILabel>();

         // some defaults
         this.height = 40;
         this.width = 400;
         Checkbox.normalFgSprite = "check-unchecked";
      }

      public override void Start()
      {
         base.Start();

         if (ParentBannerConfig == null)
         {
            return;
         }

         // setup now that we are in parent
         this.width = ParentBannerConfig.width;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         this.relativePosition = Vector3.zero;

         int inset = 5;

         Checkbox.relativePosition = new Vector3(inset, 0);
         Checkbox.eventClick += (Component, param) => { this.Checked = !this.Checked; };

         Description.text = "Hide the built-in Chirper";
         Description.autoHeight = true;
         Description.autoSize = true;
         Description.relativePosition = new Vector3(Checkbox.relativePosition.x + Checkbox.width + inset, 0);

      }
   }
}
