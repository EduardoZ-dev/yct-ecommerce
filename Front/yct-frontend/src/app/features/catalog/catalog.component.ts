import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ProductDto, CategoryDto, ResponseBase } from '../../core/models';
import { CartService } from '../../core/services/cart.service';
import { environment } from '../../../environments/environment';

const BRAND_NAMES: Record<string, string> = {
  'mama-sara': 'Mamá Sara',
  'lacteos-ideal': 'Lácteos Ideal'
};

type SortOption = 'name-asc' | 'name-desc' | 'price-asc' | 'price-desc';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss'
})
export class CatalogComponent implements OnInit {
  private route = inject(ActivatedRoute);

  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  selectedCategory = signal<number | null>(null);
  searchTerm = signal('');
  sortBy = signal<SortOption>('name-asc');
  loading = signal(true);
  addedProductId = signal<number | null>(null);
  toastMessage = signal('');

  // Brand desde query param (?brand=mama-sara)
  brandSlug = toSignal(
    this.route.queryParams.pipe(map(params => params['brand'] as string | undefined)),
    { initialValue: undefined }
  );
  brandName = computed(() => {
    const slug = this.brandSlug();
    return slug ? BRAND_NAMES[slug] : undefined;
  });

  skeletons = Array(12).fill(0);

  filteredProducts = computed(() => {
    const catId = this.selectedCategory();
    const term = this.searchTerm().toLowerCase().trim();
    const sort = this.sortBy();
    const brand = this.brandName();

    let list = this.products();

    if (brand) {
      list = list.filter(p => p.brand === brand);
    }

    if (catId !== null) {
      list = list.filter(p => p.categoryId === catId);
    }

    if (term) {
      list = list.filter(p =>
        p.name.toLowerCase().includes(term) ||
        (p.description?.toLowerCase().includes(term) ?? false)
      );
    }

    list = [...list].sort((a, b) => {
      switch (sort) {
        case 'name-asc': return a.name.localeCompare(b.name);
        case 'name-desc': return b.name.localeCompare(a.name);
        case 'price-asc': return a.price - b.price;
        case 'price-desc': return b.price - a.price;
      }
    });

    return list;
  });

  constructor(
    private http: HttpClient,
    public cart: CartService
  ) {}

  ngOnInit(): void {
    this.loadCatalog();
    this.loadCategories();
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
  }

  clearFilters(): void {
    this.selectedCategory.set(null);
    this.searchTerm.set('');
    this.sortBy.set('name-asc');
  }

  addToCart(product: ProductDto, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();
    this.cart.add(product);
    this.addedProductId.set(product.id);
    this.showToast(`${product.name} agregado al carrito`);
    setTimeout(() => this.addedProductId.set(null), 1500);
  }

  private showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(''), 3000);
  }
}
