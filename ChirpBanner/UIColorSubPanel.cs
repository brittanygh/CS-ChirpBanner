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
   class UIColorSubPanel : UIPanel
   {
      public BannerConfiguration ParentBannerConfig;

      public UITextField ColorText; // until we can get color picker working
      public UILabel Description;

      //public UIColorField ColorField;


      public override void Awake()
      {
         base.Awake();

         ColorText = AddUIComponent<UITextField>();
         Description = AddUIComponent<UILabel>();
         //ColorField = AddUIComponent<UIColorField>();

         this.height = 40;
         this.width = 400;
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

         ColorText.autoSize = true;
         ColorText.maxLength = 8;
         ColorText.allowFloats = false;
         ColorText.isInteractive = true;
         ColorText.selectOnFocus = true;
         ColorText.selectionSprite = "EmptySprite";
         ColorText.hoveredBgSprite = "TextFieldPanelHovered";
         ColorText.normalBgSprite = "GenericLightPanel";
         ColorText.focusedBgSprite = "TextFieldPanel";
         ColorText.builtinKeyNavigation = true;
         ColorText.Enable();
         ColorText.relativePosition = new Vector3(inset, 0, 0);
         
         //Description.text = "(no label)";
         Description.autoHeight = true;
         Description.autoSize = true;
         Description.relativePosition = new Vector3(ColorText.relativePosition.x + ColorText.width + inset, 0, 0);

      }
   }
}
