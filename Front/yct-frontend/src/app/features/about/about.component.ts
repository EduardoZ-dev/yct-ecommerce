import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent {
  values = [
    { icon: '✓', title: 'Calidad', text: 'Inspecciones diarias, trazabilidad y auditorías internas que garantizan frescura y autenticidad en cada producto.' },
    { icon: '🌱', title: 'Responsabilidad Ambiental', text: 'Procesos eficientes, empaques sostenibles y prácticas de reducción de residuos en toda la operación.' },
    { icon: '💡', title: 'Innovación', text: 'Investigación continua y colaboración con centros especializados para mejorar productos y procesos.' },
    { icon: '🤝', title: 'Ética y Respeto', text: 'Trato respetuoso con clientes, proveedores y colaboradores. Confidencialidad y conducta profesional.' },
    { icon: '🛡️', title: 'Seguridad', text: 'Cumplimos el programa de seguridad e higiene vigente para proteger a nuestro equipo y clientes.' },
    { icon: '👥', title: 'Desarrollo Humano', text: 'Capacitación continua, evaluaciones justas y crecimiento profesional para todo nuestro talento.' }
  ];

  brands = [
    { name: 'Lácteos Ideal', image: 'assets/brands/lacteos-ideal.png' },
    { name: 'Mamá Sara', image: 'assets/brands/mama-sara.png' }
  ];
}
