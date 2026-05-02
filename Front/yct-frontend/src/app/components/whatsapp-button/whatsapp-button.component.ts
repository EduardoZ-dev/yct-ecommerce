import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent, IconName } from '../icon/icon.component';
import { WhatsappService } from '../../core/services/whatsapp.service';

interface QuickAction {
  icon: IconName;
  label: string;
  text: string;
}

@Component({
  selector: 'app-whatsapp-button',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './whatsapp-button.component.html',
  styleUrl: './whatsapp-button.component.scss'
})
export class WhatsappButtonComponent {
  private wa = inject(WhatsappService);

  showTooltip = signal(true);
  isOpen = signal(false);

  quickMessages: QuickAction[] = [
    { icon: 'shopping-bag', label: 'Hacer un pedido', text: 'Hola, me gustaría hacer un pedido.' },
    { icon: 'truck',        label: 'Consultar mi pedido', text: 'Hola, quiero consultar el estado de mi pedido.' },
    { icon: 'money',        label: 'Precios al por mayor', text: 'Hola, quisiera conocer precios al por mayor.' },
    { icon: 'mail',         label: 'Otra consulta', text: 'Hola, tengo una consulta sobre sus productos.' }
  ];

  constructor() {
    setTimeout(() => this.showTooltip.set(false), 6000);
  }

  toggleMenu(): void {
    this.isOpen.update(v => !v);
    this.showTooltip.set(false);
  }

  closeMenu(): void {
    this.isOpen.set(false);
  }

  send(text: string): void {
    this.wa.openBusiness(text);
    this.closeMenu();
  }
}
