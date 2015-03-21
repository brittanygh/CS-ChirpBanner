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
    public class ChirpyBannerMod : IUserMod
    {
       public string Description
       {
          get { return "Replaces Chirpy with a scrolling Marquee banner."; }
       }

       public string Name
       {
          get { return "Chirpy Banner"; }
       }
    }

   public class ChirpyBanner : IChirperExtension
   {
      public static MyConfig CurrentConfig;
      public static BannerPanel theBannerPanel;

      private Component ChirpFilter_FilterModule = null;
      private static BannerConfiguration theBannerConfigPanel;

      public void OnCreated(IChirper chirper)
      {
         // read config file for settings
         string configName = "ChirpBannerConfig.xml";
         CurrentConfig = MyConfig.Deserialize(configName);

         if (CurrentConfig == null)
         {
            CurrentConfig = new MyConfig();

            MyConfig.Serialize(configName, CurrentConfig);
         }
         // if old version, update with new
         else if (CurrentConfig.version == 0 || CurrentConfig.version < 4) // update this when we add any new settings                  
         {
            CurrentConfig.version = 4;
            MyConfig.Serialize(configName, CurrentConfig);
         }

         if (CurrentConfig.DestroyBuiltinChirper)
         {
            chirper.DestroyBuiltinChirper();
         }

         CreateBannerConfigUI();
         CreateBannerUI();
      }

      public void OnMessagesUpdated()
      {
      }

      public void OnNewMessage(IChirperMessage message)
      {
         if (message != null && theBannerPanel != null)
         {
            try
            {
               string citizenMessageID = string.Empty;

               CitizenMessage cm = message as CitizenMessage;
               if (cm != null)
               {
                  //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("found citmess MessageID: {0} GetText: {1}", cm.m_messageID, cm.GetText()));
                  citizenMessageID = cm.m_messageID;
               }

               if (!string.IsNullOrEmpty(citizenMessageID))
               {
                  // Integrate with ChirpFilter
                  if (ChirpFilter_FilterModule == null)
                  {
                     GameObject ChirpFilter_GameObject = GameObject.Find("ChirperFilterModule");

                     if (ChirpFilter_GameObject != null)
                     {
                        ChirpFilter_FilterModule = ChirpFilter_GameObject.GetComponent("ChirpFilter.FilterModule");
                     }
                  }

                  if (ChirpFilter_FilterModule != null)
                  {
                     bool bIsBlacklisted = false;

                     if (ChirpFilter_FilterModule is IFormattable)
                     {
                        IFormattable ifs = ChirpFilter_FilterModule as IFormattable;


                        string sresult = ifs.ToString(citizenMessageID, null);

                        if (string.IsNullOrEmpty(sresult) || sresult == "false")
                        {
                           bIsBlacklisted = false;
                        }
                        else if (sresult == "true")
                        {
                           bIsBlacklisted = true;
                        }
                     }

                     // see if ChirpFilter wants us to filter (not show) the message
                     if (bIsBlacklisted)
                     {
                        //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("msgid {0} msg {1} blacklisted", citizenMessageID, message.text));
                        return;
                     }
                  }
               }
            }
            catch (Exception ex)
            {
               DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("ChirpBanner.OnNewMessage threw Exception: {0}", ex.Message));
            }
           
            // use rich styled text
            // Colossal markup uses sliiiiiightly different tags than unity.
            // munge our config strings to fit
            string nameColorTag = CurrentConfig.NameColor;
            string textColorTag = CurrentConfig.MessageColor;

            if (nameColorTag.Length == 9) // ie: #001122FF
            {
               nameColorTag = nameColorTag.Substring(0, 7); // drop alpha bits
            }

            if (textColorTag.Length == 9) // ie: #001122FF
            {
               textColorTag = textColorTag.Substring(0, 7); // drop alpha bits
            }
            
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("chirp! {0}", message.text));

            string str = String.Format("<color{0}>{1}</color> : <color{2}>{3}</color>", nameColorTag, message.senderName, textColorTag, message.text);
            theBannerPanel.CreateBannerLabel(str);
         }
      }

      public void OnReleased()
      {
         if (theBannerPanel != null)
         {
            theBannerPanel = null;
         }
      }

      public void OnUpdate()
      {
      }

      private void CreateBannerConfigUI()
      {
         // create UI using ColossalFramework UI classes
         UIView uiv = UIView.GetAView();

         if (uiv == null)
         {
            return;
         }

         theBannerConfigPanel = (BannerConfiguration)uiv.AddUIComponent(typeof(BannerConfiguration));

         if (theBannerConfigPanel != null)
         {
            theBannerConfigPanel.isVisible = false; // start off hidden, until banner clicked
            theBannerConfigPanel.Initialize(this);
         }
      }

      private void CreateBannerUI()
      {
         // create UI using ColossalFramework UI classes
         UIView uiv = UIView.GetAView();

         if (uiv == null)
         {
            return;
         }

         theBannerPanel = (BannerPanel)uiv.AddUIComponent(typeof(BannerPanel));

         if (theBannerPanel != null)
         {
            //theBannerPanel.ScrollSpeed = CurrentConfig.ScrollSpeed;
            //byte bAlpha = (byte)(ushort)(255f * CurrentConfig.BackgroundAlpha);

            //theBannerPanel.BackgroundColor = new Color32(0, 0, 0, bAlpha);
            //theBannerPanel.FontSize = CurrentConfig.TextSize;
            theBannerPanel.Initialize();

            // add mouse click handler to us here
            //theBannerPanel.eventClick += BannerPanel_eventClick;
            theBannerPanel.eventMouseUp += BannerPanel_eventMouseUp;
         }
      }

      public void BannerPanel_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
      }

      public void BannerPanel_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         if (eventParam.buttons == UIMouseButton.Right)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, eventParam.position.ToString());
            // show config window at position of click
            theBannerConfigPanel.ShowPanel(eventParam.position);
         }
      }

   }

   public class BannerLabelStruct
   {
      public Vector3 RelativePosition; // relative to parent BannerPanel
      public UILabel Label;
      public bool IsCurrentlyVisible;
   }

   // A UIPanel that contains a queue of UILabels (one for each chirp to be displayed)
   // - as UILabels are added, the are put in a queue with geometry information (current position, extents)
   // - OnUpdate() handler moves each panel across the screen (all in line, like ducks :)
   // - when a UILable moves to the left such that it's out of the panel, it gets deleted and popped off the queue
   public class BannerPanel : UIPanel
   {
      // members
      bool Shutdown = false;

      const int banner_inset = 60;
      const int label_y_inset = 5;

      public Color32 BackgroundColor = new Color32(0, 0, 0, 0xff);
      UIFont OurLabelFont = null;

      Queue<BannerLabelStruct> BannerLabelsQ = new Queue<BannerLabelStruct>();
      
      public void Initialize()
      {
         UIView uiv = UIView.GetAView();

         if (uiv == null)
         {
            Cleanup();
            return;
         }

         int viewWidth = (int)uiv.GetScreenResolution().x;
         int viewHeight = (int)uiv.GetScreenResolution().y;
         
         this.autoSize = true;

         this.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
         this.backgroundSprite = "GenericPanel";

         this.position = new Vector3((-viewWidth / 2) + banner_inset, (viewHeight / 2));
         this.width = viewWidth - (banner_inset * 2);
        
         this.height = 25;

         this.color = BackgroundColor;
         this.opacity = (1f - ChirpyBanner.CurrentConfig.BackgroundAlpha);

         this.autoLayout = false;
         this.clipChildren = true;
         //this.isInteractive = true;

         this.SendToBack();
      }

      public void Cleanup()
      {
         Shutdown = true;
      }

      public void CreateBannerLabel(string chirpStr)
      {
         if (Shutdown || string.IsNullOrEmpty(chirpStr))
         {
            return;
         }

         UILabel newLabel = (UILabel)this.AddUIComponent<UILabel>();

         if (newLabel != null)
         {            
            //newLabel.autoSize = true;
            //newLabel.autoHeight = true;
            newLabel.verticalAlignment = UIVerticalAlignment.Middle;
            newLabel.textAlignment = UIHorizontalAlignment.Left;

            newLabel.height = this.height;
            newLabel.padding = new RectOffset(0, 0, 5, 5);
            newLabel.relativePosition = new Vector3(this.width, 0);

            newLabel.textScaleMode = UITextScaleMode.ScreenResolution;
            newLabel.textScale = (float)ChirpyBanner.CurrentConfig.TextSize / 20f;

            newLabel.processMarkup = true;
            newLabel.text = chirpStr;

            if (OurLabelFont == null)
            {
               OurLabelFont = newLabel.font;
            }
            
           
            if (OurLabelFont != null)
            {
               //OurLabelFont.size = ChirpyBanner.CurrentConfig.TextSize;
               UIFontRenderer fr = OurLabelFont.ObtainRenderer();
               fr.textScale = newLabel.textScale;

               if (fr != null)
               {
                  Vector2 ms = fr.MeasureString(chirpStr);

                  newLabel.width = ms.x;
                  newLabel.height = ms.y;

                  if (this.height != (newLabel.height + 0))
                  {
                     this.height = newLabel.height + 0;
                  }
               }
            }

            BannerLabelStruct bls = new BannerLabelStruct();
            bls.RelativePosition = new Vector3(this.width, label_y_inset); // starting position is off screen, at max extent of parent panel
            bls.Label = newLabel;
            bls.IsCurrentlyVisible = false;


            // add to queue
            BannerLabelsQ.Enqueue(bls);
         }
      }

      // Monobehaviour mehods
      public override void OnDestroy()
      {
         Cleanup();
         base.OnDestroy();
      }

      public void OnGUI()
      {
         if (Shutdown)
         {
            return;
         }

         // if game is in free camera (screenshot/cinematic) mode, don't display chirp banner
         GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
         if (gameObject != null)
         {
            CameraController cameraController = gameObject.GetComponent<CameraController>();

            if (cameraController != null)
            {
               if (cameraController.m_freeCamera)
               {
                  return;
               }
            }
         }

         // HideUI mod uses different mechanism to hide ui - it looks like it's disabling the UIView camera.
         // - check and see if uiview cam is disabled, and if so don't show banner
         var cameras = GameObject.FindObjectsOfType<Camera>();
         foreach (var cam in cameras)
         {
            if (cam.name == "UIView")
            {
               if (!cam.enabled)
               {
                  return;
               }

               break;
            }
         }

         // move the labels in the banner across the screen
         // - foreach label that is visible, move left by step
         // - if the first one is off screen pop it off queue
         // - if the last one is fully onscreen, start moving next in line

         bool bPopIt = false;
         float currentTrailingEdge = 0;

         // A queue can be enumerated without disturbing its contents. 
         foreach (BannerLabelStruct bls in BannerLabelsQ)
         {
            if (bls.IsCurrentlyVisible)
            {
               bls.Label.relativePosition = new Vector3(bls.Label.relativePosition.x - Time.deltaTime * ChirpyBanner.CurrentConfig.ScrollSpeed, bls.Label.relativePosition.y, bls.Label.relativePosition.z);
               bls.RelativePosition = bls.Label.relativePosition;

               // is it off to the left entirely?                              
               if ((bls.Label.relativePosition.x + bls.Label.width) <= 0)
               {
                  bPopIt = true;
                  bls.IsCurrentlyVisible = false;
                  bls.Label.isVisible = false;
               }
               else
               {
                  // still in banner (in whole or in part)
                  currentTrailingEdge += bls.RelativePosition.x + bls.Label.width;
                  bls.Label.isVisible = true;
               }
            }
            else // I'm not yet visible...
            {
               // is there room for me to start scrolling?
               if (currentTrailingEdge < this.width)
               {
                  // yes, there's room
                  bls.Label.relativePosition = new Vector3(this.width, bls.Label.relativePosition.y, bls.Label.relativePosition.z);
                  bls.RelativePosition = bls.Label.relativePosition;
                  bls.IsCurrentlyVisible = true;
                  bls.Label.isVisible = true;
                  currentTrailingEdge += bls.RelativePosition.x + bls.Label.width; 
               }
            }
         }

         if (bPopIt)
         {
            BannerLabelStruct blsfree = BannerLabelsQ.Dequeue();
            this.RemoveUIComponent(blsfree.Label);
         }
      }
   }

   public class MyConfig
   {      
      public bool DestroyBuiltinChirper = true;
      public int ScrollSpeed = 50;
      public int TextSize = 20; // pixels
      public string MessageColor = "#FFFFFFFF";
      public string NameColor = "#31C3FFFF";
      public int version = 0;
      public float BackgroundAlpha = 0.4f;

      public static void Serialize(string filename, MyConfig config)
      {
         try
         {
            var serializer = new XmlSerializer(typeof(MyConfig));

            using (var writer = new StreamWriter(filename))
            {
               serializer.Serialize(writer, config);
            }
         }
         catch { }
      }

      public static MyConfig Deserialize(string filename)
      {
         var serializer = new XmlSerializer(typeof(MyConfig));

         try
         {
            using (var reader = new StreamReader(filename))
            {
               MyConfig config = (MyConfig)serializer.Deserialize(reader);

               // sanity checks
               if (config.ScrollSpeed < 1) config.ScrollSpeed = 1;
               if (config.ScrollSpeed > 200) config.ScrollSpeed = 200;
               if (config.TextSize < 1 || config.TextSize > 100) config.TextSize = 20;

               if (config.BackgroundAlpha < 0f)
               {
                  config.BackgroundAlpha = 0f;                     
               }
               else if (config.BackgroundAlpha > 1.0f)
               {
                  config.BackgroundAlpha = 1.0f;
               }

               return config;
            }
         }
         catch { }

         return null;
      }

   }
}
