import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { OrderDto, PaymentMethod, PaymentStatus, ResponseBase, DistributorDto } from '../../core/models';
import { environment } from '../../../environments/environment';
import { IconComponent } from '../../components/icon/icon.component';
import { WhatsappService } from '../../core/services/whatsapp.service';

const STATUSES = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'] as const;

interface WizardStep {
  key: 'received' | 'confirmed' | 'shipped' | 'delivered';
  label: string;
  description: string;
}

const WIZARD_STEPS: WizardStep[] = [
  { key: 'received',  label: 'Recibido',   description: 'Pedido recibido del cliente.' },
  { key: 'confirmed', label: 'Confirmado', description: 'Confirmado y en preparación.' },
  { key: 'shipped',   label: 'Enviado',    description: 'En camino con el distribuidor.' },
  { key: 'delivered', label: 'Entregado',  description: 'Entregado al cliente.' }
];

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, IconComponent],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {
  private http = inject(HttpClient);
  wa = inject(WhatsappService);

  orders = signal<OrderDto[]>([]);
  distributors = signal<DistributorDto[]>([]);
  loading = signal(false);
  filterStatus = signal<string>('all');
  filterPayment = signal<'all' | 'unpaid' | 'paid'>('all');
  searchTerm = signal('');

  // ===== Filtros fecha + sort + paginación =====
  dateFrom = signal<string>('');
  dateTo = signal<string>('');
  quickRange = signal<'all' | 'today' | '7d' | 'month'>('all');

  sortBy = signal<'consecutive' | 'orderDate' | 'userFullName' | 'total' | 'status'>('consecutive');
  sortDir = signal<'asc' | 'desc'>('desc');

  page = signal(1);
  pageSize = signal(25);
  pageSizeOptions = [10, 25, 50, 100];

  // ===== Modal wizard =====
  modalOrder = signal<OrderDto | null>(null);
  wizardSubmitting = signal(false);
  wizardError = signal('');

  shipDistributorId = signal<number | null>(null);
  shipTrackingNumber = '';
  feedbackRating = signal<number | null>(null);
  feedbackComment = '';

  statuses = STATUSES;
  steps = WIZARD_STEPS;

  filteredOrders = computed(() => {
    const status = this.filterStatus();
    const payment = this.filterPayment();
    const term = this.searchTerm().toLowerCase().trim();
    const from = this.dateFrom();
    const to = this.dateTo();
    const fromTs = from ? new Date(from + 'T00:00:00').getTime() : null;
    const toTs = to ? new Date(to + 'T23:59:59').getTime() : null;

    return this.orders().filter(o => {
      if (status !== 'all' && o.status !== status) return false;
      if (payment === 'unpaid' && o.paymentStatus !== 'Unpaid') return false;
      if (payment === 'paid' && o.paymentStatus !== 'Paid') return false;
      if (fromTs || toTs) {
        const ts = new Date(o.orderDate).getTime();
        if (fromTs && ts < fromTs) return false;
        if (toTs && ts > toTs) return false;
      }
      if (term) {
        const hay = `${o.consecutive} ${o.orderNumber} ${o.userFullName} ${o.userPhone ?? ''}`.toLowerCase();
        if (!hay.includes(term)) return false;
      }
      return true;
    });
  });

  sortedOrders = computed(() => {
    const list = [...this.filteredOrders()];
    const by = this.sortBy();
    const dir = this.sortDir() === 'asc' ? 1 : -1;
    list.sort((a, b) => {
      let av: any = a[by as keyof OrderDto];
      let bv: any = b[by as keyof OrderDto];
      if (by === 'orderDate') { av = new Date(av).getTime(); bv = new Date(bv).getTime(); }
      if (typeof av === 'string') { av = av.toLowerCase(); bv = (bv ?? '').toLowerCase(); }
      if (av == null) return 1;
      if (bv == null) return -1;
      if (av < bv) return -1 * dir;
      if (av > bv) return  1 * dir;
      return 0;
    });
    return list;
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.sortedOrders().length / this.pageSize())));

  pagedOrders = computed(() => {
    const list = this.sortedOrders();
    const size = this.pageSize();
    const p = Math.min(this.page(), this.totalPages());
    const start = (p - 1) * size;
    return list.slice(start, start + size);
  });

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const cur = this.page();
    const arr: (number | '…')[] = [];
    const push = (n: number | '…') => arr.push(n);
    if (total <= 7) { for (let i = 1; i <= total; i++) push(i); return arr; }
    push(1);
    if (cur > 3) push('…');
    for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) push(i);
    if (cur < total - 2) push('…');
    push(total);
    return arr;
  });

  counts = computed(() => {
    const all = this.orders();
    return {
      all: all.length,
      pending: all.filter(o => o.status === 'Pending').length,
      confirmed: all.filter(o => o.status === 'Confirmed').length,
      shipped: all.filter(o => o.status === 'Shipped').length,
      delivered: all.filter(o => o.status === 'Delivered').length,
      cancelled: all.filter(o => o.status === 'Cancelled').length,
      unpaid: all.filter(o => o.paymentStatus === 'Unpaid' && o.status !== 'Cancelled').length
    };
  });

  activeDistributors = computed(() => this.distributors().filter(d => d.isActive));

  /** Índice del paso actual (0..3) basado en estado y timestamps. */
  currentStepIndex = computed(() => {
    const o = this.modalOrder();
    if (!o) return 0;
    if (o.status === 'Cancelled') return -1;
    if (o.deliveredAt) return 3;
    if (o.shippedAt) return 2;
    if (o.status === 'Confirmed') return 1;
    return 0;
  });

  ngOnInit(): void {
    this.loadOrders();
    this.loadDistributors();
  }

  loadOrders(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<OrderDto[]>>(`${environment.apiUrl}/api/Orders`).subscribe({
      next: (res) => {
        this.orders.set(res.data ?? []);
        const open = this.modalOrder();
        if (open) {
          const updated = (res.data ?? []).find(x => x.id === open.id);
          if (updated) this.modalOrder.set(updated);
        }
        this.loading.set(false);
        if (this.page() > this.totalPages()) this.page.set(1);
      },
      error: () => this.loading.set(false)
    });
  }

  loadDistributors(): void {
    this.http.get<ResponseBase<DistributorDto[]>>(`${environment.apiUrl}/api/Distributors`).subscribe({
      next: (res) => this.distributors.set(res.data ?? [])
    });
  }

  // ===== Modal control =====
  openModal(o: OrderDto): void {
    this.modalOrder.set(o);
    this.wizardError.set('');
    this.shipDistributorId.set(o.distributorId ?? null);
    this.shipTrackingNumber = o.trackingNumber ?? '';
    this.feedbackRating.set(o.customerRating ?? null);
    this.feedbackComment = o.feedbackComment ?? '';
  }

  closeModal(): void {
    this.modalOrder.set(null);
    this.wizardError.set('');
    this.wizardSubmitting.set(false);
  }

  // ===== Acciones del wizard =====
  confirm(): void {
    const o = this.modalOrder(); if (!o) return;
    this.runAction(`/${o.id}/confirm`, {});
  }

  ship(): void {
    const o = this.modalOrder(); if (!o) return;
    const distId = this.shipDistributorId();
    if (!distId) {
      this.wizardError.set('Selecciona un distribuidor antes de enviar.');
      return;
    }
    this.runAction(`/${o.id}/ship`, {
      distributorId: distId,
      trackingNumber: this.shipTrackingNumber.trim() || null
    });
  }

  deliver(): void {
    const o = this.modalOrder(); if (!o) return;
    this.runAction(`/${o.id}/deliver`, {
      customerRating: this.feedbackRating(),
      feedbackComment: this.feedbackComment.trim() || null
    });
  }

  cancel(): void {
    const o = this.modalOrder(); if (!o) return;
    if (!confirm(`¿Cancelar el pedido #${o.consecutive}?`)) return;
    this.runAction(`/${o.id}/cancel`, {});
  }

  togglePaid(): void {
    const o = this.modalOrder(); if (!o) return;
    const newStatus: PaymentStatus = o.paymentStatus === 'Paid' ? 'Unpaid' : 'Paid';
    this.http.patch<ResponseBase<OrderDto>>(
      `${environment.apiUrl}/api/Orders/${o.id}/payment`,
      { paymentStatus: newStatus }
    ).subscribe({ next: () => this.loadOrders() });
  }

  changePaymentMethod(method: PaymentMethod): void {
    const o = this.modalOrder(); if (!o) return;
    this.http.patch<ResponseBase<OrderDto>>(
      `${environment.apiUrl}/api/Orders/${o.id}/payment`,
      { paymentStatus: o.paymentStatus, paymentMethod: method }
    ).subscribe({ next: () => this.loadOrders() });
  }

  setRating(n: number): void {
    this.feedbackRating.set(this.feedbackRating() === n ? null : n);
  }

  // ===== WhatsApp =====
  whatsappCustomer(): void {
    const o = this.modalOrder();
    if (!o) return;
    if (!o.userPhone) {
      alert('Este cliente no tiene teléfono registrado.');
      return;
    }
    this.wa.open(o.userPhone, this.wa.buildAdminToCustomer(o));
  }

  whatsappDistributor(): void {
    const o = this.modalOrder();
    if (!o) return;
    const distId = this.shipDistributorId() ?? o.distributorId;
    if (!distId) {
      alert('Selecciona un distribuidor primero.');
      return;
    }
    const dist = this.distributors().find(d => d.id === distId);
    if (!dist) return;
    if (!dist.phone) {
      alert(`${dist.name} no tiene teléfono registrado.`);
      return;
    }
    this.wa.open(dist.phone, this.wa.buildAdminToDistributor(o, dist));
  }

  // ===== Impresión =====
  printDeliveryNote(): void {
    const o = this.modalOrder();
    if (!o) return;

    const dist = this.distributors().find(d => d.id === (o.distributorId ?? this.shipDistributorId()));
    const html = this.buildDeliveryNoteHtml(o, dist);

    const w = window.open('', '_blank', 'width=720,height=900');
    if (!w) {
      alert('Tu navegador bloqueó la ventana. Permite ventanas emergentes para imprimir.');
      return;
    }
    w.document.write(html);
    w.document.close();
    w.onload = () => {
      w.focus();
      w.print();
    };
  }

  private buildDeliveryNoteHtml(o: OrderDto, dist?: DistributorDto): string {
    const fmt = (n: number) => '$' + n.toLocaleString('es-CO');
    const date = new Date(o.orderDate).toLocaleString('es-CO', { dateStyle: 'medium', timeStyle: 'short' });
    const items = o.details
      .map(d => `<tr><td>${d.quantity}×</td><td>${d.productName}</td><td class="r">${fmt(d.unitPrice)}</td><td class="r">${fmt(d.subtotal)}</td></tr>`)
      .join('');

    return `<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8">
<title>Nota de entrega · Pedido #${o.consecutive}</title>
<style>
  * { box-sizing: border-box; }
  body { font-family: 'Segoe UI', Arial, sans-serif; color: #1E2A1E; margin: 0; padding: 20px; }
  .note { max-width: 580px; margin: 0 auto; }
  .head { display: flex; justify-content: space-between; align-items: flex-start; padding-bottom: 12px; border-bottom: 2px solid #7AB648; margin-bottom: 16px; }
  .brand { font-weight: 700; font-size: 18px; color: #5A9E2F; }
  .brand small { display: block; color: #7A8A7A; font-size: 11px; font-weight: 400; margin-top: 2px; }
  .num { text-align: right; }
  .num .big { font-size: 26px; font-weight: 800; color: #E8862A; line-height: 1; }
  .num small { color: #7A8A7A; font-size: 10px; text-transform: uppercase; letter-spacing: 0.4px; }
  .info { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 16px; }
  .info .box { padding: 10px; border: 1px solid #E0E4DB; border-radius: 6px; }
  .info small { display: block; color: #7A8A7A; font-size: 10px; text-transform: uppercase; letter-spacing: 0.4px; margin-bottom: 4px; font-weight: 600; }
  .info p { margin: 0; font-size: 13px; line-height: 1.4; }
  table { width: 100%; border-collapse: collapse; margin-bottom: 12px; }
  th { background: #EDF7E5; padding: 8px; text-align: left; font-size: 11px; text-transform: uppercase; letter-spacing: 0.4px; color: #5A9E2F; border-bottom: 1px solid #7AB648; }
  td { padding: 8px; font-size: 13px; border-bottom: 1px solid #E0E4DB; }
  .r { text-align: right; }
  .totals { margin-top: 8px; padding: 12px; background: #F5F7F2; border-radius: 6px; }
  .totals .row { display: flex; justify-content: space-between; font-size: 13px; padding: 3px 0; }
  .totals .row.total { font-size: 16px; font-weight: 800; color: #E8862A; border-top: 1px solid #E0E4DB; margin-top: 6px; padding-top: 8px; }
  .pay { display: inline-block; padding: 4px 10px; background: #FEF3E8; color: #C96E1A; border-radius: 12px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.4px; }
  .pay.paid { background: #E8F5E9; color: #4caf50; }
  .sign { margin-top: 30px; display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
  .sign .line { border-top: 1px solid #1E2A1E; padding-top: 6px; text-align: center; font-size: 11px; color: #7A8A7A; }
  .foot { margin-top: 16px; text-align: center; color: #7A8A7A; font-size: 10px; }
  @media print { body { padding: 0; } .note { max-width: 100%; } }
</style>
</head>
<body>
  <div class="note">
    <div class="head">
      <div class="brand">Distribuciones YCT<small>Derivados Lácteos · Cesar/Magdalena/Guajira</small></div>
      <div class="num">
        <small>Pedido</small>
        <div class="big">#${o.consecutive}</div>
        <small>${o.orderNumber}</small>
      </div>
    </div>

    <div class="info">
      <div class="box">
        <small>Cliente</small>
        <p><strong>${o.userFullName}</strong></p>
        ${o.userPhone ? `<p>${o.userPhone}</p>` : ''}
      </div>
      <div class="box">
        <small>Dirección de entrega</small>
        <p>${o.shippingAddress ?? '—'}</p>
        ${o.shippingCity ? `<p>${o.shippingCity}</p>` : ''}
      </div>
      <div class="box">
        <small>Fecha del pedido</small>
        <p>${date}</p>
      </div>
      <div class="box">
        <small>Distribuidor</small>
        <p>${dist ? `<strong>${dist.name}</strong>${dist.vehicleType ? ` · ${dist.vehicleType}` : ''}${dist.vehiclePlate ? ` · ${dist.vehiclePlate}` : ''}` : '—'}</p>
        ${o.trackingNumber ? `<p>Guía: ${o.trackingNumber}</p>` : ''}
      </div>
    </div>

    <table>
      <thead>
        <tr><th>Cant.</th><th>Producto</th><th class="r">Unitario</th><th class="r">Subtotal</th></tr>
      </thead>
      <tbody>${items}</tbody>
    </table>

    <div class="totals">
      <div class="row"><span>Subtotal</span><strong>${fmt(o.total)}</strong></div>
      <div class="row total"><span>Total a cobrar</span><strong>${fmt(o.total)}</strong></div>
      <div class="row" style="margin-top:8px;">
        <span>Pago</span>
        <span class="pay ${o.paymentStatus === 'Paid' ? 'paid' : ''}">
          ${this.paymentMethodLabel(o.paymentMethod)}${o.paymentStatus === 'Paid' ? ' · Pagado' : ''}
        </span>
      </div>
    </div>

    ${o.notes ? `<p style="margin-top:12px;font-size:12px;color:#3D4A3D;"><strong>Notas:</strong> ${o.notes}</p>` : ''}

    <div class="sign">
      <div class="line">Firma del distribuidor</div>
      <div class="line">Firma del cliente</div>
    </div>

    <div class="foot">Generado el ${new Date().toLocaleString('es-CO')} · Distribuciones YCT</div>
  </div>
</body>
</html>`;
  }

  private runAction(path: string, body: object): void {
    this.wizardSubmitting.set(true);
    this.wizardError.set('');
    this.http.patch<ResponseBase<OrderDto>>(`${environment.apiUrl}/api/Orders${path}`, body).subscribe({
      next: (res) => {
        this.wizardSubmitting.set(false);
        if (res.success) this.loadOrders();
        else this.wizardError.set(res.message || 'Error al actualizar');
      },
      error: (e) => {
        this.wizardSubmitting.set(false);
        this.wizardError.set(e.error?.message ?? 'Error al actualizar');
      }
    });
  }

  setStatusFilter(s: string): void { this.filterStatus.set(s); this.page.set(1); }
  setPaymentFilter(p: 'all' | 'unpaid' | 'paid'): void { this.filterPayment.set(p); this.page.set(1); }

  // ===== Date / Sort / Paginación =====
  setQuickRange(key: 'all' | 'today' | '7d' | 'month'): void {
    this.quickRange.set(key);
    const now = new Date();
    const fmt = (d: Date) => d.toISOString().slice(0, 10);
    if (key === 'all') { this.dateFrom.set(''); this.dateTo.set(''); }
    else if (key === 'today') { const t = fmt(now); this.dateFrom.set(t); this.dateTo.set(t); }
    else if (key === '7d') { const d = new Date(now); d.setDate(d.getDate() - 6); this.dateFrom.set(fmt(d)); this.dateTo.set(fmt(now)); }
    else if (key === 'month') { const d = new Date(now.getFullYear(), now.getMonth(), 1); this.dateFrom.set(fmt(d)); this.dateTo.set(fmt(now)); }
    this.page.set(1);
  }

  onDateChange(): void { this.quickRange.set('all'); this.page.set(1); }

  toggleSort(col: 'consecutive' | 'orderDate' | 'userFullName' | 'total' | 'status'): void {
    if (this.sortBy() === col) this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    else { this.sortBy.set(col); this.sortDir.set(col === 'orderDate' || col === 'consecutive' || col === 'total' ? 'desc' : 'asc'); }
  }

  setPage(n: number): void {
    const total = this.totalPages();
    this.page.set(Math.max(1, Math.min(n, total)));
  }

  setPageSize(s: number): void { this.pageSize.set(s); this.page.set(1); }

  // ===== Status =====
  statusLabel(status: string): string {
    return ({
      'Pending': 'Pendiente',
      'Confirmed': 'Confirmado',
      'Shipped': 'En camino',
      'Delivered': 'Entregado',
      'Cancelled': 'Cancelado'
    } as Record<string, string>)[status] ?? status;
  }

  statusDescription(status: string): string {
    return ({
      'Pending': 'Pedido recibido. Falta confirmar disponibilidad y datos.',
      'Confirmed': 'Confirmado por el negocio. Se está preparando.',
      'Shipped': 'Salió del centro de distribución. Va en camino al cliente.',
      'Delivered': 'Entregado al cliente. Pedido completado.',
      'Cancelled': 'Pedido cancelado. Stock restaurado si aplicaba.'
    } as Record<string, string>)[status] ?? '';
  }

  statusClass(status: string): string {
    return ({
      'Pending': 'pending',
      'Confirmed': 'confirmed',
      'Shipped': 'shipped',
      'Delivered': 'delivered',
      'Cancelled': 'cancelled'
    } as Record<string, string>)[status] ?? 'pending';
  }

  // ===== Payment =====
  paymentMethodLabel(m: string): string {
    return ({
      'OnDelivery': 'Pago contra entrega',
      'Transfer': 'Transferencia previa',
      'Cash': 'Efectivo en sede'
    } as Record<string, string>)[m] ?? m;
  }

  paymentMethodShort(m: string): string {
    return ({
      'OnDelivery': 'Contra entrega',
      'Transfer': 'Transferencia',
      'Cash': 'Efectivo'
    } as Record<string, string>)[m] ?? m;
  }

  paymentStatusLabel(s: string): string {
    return ({
      'Unpaid': 'Pago pendiente',
      'Paid': 'Pagado',
      'Refunded': 'Reembolsado'
    } as Record<string, string>)[s] ?? s;
  }

  paymentMethods: PaymentMethod[] = ['OnDelivery', 'Transfer', 'Cash'];
}
