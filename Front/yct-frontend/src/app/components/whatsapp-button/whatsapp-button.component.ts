import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-whatsapp-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './whatsapp-button.component.html',
  styleUrl: './whatsapp-button.component.scss'
})
export class WhatsappButtonComponent {
  // Número en formato internacional sin + ni espacios
  private readonly phoneNumber = '573104525896';
  private readonly defaultMessage = 'Hola 👋, me gustaría conocer más sobre los productos de YCT Distribuciones';

  showTooltip = signal(true);
  isOpen = signal(false);

  constructor() {
    // Oculta el tooltip después de unos segundos
    setTimeout(() => this.showTooltip.set(false), 6000);
  }

  toggleMenu(): void {
    this.isOpen.update(v => !v);
    this.showTooltip.set(false);
  }

  closeMenu(): void {
    this.isOpen.set(false);
  }

  openWhatsApp(customMessage?: string): void {
    const msg = encodeURIComponent(customMessage ?? this.defaultMessage);
    const url = `https://wa.me/${this.phoneNumber}?text=${msg}`;
    window.open(url, '_blank', 'noopener,noreferrer');
    this.closeMenu();
  }

  quickMessages = [
    { icon: '🛍️', label: 'Hacer un pedido', text: 'Hola, me gustaría hacer un pedido' },
    { icon: '📦', label: 'Consultar mi pedido', text: 'Hola, quiero consultar el estado de mi pedido' },
    { icon: '💰', label: 'Conocer precios', text: 'Hola, quisiera conocer precios al por mayor' },
    { icon: '❓', label: 'Otra consulta', text: 'Hola, tengo una consulta sobre sus productos' }
  ];
}
