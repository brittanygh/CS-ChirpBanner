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
   class UISliderSubPanel : UIPanel
   {
      public BannerConfiguration ParentBannerConfig;

      public UISlider Slider;
      public UILabel Description;

      public override void Awake()
      {
         base.Awake();

         Slider = AddUIComponent<UISlider>();
         Description = AddUIComponent<UILabel>();

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

         Slider.width = (this.width / 2) - inset;
         Slider.height = 15;

         UISprite thumbSprit = Slider.AddUIComponent<UISprite>();
         thumbSprit.spriteName = "SliderBudget";

         Slider.backgroundSprite = "ScrollbarTrack";
         Slider.thumbObject = thumbSprit;

         //Slider.minValue = 0;
         //Slider.maxValue = 100;
         //Slider.stepSize = 5;
         //Slider.value = 40;
         //Slider.scrollWheelAmount = 5;
         Slider.orientation = UIOrientation.Horizontal;
         Slider.isVisible = true;
         Slider.enabled = true;
         Slider.canFocus = true;
         Slider.isInteractive = true;
         Slider.relativePosition = new Vector3(inset, 0, 0);

         //Description.text = "(no label)";
         Description.autoHeight = true;
         Description.autoSize = true;
         Description.relativePosition = new Vector3(Slider.relativePosition.x + Slider.width + inset, 0, 0);
      }
   }
}
