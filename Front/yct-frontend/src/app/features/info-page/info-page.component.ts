import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

type InfoType = 'payment' | 'shipping' | 'quality' | 'contact';

interface InfoContent {
  icon: string;
  badge: string;
  title: string;
  subtitle: string;
  description: string;
  sections: { icon: string; title: string; text: string }[];
  cta?: { text: string; disabled: boolean };
}

const CONTENT: Record<InfoType, InfoContent> = {
  payment: {
    icon: '🛡️',
    badge: 'Pago Seguro',
    title: 'Tus pagos, 100% protegidos',
    subtitle: 'Transacciones encriptadas con los más altos estándares de seguridad',
    description: 'En YCT utilizamos las pasarelas de pago más confiables del mercado. Todas tus transacciones están protegidas con cifrado SSL de extremo a extremo y cumplimos con los estándares PCI DSS para el manejo seguro de tarjetas.',
    sections: [
      { icon: '🔒', title: 'Cifrado SSL', text: 'Toda la información viaja encriptada entre tu navegador y nuestros servidores.' },
      { icon: '💳', title: 'Múltiples métodos', text: 'Aceptaremos tarjetas crédito/débito, PSE, Nequi, Daviplata y pagos contra entrega.' },
      { icon: '✓', title: 'PCI DSS', text: 'Cumplimos con los estándares internacionales de seguridad para tarjetas de crédito.' },
      { icon: '🔐', title: '3D Secure', text: 'Autenticación adicional para pagos con tarjeta, protegiéndote contra fraudes.' }
    ],
    cta: { text: 'Próximamente disponible', disabled: true }
  },
  shipping: {
    icon: '🚚',
    badge: 'Envío en 24h',
    title: 'Del campo a tu mesa, siempre fresco',
    subtitle: 'Estimaciones de entrega precisas según tu ubicación',
    description: 'Nos comprometemos a entregar tus productos lácteos en menos de 24 horas desde que realizas tu pedido. Nuestra cadena de frío garantiza que todo llegue en óptimas condiciones.',
    sections: [
      { icon: '❄️', title: 'Cadena de frío', text: 'Transporte refrigerado para mantener la frescura y calidad de los productos.' },
      { icon: '⏱️', title: 'Entrega en 24h', text: 'Cobertura en Cesar, Magdalena y La Guajira. Entregamos al día siguiente en las principales ciudades de la región.' },
      { icon: '📍', title: 'Rastreo en vivo', text: 'Podrás seguir tu pedido en tiempo real desde el momento en que sale de nuestra sede.' },
      { icon: '🏠', title: 'A domicilio', text: 'Entregamos directamente en tu puerta, sin costos ocultos ni intermediarios.' }
    ],
    cta: { text: 'Calcular tiempo de entrega', disabled: true }
  },
  quality: {
    icon: '✓',
    badge: 'Calidad Garantizada',
    title: 'Productos directo del campo',
    subtitle: 'Comprometidos con la frescura, naturalidad y sabor auténtico',
    description: 'Trabajamos directamente con productores locales que mantienen prácticas sostenibles y respetan el bienestar animal. Cada producto pasa por rigurosos controles de calidad antes de llegar a tus manos.',
    sections: [
      { icon: '🐄', title: 'Productores locales', text: 'Trabajamos con fincas colombianas que cuidan a sus animales y la tierra.' },
      { icon: '🌱', title: '100% Natural', text: 'Sin conservantes ni aditivos artificiales. Solo ingredientes que reconoces.' },
      { icon: '🔬', title: 'Control de calidad', text: 'Laboratorio propio que analiza cada lote antes de salir a distribución.' },
      { icon: '📜', title: 'Certificaciones', text: 'Invima, BPM y HACCP vigentes en todos nuestros procesos de producción.' }
    ]
  },
  contact: {
    icon: '💬',
    badge: 'Soporte 7 días',
    title: 'Estamos aquí para ayudarte',
    subtitle: 'Atención personalizada todos los días de la semana',
    description: 'Nuestro equipo de soporte está disponible 7 días a la semana para resolver tus dudas, atender pedidos especiales o ayudarte con cualquier inconveniente. Contáctanos por el medio que prefieras.',
    sections: [
      { icon: '📞', title: 'Teléfono', text: '+57 310 452 5896 • Lunes a domingo de 7 AM a 9 PM' },
      { icon: '📧', title: 'Email', text: 'ventas@yct.com • Respuesta en menos de 2 horas' },
      { icon: '💬', title: 'WhatsApp', text: '+57 310 452 5896 • Chat directo con un asesor' },
      { icon: '📍', title: 'Visítanos', text: 'Planta principal: Cl. 5 #5A-36, San Diego – Cesar. Cobertura en Cesar, Magdalena y La Guajira.' }
    ],
    cta: { text: 'Enviar mensaje', disabled: true }
  }
};

@Component({
  selector: 'app-info-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './info-page.component.html',
  styleUrl: './info-page.component.scss'
})
export class InfoPageComponent {
  private route = inject(ActivatedRoute);

  private infoType = toSignal(
    this.route.data.pipe(map(data => (data['type'] as InfoType) ?? 'contact')),
    { initialValue: 'contact' as InfoType }
  );

  content = computed<InfoContent>(() => CONTENT[this.infoType()]);
}
