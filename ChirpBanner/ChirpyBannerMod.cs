using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
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
                  marquee.SetMaxChirps(config.MaxChirps);
                  marquee.SetScrollSpeed(config.ScrollSpeed);
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
            string str = String.Format("{0} : {1}", message.senderName, message.text);
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

   public class ChirpMarquee : MonoBehaviour
   {
      static int numChirps = 3;
      Queue<string> msgQ = new Queue<string>(numChirps);
      float scrollSpeed = 30;
      bool Shutdown = false;
      bool binit = true;

      Rect messageRect;

      public void SetMaxChirps(int i)
      {
         numChirps = i;
         msgQ = new Queue<string>(numChirps);
      }

      public void SetScrollSpeed(int speed)
      {
         scrollSpeed = speed;
      }

      public void AddChirp(string chirpStr)
      {
         if (Shutdown)
         {
            return;
         }

         while (msgQ.Count >= numChirps)
         {
            msgQ.Dequeue();
         }

         msgQ.Enqueue(chirpStr);
      }
      
      void OnDestroy()
      {
         Shutdown = true;
         msgQ.Clear();
      }

      void OnGUI()
      {
         if (Shutdown)
         {
            return;
         }

         string message = "";

         // A queue can be enumerated without disturbing its contents. 
         foreach (string chirpStr in msgQ)
         {
            if (!string.IsNullOrEmpty(message))
            {
               message += "   \u2600   "; // separator char
            }
            message += chirpStr;
         }

         Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(message));

         // Set up the message's rect if we haven't already
         if (binit)
         {
            binit = false;
            // Start the message past the left side of the screen
            messageRect.x = Screen.width; // was = -dimensions.x;
         }
         
         messageRect.width = dimensions.x;
         messageRect.height = dimensions.y;         

         messageRect.x -= Time.deltaTime * scrollSpeed;

         // If the message has moved past the right side, move it back to the left
         if ((messageRect.x + messageRect.width) < 0) // was > Screen.width)
         {
            messageRect.x = Screen.width;// was = -messageRect.width;
         }

         GUI.Label(messageRect, message);
      }
   }

   public class MyConfig
   {
      public bool DestroyBuiltinChirper = false;
      public int MaxChirps = 3;
      public int ScrollSpeed = 30;

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
               if (config.MaxChirps < 1) config.MaxChirps = 1;
               if (config.MaxChirps > 10) config.MaxChirps = 10;
               if (config.ScrollSpeed < 1) config.ScrollSpeed = 1;
               if (config.ScrollSpeed > 100) config.ScrollSpeed = 100;

               return config;
            }
         }
         catch { }

         return null;
      }

   }
}
