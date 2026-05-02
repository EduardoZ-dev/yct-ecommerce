import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CartService } from '../../core/services/cart.service';
import { WhatsappService } from '../../core/services/whatsapp.service';
import { OrderDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent {
  private http = inject(HttpClient);
  private wa = inject(WhatsappService);

  fullName = '';
  shippingAddress = '';
  phone = '';
  notes = '';
  paymentMethod: 'OnDelivery' | 'Transfer' | 'Cash' = 'OnDelivery';

  loading = signal(false);
  orderSent = signal(false);
  errorMsg = signal('');
  createdOrder = signal<OrderDto | null>(null);

  constructor(public cart: CartService) {}

  private formatMoney(value: number): string {
    return '$' + value.toLocaleString('es-CO');
  }

  private validate(): boolean {
    if (this.cart.items().length === 0) { this.errorMsg.set('Tu carrito está vacío'); return false; }
    if (!this.fullName.trim()) { this.errorMsg.set('Ingresa tu nombre'); return false; }
    if (!this.phone.trim()) { this.errorMsg.set('Ingresa tu teléfono'); return false; }
    if (!this.shippingAddress.trim()) { this.errorMsg.set('Ingresa tu dirección'); return false; }
    return true;
  }

  /** Crea el pedido en la base de datos. */
  placeOrder(alsoSendWhatsApp = false): void {
    this.errorMsg.set('');
    if (!this.validate()) return;

    this.loading.set(true);

    const payload = {
      fullName: this.fullName.trim(),
      phone: this.phone.trim(),
      shippingAddress: this.shippingAddress.trim(),
      notes: this.notes.trim() || null,
      paymentMethod: this.paymentMethod,
      items: this.cart.items().map(i => ({
        productId: i.product.id,
        quantity: i.quantity
      }))
    };

    this.http.post<ResponseBase<OrderDto>>(
      `${environment.apiUrl}/api/Orders/guest`, payload
    ).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.createdOrder.set(res.data);
          this.cart.clear();
          this.orderSent.set(true);
          if (alsoSendWhatsApp) this.openWhatsApp(res.data.orderNumber);
        } else {
          this.errorMsg.set(res.message ?? 'No se pudo registrar el pedido');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message ?? 'Error al enviar el pedido. Intenta de nuevo.');
      }
    });
  }

  /** Abre WhatsApp con resumen del pedido (no crea orden, solo mensaje). */
  sendWhatsAppOnly(): void {
    if (!this.validate()) return;

    const items = this.cart.items()
      .map(i => `• ${i.quantity}× ${i.product.name} — ${this.formatMoney(i.product.price * i.quantity)}`)
      .join('\n');

    const total = this.cart.grandTotal();
    const shipping = this.cart.shippingCost();
    const shippingText = shipping === 0 ? 'Gratis' : this.formatMoney(shipping);

    const message = [
      '*Pedido Distribuciones YCT*',
      '',
      '*Productos:*',
      items,
      '',
      `Subtotal: ${this.formatMoney(this.cart.subtotal())}`,
      `Envío: ${shippingText}`,
      `*Total: ${this.formatMoney(total)}*`,
      '',
      '*Datos del cliente:*',
      `Nombre: ${this.fullName}`,
      `Teléfono: ${this.phone}`,
      `Dirección: ${this.shippingAddress}`,
      this.notes ? `Notas: ${this.notes}` : ''
    ].filter(Boolean).join('\n');

    this.wa.openBusiness(message);
  }

  /** Abre WhatsApp para confirmar al negocio el pedido recién creado. */
  sendOrderConfirmation(): void {
    const order = this.createdOrder();
    if (!order) return;
    this.wa.openBusiness(this.wa.buildCustomerConfirmation(order));
  }

  private openWhatsApp(orderNumber: string): void {
    const message = `Hola, acabo de hacer un pedido en la web. Mi número de orden es *${orderNumber}*. ¿Me confirman por favor?`;
    this.wa.openBusiness(message);
  }
}
