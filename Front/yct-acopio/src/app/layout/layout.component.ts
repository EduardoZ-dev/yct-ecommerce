import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { AuthService } from '../core/auth/auth.service';
import { IconComponent, IconName } from '../components/icon/icon.component';

interface NavItem {
  path: string;
  icon: IconName;
  label: string;
  exact: boolean;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, IconComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);
  mobileMenuOpen = signal(false);
  userMenuOpen = signal(false);

  navItems: NavItem[] = [
    { path: '/planillas',   icon: 'inbox',  label: 'Planillas',   exact: false },
    { path: '/camiones',    icon: 'truck',  label: 'Camiones',    exact: false },
    { path: '/conductores', icon: 'users',  label: 'Conductores', exact: false },
    { path: '/asistentes',  icon: 'users',  label: 'Asistentes',  exact: false },
    { path: '/granjeros',   icon: 'users',  label: 'Granjeros',   exact: false }
  ];

  private routeLabels: Record<string, string> = {
    '/planillas':   'Planillas de acopio',
    '/camiones':    'Camiones',
    '/conductores': 'Conductores',
    '/asistentes':  'Asistentes',
    '/granjeros':   'Granjeros'
  };

  currentUrl = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(e => (e as NavigationEnd).urlAfterRedirects),
      startWith(this.router.url)
    ),
    { initialValue: this.router.url }
  );

  pageTitle = computed(() => {
    const url = this.currentUrl();
    if (url.startsWith('/planillas/nueva')) return 'Nueva planilla';
    if (/^\/planillas\/\d+/.test(url)) return 'Editar planilla';
    const base = '/' + url.split('/')[1];
    return this.routeLabels[base] ?? '';
  });

  toggleMobile(): void { this.mobileMenuOpen.update(v => !v); }
  closeMobile(): void { this.mobileMenuOpen.set(false); }
  toggleUserMenu(): void { this.userMenuOpen.update(v => !v); }
  logout(): void { this.userMenuOpen.set(false); this.auth.logout(); }
}
