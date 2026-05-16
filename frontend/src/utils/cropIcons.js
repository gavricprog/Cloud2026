const CROP_EMOJI = {
  Suncokret: "🌻",
  "Uljana repica": "🌼",
  Lavanda: "💜",
  Bagrem: "🌳",
  Lipa: "🌿",
  Detelina: "🍀",
  Drugo: "🌾",
};

export function cropEmoji(cropType) {
  if (!cropType) return "🌾";
  return CROP_EMOJI[cropType] ?? "🌾";
}

export function cropMarkerHtml(cropType, selected = false) {
  const emoji = cropEmoji(cropType);
  const extra = selected ? " map-marker-crop-selected" : "";
  return `<span class="map-marker-crop${extra}">${emoji}</span>`;
}
