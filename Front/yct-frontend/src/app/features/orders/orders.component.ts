import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OrderDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {
  orders = signal<OrderDto[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<OrderDto[]>>(`${environment.apiUrl}/api/Orders`).subscribe({
      next: (res) => {
        this.orders.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  updateStatus(id: number, status: string): void {
    this.http.patch<ResponseBase<OrderDto>>(`${environment.apiUrl}/api/Orders/${id}/status`, { status }).subscribe({
      next: () => this.loadOrders()
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'pending',
      'Confirmed': 'confirmed',
      'Shipped': 'shipped',
      'Delivered': 'delivered',
      'Cancelled': 'cancelled'
    };
    return map[status] ?? '';
  }
}
