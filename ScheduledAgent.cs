using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Phone.Shell;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace HighscoreBackgroundAgent {
    public class ScheduledAgent : ScheduledTaskAgent {

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent() {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (Debugger.IsAttached) {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        private bool tileupdated;

        protected override void OnInvoke(ScheduledTask task) {
            //TODO: Add code to perform your task in background
            tileupdated = false;
            
            QueryHighscoreAndUpdateTile();

            int waitCount = 0;
            while (!tileupdated && waitCount < 5) {
                waitCount++;
                Thread.Sleep(300);
            }
            NotifyComplete();
        }

        private async void QueryHighscoreAndUpdateTile() {
            MobileServiceClient client = new MobileServiceClient("https://microchopper.azure-mobile.net/", "foobar");
            JToken token = await client.GetTable("highscores").ReadAsync("$orderby=score desc&$top=1");
            if (token[0] != null) {
                float highscore = (float)token[0]["score"];
                UpdateTileWithHighscore(highscore);
            }
        }

        private void UpdateTileWithHighscore(float highscore) {
            ShellTile tile = ShellTile.ActiveTiles.First();
            if (tile != null) {
                IconicTileData data = new IconicTileData();
                data.Count = (int)highscore;
                data.Title = "Chopper highscore";
                tile.Update(data);
            }
            tileupdated = true;
        }

    }
}