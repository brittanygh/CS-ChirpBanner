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

      public UILabel Description;
      public UIColorField ColorField;

      public override void Awake()
      {
         base.Awake();

         Description = AddUIComponent<UILabel>();
         ColorField = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField>().gameObject).GetComponent<UIColorField>();//AddUIComponent<UIColorField>();
         this.AttachUIComponent(ColorField.gameObject);

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
                  
         ColorField.name = "BannerColorPicker";
         
         ColorField.width = 25;
         ColorField.height = 25;
         ColorField.pickerPosition = UIColorField.ColorPickerPosition.LeftBelow;

         ColorField.relativePosition = new Vector3(inset, 0, 0);
         ColorField.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
         ColorField.isInteractive = true;
         ColorField.builtinKeyNavigation = true;
         ColorField.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
         ColorField.hoveredBgSprite = "ColorPickerOutlineHovered";
         ColorField.normalBgSprite = "ColorPickerOutline";
         ColorField.normalFgSprite = "ColorPickerColor";
         ColorField.outlineSize = 1;
                    
         //Description.text = "(no label)";
         Description.autoHeight = true;
         Description.autoSize = true;
         Description.padding = new RectOffset(2, 2, 2, 2);
         Description.relativePosition = new Vector3(ColorField.relativePosition.x + ColorField.width + inset, 0, 0);
         
      }
   }
}
