import { useEffect, useMemo } from "react";
import { MapContainer, Marker, Popup, TileLayer, useMap, useMapEvents } from "react-leaflet";
import L from "leaflet";

const DEFAULT_CENTER = [44.0165, 21.0059];
const DEFAULT_ZOOM = 7;

const apiaryIcon = L.divIcon({
  className: "map-marker map-marker-apiary",
  html: "<span>PC</span>",
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -30],
});

const parcelIcon = L.divIcon({
  className: "map-marker map-marker-parcel",
  html: "<span>PA</span>",
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -30],
});

const selectedIcon = L.divIcon({
  className: "map-marker map-marker-selected",
  html: "<span>+</span>",
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -30],
});

function isValidCoordinate(latitude, longitude) {
  return Number.isFinite(latitude) && Number.isFinite(longitude);
}

function FitMapBounds({ points }) {
  const map = useMap();

  useEffect(() => {
    if (points.length === 0) return;

    if (points.length === 1) {
      map.setView(points[0], 12);
      return;
    }

    map.fitBounds(points, { padding: [32, 32], maxZoom: 13 });
  }, [map, points]);

  return null;
}

function LocationPicker({ onSelect }) {
  useMapEvents({
    click(event) {
      onSelect({
        latitude: Number(event.latlng.lat.toFixed(6)),
        longitude: Number(event.latlng.lng.toFixed(6)),
      });
    },
  });

  return null;
}

function cropFor(parcel) {
  return parcel.currentCrop || parcel.crop;
}

export default function SmartMap({
  apiaries = [],
  parcels = [],
  selectedLocation = null,
  onLocationSelect,
  height = 420,
}) {
  const markers = useMemo(() => {
    const apiaryMarkers = apiaries
      .map((apiary) => ({
        type: "apiary",
        item: apiary,
        position: [Number(apiary.latitude), Number(apiary.longitude)],
      }))
      .filter((marker) => isValidCoordinate(marker.position[0], marker.position[1]));

    const parcelMarkers = parcels
      .map((parcel) => ({
        type: "parcel",
        item: parcel,
        position: [Number(parcel.latitude), Number(parcel.longitude)],
      }))
      .filter((marker) => isValidCoordinate(marker.position[0], marker.position[1]));

    return [...apiaryMarkers, ...parcelMarkers];
  }, [apiaries, parcels]);

  const selectedPosition = selectedLocation
    ? [Number(selectedLocation.latitude), Number(selectedLocation.longitude)]
    : null;

  const hasSelectedPosition = selectedPosition && isValidCoordinate(selectedPosition[0], selectedPosition[1]);
  const boundsPoints = hasSelectedPosition
    ? [...markers.map((marker) => marker.position), selectedPosition]
    : markers.map((marker) => marker.position);

  return (
    <div className="smart-map" style={{ height }}>
      <MapContainer center={DEFAULT_CENTER} zoom={DEFAULT_ZOOM} scrollWheelZoom style={{ height: "100%", width: "100%" }}>
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <FitMapBounds points={boundsPoints} />
        {onLocationSelect && <LocationPicker onSelect={onLocationSelect} />}

        {markers.map((marker) => {
          const isApiary = marker.type === "apiary";
          const crop = !isApiary ? cropFor(marker.item) : null;

          return (
            <Marker
              key={`${marker.type}-${marker.item.id}`}
              position={marker.position}
              icon={isApiary ? apiaryIcon : parcelIcon}
            >
              <Popup>
                <strong>{marker.item.name}</strong>
                <br />
                {isApiary ? (
                  <>
                    Pcelinjak
                    <br />
                    Kosnice: {marker.item.hiveCount ?? 0}
                    {marker.item.description && (
                      <>
                        <br />
                        {marker.item.description}
                      </>
                    )}
                  </>
                ) : (
                  <>
                    Parcela
                    {crop && (
                      <>
                        <br />
                        Kultura: {crop.cropType}
                        {crop.bloomingPeriod && (
                          <>
                            <br />
                            Cvetanje: {crop.bloomingPeriod}
                          </>
                        )}
                      </>
                    )}
                    {marker.item.ownerName && (
                      <>
                        <br />
                        Vlasnik: {marker.item.ownerName}
                      </>
                    )}
                    {marker.item.ownerPhone && (
                      <>
                        <br />
                        Telefon: {marker.item.ownerPhone}
                      </>
                    )}
                  </>
                )}
                <br />
                {marker.position[0]}, {marker.position[1]}
              </Popup>
            </Marker>
          );
        })}

        {hasSelectedPosition && (
          <Marker position={selectedPosition} icon={selectedIcon}>
            <Popup>Izabrana lokacija</Popup>
          </Marker>
        )}
      </MapContainer>
    </div>
  );
}
