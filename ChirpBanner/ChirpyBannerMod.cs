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
          get { return "Chirpy Banner"; }
       }

       public string Name
       {
          get { return "Replaces Chirpy with a scrolling Marquee banner."; }
       }
    }

   public class ChirpyBanner : IChirperExtension
   {
      private static GameObject BannerWindow;
      private static ChirpMarquee marquee;
      private string MessageColor = "#FFFFFFFF";
      private string NameColor = "#31C3FFFF";
      private Component ChirpFilter_FilterModule = null;

      public void OnCreated(IChirper chirper)
      {       
         // read config file for settings
         string configName = "ChirpBannerConfig.xml";
         MyConfig config = MyConfig.Deserialize(configName);

         if (config == null)
         {
            config = new MyConfig();

            MyConfig.Serialize(configName, config);
         }
         // if old version, update with new
         else if (config.version == 0 || config.version < 3) // update this when we add any new settings                  
         {
            config.DestroyBuiltinChirper = true; // update these 2 to better values when users get this version
            config.ScrollSpeed = 50;

            config.version = 3;
            MyConfig.Serialize(configName, config);
         }         

         MessageColor = config.MessageColor;
         NameColor = config.NameColor;

         if (config.DestroyBuiltinChirper)
         {
            chirper.DestroyBuiltinChirper();
         }

         if (BannerWindow == null)
         {            
            UIView ui = GameObject.FindObjectOfType<UIView>();
            
            if (ui != null)
            {
               BannerWindow = new GameObject("ChirpyBannerWindow", typeof(ColossalFramework.UI.UILabel));

               BannerWindow.transform.parent = ui.transform;
               marquee = BannerWindow.AddComponent<ChirpMarquee>();

               if (marquee != null)
               {
                  marquee.SetScrollSpeed(config.ScrollSpeed);
                  marquee.SetTextSize(config.TextSize);
                  marquee.SetBackgroundAlpha(config.BackgroundAlpha);
                  marquee.AddChirp("Chirp Banner!");                 
               }
            }
         }
      }

      public void OnMessagesUpdated()
      {
      }

      public void OnNewMessage(IChirperMessage message)
      {
         if (message != null && marquee != null)
         {
            try
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

                     string sresult = ifs.ToString(message.text, null);

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
                     return;
                  }
               }
            }
            catch (Exception ex)
            {
               DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("ChirpBanner.OnNewMessage threw Exception: {0}", ex.Message));
            }

            // use rich styled text
            string str = String.Format("<color={0}>{1}</color> : <color={2}>{3}</color>", NameColor, message.senderName, MessageColor, message.text);
            marquee.AddChirp(str);           
         }
      }

      public void OnReleased()
      {
         if (BannerWindow != null)
         {
            BannerWindow = null;
         }

         if (marquee != null)
         {
            marquee = null;
         }
      }

      public void OnUpdate()
      {
      }
   }

   public class ChirpStruct
   {
      public string chirp;
      public int trips = -1; // the number of times the chirp has fully traversed the screen

      public ChirpStruct(string msg)
      {
         chirp = msg;
      }
   }

   public class ChirpMarquee : MonoBehaviour
   {
      Queue<ChirpStruct> msgQ = new Queue<ChirpStruct>(20);
      Queue<ChirpStruct> pendingChirps = new Queue<ChirpStruct>(20); // picked 20 out of a hat, don't want to lose any, but if they're coming in that fast too bad :)
      bool binit = true;

      float backAlpha = 0.4f;
      float scrollSpeed = 50;
      bool Shutdown = false;

      string message = null;
      Rect messageRect;

      int TextSize = 20;

      GUIStyle style = new GUIStyle();
      Texture2D texblack = new Texture2D(2, 2);

      public void Start()
      {
         for (int i = 0; i < texblack.width; i++)
         {
            for (int j = 0; j < texblack.height; j++)
            {
               texblack.SetPixel(i, j, new Color(0f, 0f, 0f, backAlpha));
            }
         }
         texblack.Apply();

         //style = new GUIStyle(GUI.skin.label);

         style.normal.background = texblack;
      }

      public void SetScrollSpeed(int speed)
      {
         scrollSpeed = speed;
      }

      public void SetTextSize(int size)
      {
         TextSize = size;
      }

      public void SetBackgroundAlpha(float alpha)
      {
         backAlpha = alpha;
      }

      public void AddChirp(string chirpStr)
      {
         if (Shutdown)
         {
            return;
         }

         pendingChirps.Enqueue(new ChirpStruct(chirpStr));

         // we're not adding the chirp to the display queue until the current banner has transited the screen

         //while (msgQ.Count >= numChirps)
         //{
         //   msgQ.Dequeue();
         //}

         //msgQ.Enqueue(new ChirpStruct(chirpStr));
      }
      
      void OnDestroy()
      {
         Shutdown = true;
         msgQ.Clear();
         pendingChirps.Clear();
      }

      void OnGUI()
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

         // background
         Rect brect = new Rect();
         brect.x = 0;
         string dummystr = string.Format("<size={0}>Test</size>", TextSize);
         Vector2 bdim = GUI.skin.label.CalcSize(new GUIContent(dummystr));
         brect.width = Screen.width;
         brect.height = bdim.y;
         GUI.Label(brect, "", style);

         // Set up the message's rect if we haven't already
         if (binit)
         {
            binit = false;
            // Start the message past the left side of the screen
            messageRect.x = Screen.width; // was = -dimensions.x;
         }

         if (msgQ.Count != 0)
         {
            message = string.Format("<size={0}>", TextSize);

            //lock(msgQ)

            // A queue can be enumerated without disturbing its contents. 
            foreach (ChirpStruct chirpStr in msgQ)
            {
               if (!string.IsNullOrEmpty(message))
               {
                  message += "   \u2600   "; // separator char
               }
               message += chirpStr.chirp;
            }

            message += "</size>";

            Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(message));

            //// Start the message past the left side of the screen
            //messageRect.x = Screen.width; // was = -dimensions.x;

            messageRect.width = dimensions.x;
            messageRect.height = dimensions.y;

         }

         messageRect.x -= Time.deltaTime * scrollSpeed;
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("{0} : {1} m:{2} rect:{3}", msgQ.Count, pendingChirps.Count, message, messageRect));
            
        // If the message has moved past the right side, move it back to the left
         if (((messageRect.x + messageRect.width) < 0) || (msgQ.Count == 0 && pendingChirps.Count != 0)) 
         {
            //lock(msgQ)
            // we now reset the msgQ - empty it and add everything in pendingQ
            msgQ.Clear();

            while (pendingChirps.Count > 0)
            {
               ChirpStruct cs = pendingChirps.Dequeue();
               msgQ.Enqueue(cs);
            }

            // clear message string 
            message = null;
            messageRect.x = Screen.width;

            // return now without displaying anything, the next OnGUI() call will do the right thing
            return;
         }


         GUI.Label(messageRect, message);
      }
   }

   public class MyConfig
   {      
      public bool DestroyBuiltinChirper = true;
      public int MaxChirps = 3;
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
