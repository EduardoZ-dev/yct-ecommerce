import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ProductDto, ResponseBase } from '../../core/models';
import { CartService } from '../../core/services/cart.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss'
})
export class ProductDetailComponent implements OnInit {
  product = signal<ProductDto | null>(null);
  loading = signal(true);
  quantity = signal(1);
  added = signal(false);
  toastMessage = signal('');
  activeTab = signal<'nutrition' | 'ingredients' | 'storage'>('nutrition');

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    public cart: CartService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.http.get<ResponseBase<ProductDto>>(`${environment.apiUrl}/api/Products/${id}`).subscribe({
        next: (res) => {
          this.product.set(res.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
    }
  }

  increment(): void {
    const p = this.product();
    if (p && this.quantity() < p.stock) {
      this.quantity.update(q => q + 1);
    }
  }

  decrement(): void {
    if (this.quantity() > 1) {
      this.quantity.update(q => q - 1);
    }
  }

  addToCart(): void {
    const p = this.product();
    if (!p) return;
    this.cart.add(p, this.quantity());
    this.added.set(true);
    this.toastMessage.set(`${p.name} agregado al carrito`);
    setTimeout(() => {
      this.added.set(false);
      this.toastMessage.set('');
    }, 2500);
  }
}
