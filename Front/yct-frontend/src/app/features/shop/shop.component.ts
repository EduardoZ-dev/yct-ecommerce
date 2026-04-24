import { Component, OnInit, OnDestroy, signal, computed, effect, viewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { ProductDto, CategoryDto, ResponseBase } from '../../core/models';
import { CartService } from '../../core/services/cart.service';
import { environment } from '../../../environments/environment';
import { LocationsMapComponent } from '../locations-map/locations-map.component';

interface Testimonial {
  name: string;
  location: string;
  initials: string;
  rating: number;
  date: string;
  product: string;
  comment: string;
  avatarColor: string;
}

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [CommonModule, RouterModule, LocationsMapComponent],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent implements OnInit, OnDestroy {
  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  selectedCategory = signal<number | null>(null);
  loading = signal(true);
  addedProductId = signal<number | null>(null);
  toastMessage = signal('');

  // Paginación "Ver más"
  private readonly pageSize = 8;
  displayLimit = signal(this.pageSize);

  // Auto-scroll del Hot
  isHotPaused = signal(false);
  private hotScrollOffset = 0;
  private hotRafId: number | null = null;

  hotTrackRef = viewChild<ElementRef<HTMLDivElement>>('hotTrack');

  skeletons = Array(8).fill(0);

  // Testimonios (datos estáticos por ahora — conectar a backend cuando exista la tabla)
  testimonials: Testimonial[] = [
    {
      name: 'María Fernanda López',
      location: 'Valledupar, Cesar',
      initials: 'MF',
      rating: 5,
      date: 'Hace 2 días',
      product: 'Quesillo Tajado',
      comment: 'El quesillo tajado es una maravilla, sabor auténtico y la textura perfecta para las arepas de la mañana. Llegó fresco y muy bien empacado.',
      avatarColor: '#FF9F1C'
    },
    {
      name: 'Carlos Andrés Ramírez',
      location: 'Santa Marta, Magdalena',
      initials: 'CA',
      rating: 5,
      date: 'Hace 5 días',
      product: 'Quesillo Tipo Cheddar',
      comment: 'Para mi negocio de sánduches el cheddar de YCT es ideal: funde muy bien y el empaque al vacío me permite mantener el inventario sin perder calidad.',
      avatarColor: '#5FB878'
    },
    {
      name: 'Laura Gómez',
      location: 'Riohacha, La Guajira',
      initials: 'LG',
      rating: 5,
      date: 'Hace 1 semana',
      product: 'Suero Costeño',
      comment: 'El suero costeño tiene el punto justo de acidez, como el casero. Mi familia ya no quiere otro, siempre lo pedimos con la bandeja del almuerzo.',
      avatarColor: '#4A9B6F'
    },
    {
      name: 'Juan Pablo Herrera',
      location: 'San Juan del Cesar, Cesar',
      initials: 'JP',
      rating: 5,
      date: 'Hace 2 semanas',
      product: 'Quesillo Tajado',
      comment: 'Calidad de finca con entrega puntual. El equipo atiende rápido por WhatsApp y los precios son justos para lo fresco que llega todo.',
      avatarColor: '#FF9F1C'
    },
    {
      name: 'Sandra Milena Torres',
      location: 'Ciénaga, Magdalena',
      initials: 'SM',
      rating: 5,
      date: 'Hace 3 semanas',
      product: 'Quesillo Tipo Cheddar',
      comment: 'Llevo varios meses pidiéndoles y la calidad es constante. Las 20 tajadas individuales me rinden perfecto para el negocio.',
      avatarColor: '#5FB878'
    },
    {
      name: 'Diego Martínez',
      location: 'Maicao, La Guajira',
      initials: 'DM',
      rating: 5,
      date: 'Hace 1 mes',
      product: 'Suero Costeño',
      comment: 'Tengo un restaurante y el suero costeño se ha vuelto el favorito de los clientes. Sabor auténtico, rinde y nunca han fallado con la entrega.',
      avatarColor: '#4A9B6F'
    }
  ];

  trustBadges = [
    { icon: '🛡️', title: 'Pago Seguro', desc: 'Transacciones 100% protegidas', link: '/payment-info' },
    { icon: '🚚', title: 'Envío en 24h', desc: 'Llega fresco a tu puerta', link: '/shipping-info' },
    { icon: '☀️', title: 'Smart Solar Quality', desc: 'Producción con energía solar', link: '/about' },
    { icon: '💬', title: 'Soporte 7 días', desc: 'Estamos para ayudarte', link: '/contact' }
  ];

  // Marcas derivadas de Distribuciones YCT (al hacer click se navega al catálogo filtrado)
  brands = [
    {
      slug: 'mama-sara',
      name: 'Mamá Sara',
      tagline: 'Quesillo tajado, listo para servir',
      image: 'assets/brands/mama-sara.png',
      bg: '#fff8e1'
    },
    {
      slug: 'lacteos-ideal',
      name: 'Lácteos Ideal',
      tagline: 'Quesillo entero, tradición artesanal',
      image: 'assets/brands/lacteos-ideal.png',
      bg: '#e8f5e9'
    }
  ];

  averageRating = computed(() => {
    const total = this.testimonials.reduce((sum, t) => sum + t.rating, 0);
    return (total / this.testimonials.length).toFixed(1);
  });

  totalReviews = computed(() => this.testimonials.length);

  filteredProducts = computed(() => {
    const catId = this.selectedCategory();
    const prods = this.products();
    const filtered = catId ? prods.filter(p => p.categoryId === catId) : prods;
    // Marca propia YCT primero para destacarla en "Nuestros productos".
    return [...filtered].sort((a, b) => {
      const aYct = a.brand === 'YCT' ? 0 : 1;
      const bYct = b.brand === 'YCT' ? 0 : 1;
      return aYct - bYct;
    });
  });

  // Productos "Hot" / más vendidos: marca propia YCT primero, luego resto por id desc.
  hotProducts = computed(() => {
    const all = this.products();
    const yct = all.filter(p => p.brand === 'YCT');
    const rest = all.filter(p => p.brand !== 'YCT');
    return [...yct, ...rest].slice(0, 6);
  });

  // Lista duplicada para el bucle infinito del carrusel hot
  loopedHotProducts = computed(() => {
    const hot = this.hotProducts();
    return [...hot, ...hot];
  });

  // Productos visibles en el grid (limitados por "Ver más")
  displayedProducts = computed(() =>
    this.filteredProducts().slice(0, this.displayLimit())
  );

  hasMoreProducts = computed(() =>
    this.filteredProducts().length > this.displayLimit()
  );

  remainingCount = computed(() =>
    Math.max(0, this.filteredProducts().length - this.displayLimit())
  );

  constructor(
    private http: HttpClient,
    public cart: CartService
  ) {
    effect(() => {
      const track = this.hotTrackRef()?.nativeElement;
      const hot = this.hotProducts();
      if (track && hot.length > 0) {
        setTimeout(() => this.startHotAutoScroll(), 80);
      }
    });
  }

  ngOnInit(): void {
    this.loadCatalog();
    this.loadCategories();
  }

  ngOnDestroy(): void {
    if (this.hotRafId !== null) cancelAnimationFrame(this.hotRafId);
  }

  loadCatalog(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<ProductDto[]>>(`${environment.apiUrl}/api/Products/catalog`).subscribe({
      next: (res) => {
        this.products.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadCategories(): void {
    this.http.get<ResponseBase<CategoryDto[]>>(`${environment.apiUrl}/api/Categories`).subscribe({
      next: (res) => this.categories.set((res.data ?? []).filter(c => c.isActive))
    });
  }

  filterByCategory(categoryId: number | null): void {
    this.selectedCategory.set(categoryId);
    // Resetea el limite de productos visibles al cambiar filtro
    this.displayLimit.set(this.pageSize);
  }

  onHotMouseEnter(): void {
    this.isHotPaused.set(true);
  }

  onHotMouseLeave(): void {
    this.isHotPaused.set(false);
  }

  private startHotAutoScroll(): void {
    if (this.hotRafId !== null) cancelAnimationFrame(this.hotRafId);

    const animate = () => {
      const track = this.hotTrackRef()?.nativeElement;
      if (!track) {
        this.hotRafId = requestAnimationFrame(animate);
        return;
      }
      if (!this.isHotPaused()) {
        const halfWidth = track.scrollWidth / 2;
        if (halfWidth > 0) {
          this.hotScrollOffset -= 0.9;
          if (this.hotScrollOffset <= -halfWidth) this.hotScrollOffset += halfWidth;
          track.style.transform = `translate3d(${this.hotScrollOffset}px, 0, 0)`;
        }
      }
      this.hotRafId = requestAnimationFrame(animate);
    };
    this.hotRafId = requestAnimationFrame(animate);
  }

  addToCart(product: ProductDto, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();

    // Siempre agregar al carrito — la animación es cosmética
    this.cart.add(product);
    this.addedProductId.set(product.id);
    this.showToast(`${product.name} agregado al carrito`);
    setTimeout(() => this.addedProductId.set(null), 1500);

    // Animación "volar al carrito"
    try {
      this.flyToCart(event);
    } catch {
      // Si la animación falla, no afecta la funcionalidad
    }
  }

  private flyToCart(event: MouseEvent): void {
    const btn = event.currentTarget as HTMLElement;
    if (!btn) return;

    const cartIcon = document.querySelector('.cart-icon');
    if (!cartIcon) return;

    const btnRect = btn.getBoundingClientRect();
    const cartRect = cartIcon.getBoundingClientRect();

    // Crear clon volador
    const flyer = document.createElement('div');
    flyer.className = 'fly-to-cart';
    flyer.textContent = '\u{1F9C0}'; // queso emoji
    flyer.style.cssText = `
      position: fixed;
      z-index: 9999;
      top: ${btnRect.top + btnRect.height / 2 - 16}px;
      left: ${btnRect.left + btnRect.width / 2 - 16}px;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 20px;
      pointer-events: none;
      will-change: transform, opacity;
    `;

    document.body.appendChild(flyer);

    // Calcular deltas
    const dx = (cartRect.left + cartRect.width / 2) - (btnRect.left + btnRect.width / 2);
    const dy = (cartRect.top + cartRect.height / 2) - (btnRect.top + btnRect.height / 2);

    // Animar con Web Animations API
    const animation = flyer.animate([
      { transform: 'translate(0, 0) scale(1)', opacity: 1 },
      { transform: `translate(${dx * 0.4}px, ${dy * 0.3 - 40}px) scale(0.7)`, opacity: 0.9, offset: 0.4 },
      { transform: `translate(${dx}px, ${dy}px) scale(0.2)`, opacity: 0.2 }
    ], {
      duration: 700,
      easing: 'cubic-bezier(0.25, 0.1, 0.25, 1)',
      fill: 'forwards'
    });

    animation.onfinish = () => flyer.remove();
  }

  private showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(''), 3000);
  }
}
