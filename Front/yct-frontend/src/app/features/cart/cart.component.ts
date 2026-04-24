import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CartService } from '../../core/services/cart.service';
import { ProductDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit {
  private http = inject(HttpClient);
  cart = inject(CartService);

  suggested = signal<ProductDto[]>([]);
  justAddedId = signal<number | null>(null);

  steps = [
    { key: 'cart', label: 'Carrito' },
    { key: 'shipping', label: 'Envío' },
    { key: 'payment', label: 'Pago' },
    { key: 'done', label: 'Confirmación' }
  ];

  ngOnInit(): void {
    if (this.cart.items().length === 0) {
      this.loadSuggested();
    }
  }

  private loadSuggested(): void {
    this.http.get<ResponseBase<ProductDto[]>>(`${environment.apiUrl}/api/Products/catalog`).subscribe({
      next: (res) => {
        // Mostrar los 3 productos más recientes (los de mayor Id) — presentaciones grandes de cada marca.
        const list = res.data ?? [];
        const top = [...list].sort((a, b) => b.id - a.id).slice(0, 3);
        this.suggested.set(top);
      }
    });
  }

  increment(productId: number): void {
    const item = this.cart.items().find(i => i.product.id === productId);
    if (item) this.cart.updateQuantity(productId, item.quantity + 1);
  }

  decrement(productId: number): void {
    const item = this.cart.items().find(i => i.product.id === productId);
    if (item) this.cart.updateQuantity(productId, item.quantity - 1);
  }

  remove(productId: number): void {
    this.cart.remove(productId);
  }

  quickAdd(product: ProductDto): void {
    this.cart.add(product, 1);
    this.justAddedId.set(product.id);
    setTimeout(() => this.justAddedId.set(null), 1200);
  }
}
