import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AcopioService } from '../../core/services/acopio.service';
import { ToastService } from '../../core/toast/toast.service';
import { PlanillaHeaderDto, CamionDto, ConductorDto } from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { DatePickerComponent, SelectPickerComponent, SelectOption } from '../../shared/pickers';
import { ConfirmService } from '../../shared/confirm';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-planillas',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, IconComponent, DatePickerComponent, SelectPickerComponent],
  templateUrl: './planillas.component.html',
  styleUrl: './planillas.component.scss'
})
export class PlanillasComponent implements OnInit {
  private svc = inject(AcopioService);
  private toast = inject(ToastService);
  private confirmSvc = inject(ConfirmService);

  planillas = signal<PlanillaHeaderDto[]>([]);
  camiones = signal<CamionDto[]>([]);
  conductores = signal<ConductorDto[]>([]);
  loading = signal(false);

  // Filtros
  fDesde = signal<string>('');
  fHasta = signal<string>('');
  fCamionId = signal<number | null>(null);
  fConductorId = signal<number | null>(null);
  fEstado = signal<string>('');

  page = signal(1);
  pageSize = PAGE_SIZE;

  estados = ['EnProgreso', 'EsperandoDescargue', 'Conciliada', 'Anulada'];

  camionOptions = computed<SelectOption<number | null>[]>(() => [
    { value: null, label: 'Todas las rutas' },
    ...this.camiones().map(c => ({ value: c.id as number | null, label: c.nombre, sub: c.placa ?? undefined }))
  ]);
  conductorOptions = computed<SelectOption<number | null>[]>(() => [
    { value: null, label: 'Todos los conductores' },
    ...this.conductores().map(c => ({ value: c.id as number | null, label: c.nombreCompleto, sub: c.telefono ?? undefined }))
  ]);
  estadoOptions = computed<SelectOption<string>[]>(() => [
    { value: '', label: 'Todos los estados' },
    ...this.estados.map(e => ({ value: e, label: this.statusLabel(e) }))
  ]);

  filtered = computed(() => {
    let list = this.planillas();
    const desde = this.fDesde();
    const hasta = this.fHasta();
    const cam = this.fCamionId();
    const con = this.fConductorId();
    const est = this.fEstado();

    if (desde) list = list.filter(p => p.fecha.substring(0, 10) >= desde);
    if (hasta) list = list.filter(p => p.fecha.substring(0, 10) <= hasta);
    if (cam != null) list = list.filter(p => p.camionId === cam);
    if (con != null) list = list.filter(p => p.conductorId === con);
    if (est) list = list.filter(p => p.status === est);
    return list;
  });

  pageCount = computed(() => Math.max(1, Math.ceil(this.filtered().length / this.pageSize)));

  paged = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  });

  activeFiltersCount = computed(() => {
    let n = 0;
    if (this.fDesde()) n++;
    if (this.fHasta()) n++;
    if (this.fCamionId() != null) n++;
    if (this.fConductorId() != null) n++;
    if (this.fEstado()) n++;
    return n;
  });

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      p: this.svc.getPlanillas(),
      c: this.svc.getCamiones(),
      d: this.svc.getConductores()
    }).subscribe({
      next: ({ p, c, d }) => {
        this.planillas.set(p.data ?? []);
        this.camiones.set(c.data ?? []);
        this.conductores.set(d.data ?? []);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.toast.error('Error cargando planillas'); }
    });
  }

  load(): void {
    this.loading.set(true);
    this.svc.getPlanillas().subscribe({
      next: (res) => { this.planillas.set(res.data ?? []); this.loading.set(false); },
      error: () => { this.loading.set(false); this.toast.error('Error cargando planillas'); }
    });
  }

  clearFilters(): void {
    this.fDesde.set('');
    this.fHasta.set('');
    this.fCamionId.set(null);
    this.fConductorId.set(null);
    this.fEstado.set('');
    this.page.set(1);
  }

  goPage(p: number): void {
    if (p < 1 || p > this.pageCount()) return;
    this.page.set(p);
  }

  // Resetear página al cambiar filtros
  onFilterChange(): void { this.page.set(1); }

  async delete(p: PlanillaHeaderDto, ev: Event): Promise<void> {
    ev.stopPropagation();
    ev.preventDefault();
    const ok = await this.confirmSvc.ask({
      title: 'Eliminar planilla',
      message: `Vas a eliminar la planilla ${p.codigo} del ${this.dateFormat(p.fecha)}. Esta acción no se puede deshacer. En el futuro esta acción quedará restringida por roles.`,
      confirmText: 'Eliminar',
      cancelText: 'Cancelar',
      danger: true,
      icon: 'trash'
    });
    if (!ok) return;
    this.svc.deletePlanilla(p.id).subscribe({
      next: (res) => {
        if (res.success) { this.toast.success('Planilla eliminada'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (e) => this.toast.error(e.error?.message ?? 'Error al eliminar')
    });
  }

  dateFormat(iso: string): string {
    return new Date(iso).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  statusLabel(s: string): string {
    return (s || '').replace(/([a-z])([A-Z])/g, '$1 $2').toUpperCase();
  }

  statusClass(s: string): string {
    return (s || '').toLowerCase();
  }
}
