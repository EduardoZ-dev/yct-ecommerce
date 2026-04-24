import { Injectable, signal, computed } from '@angular/core';
import { ProductDto } from '../models';

export interface CartItem {
  product: ProductDto;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  readonly FREE_SHIPPING_THRESHOLD = 80000;
  readonly SHIPPING_COST = 10000;

  private _items = signal<CartItem[]>([]);

  /** Se incrementa cada vez que se agrega un producto — los componentes lo observan para animaciones */
  readonly itemAdded = signal(0);

  items = this._items.asReadonly();
  itemCount = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));
  subtotal = computed(() => this._items().reduce((sum, i) => sum + (i.product.price * i.quantity), 0));
  total = this.subtotal; // alias retrocompatible

  shippingCost = computed(() => {
    const sub = this.subtotal();
    if (sub === 0) return 0;
    return sub >= this.FREE_SHIPPING_THRESHOLD ? 0 : this.SHIPPING_COST;
  });

  grandTotal = computed(() => this.subtotal() + this.shippingCost());

  isFreeShipping = computed(() => this.subtotal() >= this.FREE_SHIPPING_THRESHOLD);

  amountForFreeShipping = computed(() => {
    const missing = this.FREE_SHIPPING_THRESHOLD - this.subtotal();
    return missing > 0 ? missing : 0;
  });

  freeShippingProgress = computed(() => {
    const sub = this.subtotal();
    const pct = (sub / this.FREE_SHIPPING_THRESHOLD) * 100;
    return Math.min(100, Math.max(0, pct));
  });

  add(product: ProductDto, quantity: number = 1): void {
    const current = this._items();
    const existing = current.find(i => i.product.id === product.id);

    if (existing) {
      this._items.set(current.map(i =>
        i.product.id === product.id
          ? { ...i, quantity: Math.min(i.quantity + quantity, product.stock) }
          : i
      ));
    } else {
      this._items.set([...current, { product, quantity }]);
    }

    // Notificar animación
    this.itemAdded.update(v => v + 1);
  }

  remove(productId: number): void {
    this._items.set(this._items().filter(i => i.product.id !== productId));
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.remove(productId);
      return;
    }
    this._items.set(this._items().map(i =>
      i.product.id === productId
        ? { ...i, quantity: Math.min(quantity, i.product.stock) }
        : i
    ));
  }

  clear(): void {
    this._items.set([]);
  }
}
