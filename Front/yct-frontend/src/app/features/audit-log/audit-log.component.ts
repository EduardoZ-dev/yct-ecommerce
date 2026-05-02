import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AuditLogDto, AuditLogPageDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';
import { IconComponent, IconName } from '../../components/icon/icon.component';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './audit-log.component.html',
  styleUrl: './audit-log.component.scss'
})
export class AuditLogComponent implements OnInit {
  private http = inject(HttpClient);

  logs = signal<AuditLogDto[]>([]);
  total = signal(0);
  page = signal(1);
  pageSize = signal(50);
  loading = signal(false);
  expandedId = signal<number | null>(null);

  filterAction = signal<string>('');
  filterEntityType = signal<string>('');
  filterSuccess = signal<string>('');

  actions = ['Login', 'LoginFailed', 'Logout', 'Create', 'Update', 'Delete', 'StatusChange'];
  entityTypes = ['Auth', 'Product', 'Category', 'Order', 'User'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    let params = new HttpParams()
      .set('page', this.page())
      .set('pageSize', this.pageSize());
    if (this.filterAction()) params = params.set('action', this.filterAction());
    if (this.filterEntityType()) params = params.set('entityType', this.filterEntityType());
    if (this.filterSuccess() !== '') params = params.set('success', this.filterSuccess());

    this.http.get<ResponseBase<AuditLogPageDto>>(`${environment.apiUrl}/api/AuditLog`, { params }).subscribe({
      next: (res) => {
        this.logs.set(res.data?.items ?? []);
        this.total.set(res.data?.total ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  clearFilters(): void {
    this.filterAction.set('');
    this.filterEntityType.set('');
    this.filterSuccess.set('');
    this.page.set(1);
    this.load();
  }

  toggleExpand(id: number): void {
    this.expandedId.update(v => v === id ? null : id);
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.load();
    }
  }

  nextPage(): void {
    if (this.page() * this.pageSize() < this.total()) {
      this.page.update(p => p + 1);
      this.load();
    }
  }

  formatDetails(details: string | null): string {
    if (!details) return '';
    try {
      return JSON.stringify(JSON.parse(details), null, 2);
    } catch {
      return details;
    }
  }

  actionLabel(action: string): string {
    return {
      'Login': 'Inicio de sesión',
      'LoginFailed': 'Login fallido',
      'Logout': 'Cierre de sesión',
      'Create': 'Crear',
      'Update': 'Actualizar',
      'Delete': 'Eliminar',
      'StatusChange': 'Cambio de estado'
    }[action] ?? action;
  }

  actionClass(action: string, success: boolean): string {
    if (!success) return 'failed';
    return {
      'Login': 'login',
      'LoginFailed': 'failed',
      'Logout': 'logout',
      'Create': 'create',
      'Update': 'update',
      'Delete': 'delete',
      'StatusChange': 'status'
    }[action] ?? '';
  }

  entityIcon(entity: string): IconName {
    return ({
      'Auth': 'lock' as IconName,
      'Product': 'cube' as IconName,
      'Category': 'folder' as IconName,
      'Order': 'shopping-bag' as IconName,
      'User': 'user' as IconName
    }[entity] as IconName | undefined) ?? 'shield';
  }
}
