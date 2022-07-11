using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Data.Extensions
{
    public static class GeoExtensions
    {
        private const double EarthRadius = 6372795;
        private const double PiRad = Math.PI / 180;

        public static int? CalculateDistance(this string source, string destination)
        {
            var point1 = GetGeoPoint(source);
            var point2 = GetGeoPoint(destination);

            if (point1 == null || point2 == null)
            {
                return null;
            }

            return point1.CalculateDistance(point2);
        }

        /// <summary>
        /// Calculate distance between two geopoints in meters
        /// </summary>
        public static int? CalculateDistance(this GeoPoint point1, GeoPoint point2)
        {
            ConvertToRadians(point1);
            ConvertToRadians(point2);

            var lat1Sin = Math.Sin(point1.Latitude);
            var lat2Sin = Math.Sin(point2.Latitude);

            var lat1Cos = Math.Cos(point1.Latitude);
            var lat2Cos = Math.Cos(point2.Latitude);

            var longDelta = point2.Longitude - point1.Longitude;

            var deltaSin = Math.Sin(longDelta);
            var deltaCos = Math.Cos(longDelta);

            var x = lat1Sin * lat2Sin + lat1Cos * lat2Cos * deltaCos;
            var y = Math.Sqrt(Math.Pow(lat2Cos * deltaSin, 2) + Math.Pow(lat1Cos * lat2Sin - lat1Sin * lat2Cos * deltaCos, 2));

            var radialDistance = Math.Atan2(y, x);
            var distance = radialDistance * EarthRadius;

            return Convert.ToInt32(Math.Round(distance));
        }

        private static void ConvertToRadians(GeoPoint point)
        {
            point.Latitude *= PiRad;
            point.Longitude *= PiRad;
        }

        private static GeoPoint GetGeoPoint(string geoLocation)
        {
            if (string.IsNullOrEmpty(geoLocation))
            {
                return null;
            }

            var coordinates = geoLocation.Split(',')
                .Select(x =>
                {
                    if (double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    {
                        double? result = parsed;
                        return result;
                    }

                    return null;
                }).ToArray();

            if (coordinates.Length != 2 || coordinates.Any(x => x == null))
            {
                return null;
            }

            return new GeoPoint(coordinates[0].Value, coordinates[1].Value);
        }
    }
}
