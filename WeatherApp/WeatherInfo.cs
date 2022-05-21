using System;
using System.Collections.Generic;

namespace WeatherApp
{
    public class WeatherInfo
    {
        public HeadlineClass Headline { get; set; }
        public List<DailyForecastClass> DailyForecasts { get; set; }
    }

    public class HeadlineClass
    {
        public string EffectiveDate { get; set; }
        public Nullable<long> EffectiveEpochTime { get; set; }
        public Nullable<int> Severity { get; set; }
        public string Text { get; set; }
        public string Category { get; set; }
        public string EndDate { get; set; }
        public Nullable<long> EndEpochDate { get; set; }
        public string MobileLink { get; set; }
        public string Link { get; set; }
    }

    public class DailyForecastClass
    {
        public DateTime Date { get; set; }
        public Nullable<long> EpochDate { get; set; }
        public TemperatureClass Temperature { get; set; }
        public DayForecastClass Day { get; set; }
        public DayForecastClass Night { get; set; }
        public string[] Sources { get; set; }
        public string MobileLink { get; set; }
        public string Link { get; set; }
    }

    public class TemperatureClass
    {
        public TemperatureDataClass Minimum { get; set; }
        public TemperatureDataClass Maximum { get; set; }
    }

    public class TemperatureDataClass
    {
        public Nullable<double> Value { get; set; }
        public string Unit { get; set; }
        public int UnitType { get; set; }
    }

    public class DayForecastClass
    {
        public Nullable<int> Icon { get; set; }
        public string IconPhase { get; set; }
        public bool HasPrecipitation { get; set; }
        public string PrecipitationType { get; set; }
        public string PrecipitationIntensity { get; set; }
    }
}