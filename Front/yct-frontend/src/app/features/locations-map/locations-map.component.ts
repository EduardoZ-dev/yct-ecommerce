import { AfterViewInit, Component, ElementRef, OnDestroy, signal, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import maplibregl, { LngLatBoundsLike, Map, Marker } from 'maplibre-gl';

interface Sede {
  id: string;
  name: string;
  address: string;
  phone: string;
  hours: string;
  lng: number;
  lat: number;
  color: string;
}

@Component({
  selector: 'app-locations-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './locations-map.component.html',
  styleUrl: './locations-map.component.scss'
})
export class LocationsMapComponent implements AfterViewInit, OnDestroy {
  mapContainer = viewChild<ElementRef<HTMLDivElement>>('mapContainer');

  private map?: Map;
  private markers: Marker[] = [];

  activeSedeId = signal<string>('');

  // Bounding box del departamento del Cesar (SW, NE)
  private readonly cesarBounds: LngLatBoundsLike = [
    [-74.4, 7.4],
    [-72.4, 10.95]
  ];

  // Sedes — agregar más cuando estén confirmadas las direcciones
  sedes: Sede[] = [
    {
      id: 'san-diego',
      name: 'Sede Principal - San Diego',
      address: 'Cl. 5 #5A-36, San Diego, Cesar',
      phone: '+57 310 452 5896',
      hours: 'Lun-Sáb 7:00 AM - 6:00 PM',
      lng: -73.17746303316659,
      lat: 10.335784663950779,
      color: '#5FB878'
    },
    {
      id: 'valledupar',
      name: 'Sede Valledupar - Mercado',
      address: 'Calle 20 No. 14-82, Valledupar, Cesar',
      phone: '+57 310 452 5896',
      hours: 'Lun-Sáb 7:00 AM - 6:00 PM',
      lng: -73.24769505748026,
      lat: 10.466010905009135,
      color: '#FF9F1C'
    }
  ];

  ngAfterViewInit(): void {
    const container = this.mapContainer()?.nativeElement;
    if (!container) return;

    this.map = new maplibregl.Map({
      container,
      style: 'https://tiles.openfreemap.org/styles/liberty',
      bounds: this.cesarBounds,
      fitBoundsOptions: {
        padding: 50,
        pitch: 45,
        bearing: -10
      },
      attributionControl: false
    });

    this.map.addControl(new maplibregl.NavigationControl({ visualizePitch: true }), 'top-right');
    this.map.addControl(new maplibregl.FullscreenControl(), 'top-right');
    this.map.addControl(
      new maplibregl.AttributionControl({ compact: true }),
      'bottom-right'
    );

    this.map.on('load', () => {
      this.add3DBuildings();
      this.addMarkers();
    });
  }

  ngOnDestroy(): void {
    this.markers.forEach(m => m.remove());
    this.map?.remove();
  }

  /** Inserta la capa de edificios 3D extruidos. */
  private add3DBuildings(): void {
    if (!this.map) return;

    const layers = this.map.getStyle().layers ?? [];
    let labelLayerId: string | undefined;
    for (const layer of layers) {
      if (layer.type === 'symbol' && layer.layout?.['text-field']) {
        labelLayerId = layer.id;
        break;
      }
    }

    // La capa de edificios puede variar según el style; intentamos las más comunes
    const sourceLayer = this.map.getLayer('building')
      ? 'building'
      : this.map.getLayer('building-3d') ? 'building-3d' : null;

    if (!sourceLayer) return;

    try {
      this.map.addLayer(
        {
          id: 'yct-3d-buildings',
          source: 'openmaptiles',
          'source-layer': 'building',
          filter: ['!=', ['get', 'hide_3d'], true],
          type: 'fill-extrusion',
          minzoom: 14,
          paint: {
            'fill-extrusion-color': [
              'interpolate', ['linear'], ['get', 'render_height'],
              0, '#e8f5e9',
              50, '#a8d5ba',
              100, '#5FB878'
            ],
            'fill-extrusion-height': [
              'interpolate', ['linear'], ['zoom'],
              14, 0,
              16, ['get', 'render_height']
            ],
            'fill-extrusion-base': ['get', 'render_min_height'],
            'fill-extrusion-opacity': 0.85
          }
        },
        labelLayerId
      );
    } catch {
      // Silenciar si el estilo no soporta la capa
    }
  }

  private addMarkers(): void {
    if (!this.map) return;

    this.sedes.forEach(sede => {
      const el = document.createElement('div');
      el.className = 'sede-marker';
      el.style.background = sede.color;
      el.innerHTML = '<span>🥛</span>';

      const popup = new maplibregl.Popup({
        offset: 30,
        closeButton: false,
        className: 'sede-popup'
      }).setHTML(`
        <strong>${sede.name}</strong>
        <p>${sede.address}</p>
        <p>📞 ${sede.phone}</p>
        <p>🕐 ${sede.hours}</p>
      `);

      const marker = new maplibregl.Marker({ element: el })
        .setLngLat([sede.lng, sede.lat])
        .setPopup(popup)
        .addTo(this.map!);

      el.addEventListener('click', () => this.flyToSede(sede));
      this.markers.push(marker);
    });
  }

  flyToSede(sede: Sede): void {
    if (!this.map) return;
    this.activeSedeId.set(sede.id);
    this.map.flyTo({
      center: [sede.lng, sede.lat],
      zoom: 15.5,
      pitch: 60,
      bearing: -20,
      speed: 1.2,
      essential: true
    });
  }

  resetView(): void {
    if (!this.map) return;
    this.activeSedeId.set('');
    this.map.fitBounds(this.cesarBounds, {
      padding: 50,
      pitch: 45,
      bearing: -10,
      duration: 1500
    });
  }
}
