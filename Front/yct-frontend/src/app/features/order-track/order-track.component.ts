import { AfterViewChecked, Component, ElementRef, OnDestroy, OnInit, QueryList, ViewChildren, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import maplibregl, { Map as MapLibreMap } from 'maplibre-gl';
import { ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

interface TrackedOrder {
  orderNumber: string;
  consecutive: number;
  orderDate: string;
  total: number;
  status: string;
  paymentMethod: string;
  paymentStatus: string;
  validatedAt?: string | null;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  distributorName?: string | null;
  distributorVehicle?: string | null;
  trackingNumber?: string | null;
  customerName: string;
  shippingAddress: string;
  shippingCity?: string;
  shippingLat?: number;
  shippingLng?: number;
  items: {
    productName: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
  }[];
}

// Centro de distribución YCT (San Diego, Cesar)
const WAREHOUSE = { lat: 10.335784663950779, lng: -73.17746303316659 };

// Aproximaciones de ciudades comunes en la región
const CITY_COORDS: Record<string, { lat: number; lng: number }> = {
  'valledupar': { lat: 10.466010905009135, lng: -73.24769505748026 },
  'san diego': { lat: 10.335784663950779, lng: -73.17746303316659 },
  'codazzi': { lat: 10.034857, lng: -73.236076 },
  'agustin codazzi': { lat: 10.034857, lng: -73.236076 },
  'la paz': { lat: 10.388214, lng: -73.169731 },
  'becerril': { lat: 9.704321, lng: -73.275108 },
  'santa marta': { lat: 11.247199, lng: -74.199300 },
  'riohacha': { lat: 11.544444, lng: -72.907222 },
  'maicao': { lat: 11.382500, lng: -72.241944 },
  'aguachica': { lat: 8.310744, lng: -73.617821 }
};

@Component({
  selector: 'app-order-track',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './order-track.component.html',
  styleUrl: './order-track.component.scss'
})
export class OrderTrackComponent implements OnInit, OnDestroy, AfterViewChecked {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);

  @ViewChildren('mapContainer') mapContainers!: QueryList<ElementRef<HTMLDivElement>>;

  searchTerm = '';
  loading = signal(false);
  results = signal<TrackedOrder[]>([]);
  searched = signal(false);
  errorMsg = signal('');

  private maps: MapLibreMap[] = [];
  private mapsRendered = false;

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap.get('q');
    if (q) {
      this.searchTerm = q;
      this.search();
    }
  }

  ngAfterViewChecked(): void {
    // Renderiza mapas tras el render del *ngFor
    if (!this.mapsRendered && this.results().length > 0 && this.mapContainers?.length > 0) {
      this.mapsRendered = true;
      setTimeout(() => this.renderMaps(), 0);
    }
  }

  ngOnDestroy(): void {
    this.maps.forEach(m => m.remove());
    this.maps = [];
  }

  private renderMaps(): void {
    this.maps.forEach(m => m.remove());
    this.maps = [];

    this.results().forEach((order, idx) => {
      const container = this.mapContainers.toArray()[idx]?.nativeElement;
      if (!container) return;

      const dest = this.resolveDestination(order);

      const bounds = new maplibregl.LngLatBounds();
      bounds.extend([WAREHOUSE.lng, WAREHOUSE.lat]);
      bounds.extend([dest.lng, dest.lat]);

      const map = new maplibregl.Map({
        container,
        style: 'https://tiles.openfreemap.org/styles/liberty',
        bounds,
        fitBoundsOptions: { padding: 60 },
        attributionControl: false
      });

      map.addControl(new maplibregl.NavigationControl({ showCompass: false }), 'top-right');
      map.addControl(new maplibregl.AttributionControl({ compact: true }), 'bottom-right');

      map.on('load', () => {
        // Línea de ruta entre origen y destino
        map.addSource('route', {
          type: 'geojson',
          data: {
            type: 'Feature',
            properties: {},
            geometry: {
              type: 'LineString',
              coordinates: [
                [WAREHOUSE.lng, WAREHOUSE.lat],
                [dest.lng, dest.lat]
              ]
            }
          }
        });

        map.addLayer({
          id: 'route-line',
          type: 'line',
          source: 'route',
          layout: { 'line-cap': 'round', 'line-join': 'round' },
          paint: {
            'line-color': '#7AB648',
            'line-width': 4,
            'line-dasharray': [2, 2]
          }
        });

        // Marker origen (warehouse)
        const origin = document.createElement('div');
        origin.className = 'track-marker origin';
        origin.innerHTML = '<span></span>';
        new maplibregl.Marker({ element: origin })
          .setLngLat([WAREHOUSE.lng, WAREHOUSE.lat])
          .setPopup(new maplibregl.Popup({ offset: 18 }).setHTML('<strong>Centro YCT</strong><br>San Diego, Cesar'))
          .addTo(map);

        // Marker destino
        const target = document.createElement('div');
        target.className = `track-marker destination status-${order.status.toLowerCase()}`;
        target.innerHTML = '<span></span>';
        const destLabel = order.shippingCity
          ? `<strong>Tu pedido</strong><br>${order.shippingCity}`
          : '<strong>Zona de entrega</strong>';
        new maplibregl.Marker({ element: target })
          .setLngLat([dest.lng, dest.lat])
          .setPopup(new maplibregl.Popup({ offset: 18 }).setHTML(destLabel))
          .addTo(map);

        // Si está en camino, mostrar un punto que se mueve por la ruta
        if (order.status === 'Shipped') {
          this.animateInTransit(map, [WAREHOUSE.lng, WAREHOUSE.lat], [dest.lng, dest.lat]);
        }
      });

      this.maps.push(map);
    });
  }

  /** Si la orden tiene lat/lng, las usa. Si no, intenta detectar la ciudad del texto. */
  private resolveDestination(order: TrackedOrder): { lat: number; lng: number } {
    if (order.shippingLat && order.shippingLng) {
      return { lat: order.shippingLat, lng: order.shippingLng };
    }
    const text = `${order.shippingAddress} ${order.shippingCity ?? ''}`.toLowerCase();
    for (const [city, coords] of Object.entries(CITY_COORDS)) {
      if (text.includes(city)) return coords;
    }
    // Default: Valledupar (centro de la zona de cobertura)
    return CITY_COORDS['valledupar'];
  }

  private animateInTransit(map: MapLibreMap, origin: [number, number], dest: [number, number]): void {
    const truck = document.createElement('div');
    truck.className = 'track-marker transit';
    const marker = new maplibregl.Marker({ element: truck }).setLngLat(origin).addTo(map);

    let t = 0;
    const tick = () => {
      t = (t + 0.005) % 1;
      const lng = origin[0] + (dest[0] - origin[0]) * t;
      const lat = origin[1] + (dest[1] - origin[1]) * t;
      marker.setLngLat([lng, lat]);
      requestAnimationFrame(tick);
    };
    tick();
  }

  search(): void {
    const term = this.searchTerm.trim();
    if (!term) {
      this.errorMsg.set('Ingresa tu número de pedido, consecutivo o teléfono');
      return;
    }
    this.errorMsg.set('');
    this.loading.set(true);
    this.searched.set(true);
    this.mapsRendered = false; // forzar re-render

    const params = new HttpParams().set('search', term);
    this.http.get<ResponseBase<TrackedOrder[]>>(
      `${environment.apiUrl}/api/Orders/track`, { params }
    ).subscribe({
      next: (res) => {
        this.results.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.results.set([]);
        this.loading.set(false);
        this.errorMsg.set('No se pudo consultar el estado. Intenta de nuevo.');
      }
    });
  }

  statusLabel(status: string): string {
    return {
      'Pending': 'Pendiente de confirmación',
      'Confirmed': 'Confirmado',
      'Shipped': 'En camino',
      'Delivered': 'Entregado',
      'Cancelled': 'Cancelado'
    }[status] ?? status;
  }

  statusClass(status: string): string {
    return {
      'Pending': 'pending',
      'Confirmed': 'confirmed',
      'Shipped': 'shipped',
      'Delivered': 'delivered',
      'Cancelled': 'cancelled'
    }[status] ?? 'pending';
  }

  statusStep(status: string): number {
    return { 'Pending': 1, 'Confirmed': 2, 'Shipped': 3, 'Delivered': 4, 'Cancelled': 0 }[status] ?? 0;
  }

  /** Devuelve el índice (0..4) del paso actual basado en timestamps + estado. */
  trackStep(o: TrackedOrder): number {
    if (o.status === 'Cancelled') return -1;
    if (o.deliveredAt) return 4;
    if (o.shippedAt) return 3;
    if (o.status === 'Confirmed') return 2;
    if (o.validatedAt) return 1;
    return 0;
  }

  paymentMethodLabel(m: string): string {
    return ({
      'OnDelivery': 'Pago contra entrega',
      'Transfer': 'Transferencia previa',
      'Cash': 'Efectivo en sede'
    } as Record<string, string>)[m] ?? m;
  }

  paymentStatusLabel(s: string): string {
    return ({ 'Unpaid': 'Pago pendiente', 'Paid': 'Pagado', 'Refunded': 'Reembolsado' } as Record<string, string>)[s] ?? s;
  }
}
