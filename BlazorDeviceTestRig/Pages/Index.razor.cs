using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using BlazorDeviceTestRig.Geolocation;
using Darnton.Blazor.DeviceInterop.Geolocation;
using Darnton.Blazor.Leaflet.LeafletMap;
using Microsoft.AspNetCore.Components;

namespace BlazorDeviceTestRig.Pages;

public partial class Index : ComponentBase, IDisposable
{
    protected Marker CurrentPositionMarker;

    protected Map PositionMap;
    protected TileLayer PositionTileLayer;

    protected Map WatchMap;
    protected List<Marker> WatchMarkers;
    protected Polyline WatchPath;
    protected TileLayer WatchTileLayer;

    public Index()
    {
        PositionMap = new Map("geolocationPointMap", new MapOptions //Centred on New Zealand
        {
            Center = new LatLng(28, -81),
            Zoom = 4
        });
        PositionTileLayer = new TileLayer(
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            new TileLayerOptions
            {
                Attribution = @"Map data &copy; <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors, " +
                              @"<a href=""https://creativecommons.org/licenses/by-sa/2.0/"">CC-BY-SA</a>"
            }
        );
        WatchMap = new Map("geolocationWatchMap", new MapOptions //Centred on New Zealand
        {
            Center = new LatLng(-42, 175),
            Zoom = 4
        });
        WatchTileLayer = new TileLayer(
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            new TileLayerOptions
            {
                Attribution = @"Map data &copy; <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors, " +
                              @"<a href=""https://creativecommons.org/licenses/by-sa/2.0/"">CC-BY-SA</a>"
            }
        );
    }

    protected List<BlazorDeviceTestRig.LatLngHistory> LastWatchPositionResultArr { get; set; } = [];
    [Inject] public IGeolocationService GeolocationService { get; set; }

    protected GeolocationResult CurrentPositionResult { get; set; }
    protected string CurrentLatitude => CurrentPositionResult?.Position?.Coords?.Latitude.ToString();
    protected string CurrentLongitude => CurrentPositionResult?.Position?.Coords?.Longitude.ToString();
    protected bool ShowCurrentPositionError => CurrentPositionResult?.Error != null;

    private bool isWatching => WatchHandlerId.HasValue;
    protected long? WatchHandlerId { get; set; }
    protected GeolocationResult LastWatchPositionResult { get; set; }

    protected string LastWatchLatitude => LastWatchPositionResult?.Position?.Coords?.Latitude.ToString();
    protected string LastWatchLongitude => LastWatchPositionResult?.Position?.Coords?.Longitude.ToString();
    protected string LastWatchTimestamp => LastWatchPositionResult?.Position?.DateTimeOffset.ToString();
    protected string ToggleWatchCommand => isWatching ? "Stop watching" : "Start watching";

    public async void Dispose()
    {
        if (isWatching)
        {
            await StopWatching();
        }
    }

    public async void ShowCurrentPosition()
    {
        if (CurrentPositionMarker != null)
        {
            await CurrentPositionMarker.Remove();
        }

        CurrentPositionResult = await GeolocationService.GetCurrentPosition();
        if (CurrentPositionResult.IsSuccess)
        {
            CurrentPositionMarker = new Marker(
                CurrentPositionResult.Position.ToLeafletLatLng(), null
            );
            await CurrentPositionMarker.AddTo(PositionMap);
        }

        StateHasChanged();
    }

    public async void TogglePositionWatch()
    {
        if (isWatching)
        {
            await StopWatching();
            WatchHandlerId = null;
            foreach (var marker in WatchMarkers)
            {
                await marker.Remove();
            }

            WatchMarkers.Clear();
            await WatchPath.Remove();
            WatchPath = null;
        }
        else
        {
            GeolocationService.WatchPositionReceived += HandleWatchPositionReceived;
            WatchHandlerId = await GeolocationService.WatchPosition();
        }

        StateHasChanged();
    }

    private async Task StopWatching()
    {
        GeolocationService.WatchPositionReceived -= HandleWatchPositionReceived;
        await GeolocationService.ClearWatch(WatchHandlerId.Value);
    }

    private async void HandleWatchPositionReceived(object sender, GeolocationEventArgs e)
    {

        LastWatchPositionResult = e.GeolocationResult;

        if (LastWatchPositionResult.IsSuccess)
        {
            var latlng = LastWatchPositionResult.Position.ToLeafletLatLng();


            LastWatchPositionResultArr.Add(new LatLngHistory(latlng));
            var marker = new Marker(latlng, null);
            if (WatchPath is null)
            {
                WatchMarkers = [marker];
                WatchPath = new Polyline(WatchMarkers.Select(m => m.LatLng), new PolylineOptions());
                await WatchPath.AddTo(WatchMap);
            }
            else
            {
                WatchMarkers.Add(marker);
                await WatchPath.AddLatLng(latlng);
            }

            await marker.AddTo(WatchMap);
        }

        StateHasChanged();
    }
}