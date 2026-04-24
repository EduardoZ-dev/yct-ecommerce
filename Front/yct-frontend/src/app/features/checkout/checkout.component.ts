import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent {
  private readonly whatsappNumber = '573104525896';

  fullName = '';
  shippingAddress = '';
  phone = '';
  notes = '';
  loading = signal(false);
  orderSent = signal(false);

  constructor(public cart: CartService) {}

  private formatMoney(value: number): string {
    return '$' + value.toLocaleString('es-CO');
  }

  placeOrder(): void {
    if (this.cart.items().length === 0) return;
    if (!this.fullName.trim() || !this.shippingAddress.trim() || !this.phone.trim()) return;

    this.loading.set(true);

    const items = this.cart.items()
      .map(i => `• ${i.quantity}× ${i.product.name} — ${this.formatMoney(i.product.price * i.quantity)}`)
      .join('\n');

    const subtotal = this.cart.subtotal();
    const shipping = this.cart.shippingCost();
    const total = this.cart.grandTotal();

    const shippingText = shipping === 0 ? 'Gratis' : this.formatMoney(shipping);

    const message = [
      '🧀 *Nuevo pedido - Distribuciones YCT*',
      '',
      '*Productos:*',
      items,
      '',
      `Subtotal: ${this.formatMoney(subtotal)}`,
      `Envío: ${shippingText}`,
      `*Total: ${this.formatMoney(total)}*`,
      '',
      '*Datos del cliente:*',
      `👤 ${this.fullName}`,
      `📞 ${this.phone}`,
      `📍 ${this.shippingAddress}`,
      this.notes ? `📝 Notas: ${this.notes}` : ''
    ].filter(Boolean).join('\n');

    const url = `https://wa.me/${this.whatsappNumber}?text=${encodeURIComponent(message)}`;
    window.open(url, '_blank', 'noopener,noreferrer');

    this.cart.clear();
    this.orderSent.set(true);
    this.loading.set(false);
  }
}
