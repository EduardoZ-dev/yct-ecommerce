import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderDto, DistributorDto } from '../models';

@Injectable({ providedIn: 'root' })
export class WhatsappService {
  /** Normaliza un teléfono colombiano a formato internacional sin + (ej: '3104525896' → '573104525896') */
  normalizePhone(phone: string | null | undefined): string {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    if (!digits) return '';
    if (digits.startsWith('57')) return digits;
    if (digits.length === 10) return `57${digits}`;
    return digits;
  }

  /** Abre WhatsApp Web/app con un mensaje pre-llenado al número indicado. */
  open(phone: string, message: string): void {
    const normalized = this.normalizePhone(phone);
    if (!normalized) {
      alert('Sin teléfono disponible para WhatsApp.');
      return;
    }
    const url = `https://wa.me/${normalized}?text=${encodeURIComponent(message)}`;
    window.open(url, '_blank');
  }

  /** Abre WhatsApp al número del negocio (cliente confirmando pedido). */
  openBusiness(message: string): void {
    this.open(environment.businessWhatsApp, message);
  }

  // ============ Plantillas de mensaje ============

  /** Mensaje del cliente al negocio confirmando que envió el pedido. */
  buildCustomerConfirmation(o: OrderDto): string {
    const lines = [
      `Hola ${environment.businessName} 👋`,
      `Acabo de hacer el pedido *#${o.consecutive}* (${o.orderNumber}).`,
      `Total: $${o.total.toLocaleString('es-CO')}`,
      `Dirección: ${o.shippingAddress ?? '—'}`,
      `Método de pago: ${this.paymentMethodEs(o.paymentMethod)}`,
      ``,
      `Quedo atento(a) a la confirmación. ¡Gracias!`
    ];
    return lines.join('\n');
  }

  /** Mensaje del admin al cliente según la etapa del pedido. */
  buildAdminToCustomer(o: OrderDto): string {
    const greeting = `Hola ${o.userFullName}, te escribimos de ${environment.businessName}.`;

    switch (o.status) {
      case 'Pending':
        return `${greeting}\nRecibimos tu pedido *#${o.consecutive}* por $${o.total.toLocaleString('es-CO')}. Lo estamos revisando y en breve te confirmamos. ¡Gracias!`;
      case 'Confirmed':
        return `${greeting}\nTu pedido *#${o.consecutive}* fue confirmado y está en preparación. Te avisamos cuando salga en camino.`;
      case 'Shipped': {
        const driver = o.distributorName ? ` con *${o.distributorName}*${o.distributorVehicle ? ` (${o.distributorVehicle})` : ''}` : '';
        const tracking = o.trackingNumber ? `\nGuía: ${o.trackingNumber}` : '';
        return `${greeting}\nTu pedido *#${o.consecutive}* salió en camino${driver}.${tracking}\nLlegará a: ${o.shippingAddress ?? 'la dirección registrada'}.`;
      }
      case 'Delivered':
        return `${greeting}\n¡Gracias por tu compra! Tu pedido *#${o.consecutive}* fue entregado. Esperamos verte pronto.`;
      case 'Cancelled':
        return `${greeting}\nLamentamos informarte que tu pedido *#${o.consecutive}* fue cancelado. Si tienes dudas, escríbenos por aquí.`;
      default:
        return `${greeting}\nTe contactamos sobre tu pedido *#${o.consecutive}*.`;
    }
  }

  /** Mensaje del admin al distribuidor cuando le asignan un envío. */
  buildAdminToDistributor(o: OrderDto, dist: DistributorDto): string {
    const items = o.details
      .map(d => `• ${d.quantity}× ${d.productName}`)
      .join('\n');

    const lines = [
      `Hola ${dist.name} 🚚`,
      `Te asignamos el pedido *#${o.consecutive}*:`,
      ``,
      `*Cliente:* ${o.userFullName}`,
      o.userPhone ? `*Teléfono:* ${o.userPhone}` : '',
      `*Dirección:* ${o.shippingAddress ?? '—'}`,
      o.shippingCity ? `*Ciudad:* ${o.shippingCity}` : '',
      ``,
      `*Productos:*`,
      items,
      ``,
      `*Total a cobrar:* $${o.total.toLocaleString('es-CO')}`,
      `*Pago:* ${this.paymentMethodEs(o.paymentMethod)}${o.paymentStatus === 'Paid' ? ' (ya pagado)' : ''}`,
      o.trackingNumber ? `\n*Guía:* ${o.trackingNumber}` : ''
    ].filter(Boolean);

    return lines.join('\n');
  }

  private paymentMethodEs(m: string): string {
    return ({
      'OnDelivery': 'Pago contra entrega',
      'Transfer': 'Transferencia previa',
      'Cash': 'Efectivo en sede'
    } as Record<string, string>)[m] ?? m;
  }
}
