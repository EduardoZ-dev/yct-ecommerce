import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { IconComponent, IconName } from '../components/icon/icon.component';

interface NavItem {
  path: string;
  icon: IconName;
  label: string;
  exact: boolean;
  /** Roles permitidos. undefined = cualquier rol con acceso al panel */
  requireRoles?: string[];
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, IconComponent],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent {
  sidebarCollapsed = signal(false);
  mobileMenuOpen = signal(false);
  userMenuOpen = signal(false);

  private allNavItems: NavItem[] = [
    { path: '/admin', icon: 'dashboard', label: 'Dashboard', exact: true },
    { path: '/admin/products', icon: 'cube', label: 'Productos', exact: false },
    { path: '/admin/categories', icon: 'folder', label: 'Categorías', exact: false },
    { path: '/admin/orders', icon: 'shopping-bag', label: 'Pedidos', exact: false },
    { path: '/admin/distributors', icon: 'truck', label: 'Distribuidores', exact: false, requireRoles: ['SuperAdmin', 'Admin'] },
    { path: '/admin/users', icon: 'users', label: 'Usuarios', exact: false, requireRoles: ['SuperAdmin', 'Admin'] },
    { path: '/admin/audit-log', icon: 'shield', label: 'Auditoría', exact: false, requireRoles: ['SuperAdmin', 'Admin'] }
  ];

  navItems = computed(() => {
    const role = this.auth.role();
    return this.allNavItems.filter(item =>
      !item.requireRoles || item.requireRoles.includes(role)
    );
  });

  constructor(public auth: AuthService) {}

  toggleSidebar(): void {
    if (window.innerWidth <= 768) {
      this.mobileMenuOpen.update(v => !v);
    } else {
      this.sidebarCollapsed.update(v => !v);
    }
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  toggleUserMenu(): void {
    this.userMenuOpen.update(v => !v);
  }

  logout(): void {
    this.userMenuOpen.set(false);
    this.auth.logout();
  }
}
