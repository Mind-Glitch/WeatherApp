/* 
    #####################
    Made by TonyRoss
    2022.05.16 - 
    #####################
 */

using System;
using System.Net;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace WeatherApp
{
    class Program
    {
        private static DateTime dtn => DateTime.Now;
        private static string AppRequestKey(int locationKey, string apiKey) =>
            $"http://dataservice.accuweather.com/forecasts/v1/daily/5day/{locationKey}?apikey={apiKey}&language=en";

        public static void Main(string[] args) => Application().Wait();


        /// <summary>
        /// Application core
        /// </summary>
        /// <returns></returns>
        private async static Task Application()
        {
            // App start message:
            Configuration config = ReadConfiguration();
            string generatedAppReqKey = AppRequestKey(config.LocationKey, config.ApiKey);

            Console.WriteLine(
                $"All data will be saved to \n{config.DownloadedDataSavePlace} \n" +
                $"{config.LocationKey} - Location key \n{config.ApiKey} - API key"
            );

            await Task.Delay(1000);

            //Console.Clear();

            // Here it is || >> The Application <<
            while (true)
            {
                config = ReadConfiguration();
                Console.WriteLine(config.DownloadedDataSavePlace);
                generatedAppReqKey = AppRequestKey(config.LocationKey, config.ApiKey);

                // Check if info already collected
                if (/* DateTime.Now.Hour > 10 && */ !File.Exists(config.DownloadedDataSavePlace + $"/{dtn.Year}-{dtn.Month}-{dtn.Day}.json"))
                {
                    GetAndSaveDataFromServer(generatedAppReqKey, config).Wait();
                    PrintInfo(config);
                }
                else
                {
                    PrintInfo(config);
                }

                await Task.Delay(2000 + 1000 * 60 * (config.DataCollectionTimeFrequencyMinutes + config.DataCollectionTimeFrequencyHours * 60));
                Console.WriteLine("TICK");
            }
        }


        /// <summary>
        /// Download and deserialize content
        /// </summary>
        private static async Task GetAndSaveDataFromServer(string appRequestKey, Configuration config)
        {
            WeatherInfo info = new WeatherInfo();
            string weatherString = "";
            DateTime dateToday = DateTime.Now;

            if (!Directory.Exists(config.DownloadedDataSavePlace))
                Directory.CreateDirectory(config.DownloadedDataSavePlace);

            // LOADING AND WRITING (NO READ)
            using (WebClient wbc = new WebClient())
            {
                // If folder have old info
                if (File.Exists(config.DownloadedDataSavePlace + $"{dateToday.Year}-{dateToday.Month}-{dateToday.Day - 1}.json"))
                    foreach (var e in Directory.GetFiles(config.DownloadedDataSavePlace)) File.Delete(e);


                // If folder have no data inside
                if (Directory.GetFiles(config.DownloadedDataSavePlace).Length == 0)
                {
                    try
                    {
                        Console.WriteLine("Collect new data from AW server...");
                        weatherString = wbc.DownloadString(AppRequestKey(config.LocationKey, config.ApiKey));
                        info = JsonSerializer.Deserialize<WeatherInfo>(weatherString);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Data parcing error. Collected web info: " + weatherString + $"\n\n{ex}");
                        return;
                    }
                }
            }

            // WRITING
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.WriteIndented = true;


            foreach (var e in info.DailyForecasts)
            {
                await Task.Delay(100); // Functional part of deletion (cleaning) mechanism

                FileStream file = File.Create(config.DownloadedDataSavePlace + $"/{e.Date.Year}-{e.Date.Month}-{e.Date.Day}.json");
                StreamWriter sw = new StreamWriter(file);

                sw.Write(JsonSerializer.Serialize(e, jso)); sw.Flush();
                file.Close();
            }
        }


        /// <summary>
        /// Read file Configuration.json
        /// </summary>
        /// <returns></returns>
        private static Configuration ReadConfiguration()
        {
            if (!File.Exists("./Configuration.json"))
            {
                using (FileStream fs = new FileStream("./Configuration.json", FileMode.Create)) fs.Close();
                File.WriteAllText("./Configuration.json", File.ReadAllText("./DEFAULTCONFIGURATION.json"));
            }

            try
            {
                return JsonSerializer.Deserialize<Configuration>(File.ReadAllText("./Configuration.json")); ;
            }
            catch (Exception)
            {
                Console.WriteLine("Configuration loading error. Writing new Configuration...");

                return ReadConfiguration();
            }
        }


        /// <summary>
        /// Prints days forecast info
        /// </summary>
        /// <param name="config"></param>
        private static void PrintInfo(Configuration config)
        {
            List<DailyForecastClass> days = new List<DailyForecastClass>();

            foreach (var e in Directory.GetFiles(config.DownloadedDataSavePlace))
                days.Add(JsonSerializer.Deserialize<DailyForecastClass>(File.ReadAllText(e)));

            Console.WriteLine("\n\nCollected " + days.Count + " weather reports!");

            double? maxTemp = days.Max(x => x.Temperature.Maximum.Value);

            double? deltaTemp = 0;
            days.ForEach((e) => { deltaTemp += (e.Temperature.Maximum.Value + e.Temperature.Minimum.Value) / 2; });
            deltaTemp /= days.Count;

            IEnumerable<DailyForecastClass> tempQuerry =
                from day in days
                where day.Temperature.Maximum.Value > 30.0
                select day;

            Console.WriteLine($"Max temperature - {maxTemp}\nDelta Temperature - {deltaTemp}\n" +
                $"Temperature more than 30.0 C  in {tempQuerry.ToList().Count} days");

            foreach (var e in days)
            {
                Console.WriteLine("\n\n===================================");

                Console.WriteLine("DATE : {0}\n Max temp : {1} \nMin temp : {2}",
                    e.Date.ToShortDateString() + ", " + e.Date.DayOfWeek,
                    e.Temperature.Maximum.Value,
                    e.Temperature.Minimum.Value);

                if (e.Day.HasPrecipitation) Console.WriteLine("\nHas Day Precipitation : " + e.Day.PrecipitationIntensity + "" + e.Day.PrecipitationType);
                if (e.Night.HasPrecipitation) Console.WriteLine("\nHas Night Precipitation : " + e.Night.PrecipitationIntensity + "" + e.Night.PrecipitationType);

                Console.WriteLine("===================================");
            }
        }

        /// <summary>
        /// App configuration class
        /// </summary>
        private struct Configuration
        {
            public string ApiKey { get; set; }
            public int LocationKey { get; set; }
            public int DataCollectionTimeFrequencyHours { get; set; }
            public int DataCollectionTimeFrequencyMinutes { get; set; }
            public string DownloadedDataSavePlace { get; set; }
        }
    }
}
