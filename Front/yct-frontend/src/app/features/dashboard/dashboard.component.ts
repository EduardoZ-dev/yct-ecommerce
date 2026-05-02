import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service';
import { ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';
import { IconComponent } from '../../components/icon/icon.component';

interface RecentOrder {
  id: number;
  orderNumber: string;
  orderDate: string;
  total: number;
  status: string;
  customerName: string;
}

interface LowStockProduct {
  id: number;
  name: string;
  stock: number;
  imageUrl: string | null;
}

interface DashboardMetrics {
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  totalCategories: number;
  totalOrders: number;
  pendingOrders: number;
  monthlyRevenue: number;
  totalRevenue: number;
  recentOrders: RecentOrder[];
  lowStockList: LowStockProduct[];
}

interface MonthlyRevenue {
  year: number;
  month: number;
  label: string;
  revenue: number;
  orderCount: number;
  isCurrent: boolean;
}

interface RevenueProjection {
  months: MonthlyRevenue[];
  totalRevenue: number;
  currentMonthRevenue: number;
  previousMonthRevenue: number;
  monthlyAverage: number;
  bestMonthRevenue: number;
  bestMonthLabel: string;
  totalOrders: number;
  averageTicket: number;
  growthVsPreviousMonth: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, IconComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  metrics = signal<DashboardMetrics | null>(null);
  loading = signal(true);

  // Modal de proyección de ingresos
  showRevenueModal = signal(false);
  projection = signal<RevenueProjection | null>(null);
  loadingProjection = signal(false);

  constructor(public auth: AuthService, private http: HttpClient) {}

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<DashboardMetrics>>(`${environment.apiUrl}/api/Dashboard/metrics`).subscribe({
      next: (res) => {
        this.metrics.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openRevenueModal(): void {
    this.showRevenueModal.set(true);
    if (!this.projection()) this.loadProjection();
  }

  closeRevenueModal(): void {
    this.showRevenueModal.set(false);
  }

  loadProjection(): void {
    this.loadingProjection.set(true);
    this.http.get<ResponseBase<RevenueProjection>>(`${environment.apiUrl}/api/Dashboard/revenue-projection`).subscribe({
      next: (res) => {
        this.projection.set(res.data);
        this.loadingProjection.set(false);
      },
      error: () => this.loadingProjection.set(false)
    });
  }

  /** Devuelve el % de altura de la barra para un mes (0-100). */
  barHeight(m: MonthlyRevenue): number {
    const p = this.projection();
    if (!p || p.bestMonthRevenue <= 0) return 0;
    return Math.max(2, Math.round((m.revenue / p.bestMonthRevenue) * 100));
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'pending',
      'Confirmed': 'confirmed',
      'Shipped': 'shipped',
      'Delivered': 'delivered',
      'Cancelled': 'cancelled'
    };
    return map[status] ?? 'pending';
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'Pendiente',
      'Confirmed': 'Confirmado',
      'Shipped': 'Enviado',
      'Delivered': 'Entregado',
      'Cancelled': 'Cancelado'
    };
    return map[status] ?? status;
  }
}
