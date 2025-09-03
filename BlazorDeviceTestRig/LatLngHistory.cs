using System;
using Darnton.Blazor.DeviceInterop.Geolocation;

namespace BlazorDeviceTestRig;

public class LatLngHistory(Darnton.Blazor.Leaflet.LeafletMap.LatLng latLng)
{
    public double Latitude { get; } = latLng.Lat;
    public double Longitude { get; } = latLng.Lng;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string TimestampString => $"{Timestamp:yyyy/MM/dd HH:mm:ss}";
}