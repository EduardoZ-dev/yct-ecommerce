import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AcopioService } from '../../core/services/acopio.service';
import { GranjeroDto, SaveGranjeroRequest } from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { ConfirmService } from '../../shared/confirm';
import { ToastService } from '../../core/toast/toast.service';

@Component({
  selector: 'app-granjeros',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './granjeros.component.html',
  styleUrl: './granjeros.component.scss'
})
export class GranjerosComponent implements OnInit {
  private svc = inject(AcopioService);
  private confirmSvc = inject(ConfirmService);
  private toast = inject(ToastService);

  granjeros = signal<GranjeroDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  error = signal('');
  searchTerm = signal('');

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.granjeros();
    return this.granjeros().filter(g => {
      const hay = `${g.numero} ${g.nombreCompleto} ${g.cedula ?? ''} ${g.finca ?? ''} ${g.vereda ?? ''} ${g.municipio ?? ''}`.toLowerCase();
      return hay.includes(term);
    });
  });

  formNumero: number | null = null;
  formNombre = '';
  formCedula = '';
  formTelefono = '';
  formFinca = '';
  formVereda = '';
  formMunicipio = '';
  formPrecio: number | null = null;
  formNotas = '';
  formIsActive = true;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getGranjeros().subscribe({
      next: (res) => { this.granjeros.set(res.data ?? []); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formNumero = this.nextNumero();
    this.formNombre = '';
    this.formCedula = '';
    this.formTelefono = '';
    this.formFinca = '';
    this.formVereda = '';
    this.formMunicipio = '';
    this.formPrecio = null;
    this.formNotas = '';
    this.formIsActive = true;
    this.error.set('');
    this.showForm.set(true);
  }

  openEdit(g: GranjeroDto): void {
    this.editingId.set(g.id);
    this.formNumero = g.numero;
    this.formNombre = g.nombreCompleto;
    this.formCedula = g.cedula ?? '';
    this.formTelefono = g.telefono ?? '';
    this.formFinca = g.finca ?? '';
    this.formVereda = g.vereda ?? '';
    this.formMunicipio = g.municipio ?? '';
    this.formPrecio = g.precioLitro ?? null;
    this.formNotas = g.notas ?? '';
    this.formIsActive = g.isActive;
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void { this.showForm.set(false); this.error.set(''); }

  save(): void {
    this.error.set('');
    if (!this.formNumero || this.formNumero <= 0) {
      this.error.set('El número del proveedor es obligatorio y debe ser mayor a 0');
      return;
    }
    const payload: SaveGranjeroRequest = {
      id: this.editingId(),
      numero: this.formNumero,
      nombreCompleto: this.formNombre.trim(),
      cedula: this.formCedula.trim() || null,
      telefono: this.formTelefono.trim() || null,
      finca: this.formFinca.trim() || null,
      vereda: this.formVereda.trim() || null,
      municipio: this.formMunicipio.trim() || null,
      precioLitro: this.formPrecio,
      notas: this.formNotas.trim() || null,
      isActive: this.formIsActive
    };
    const id = this.editingId();
    const req = id ? this.svc.updateGranjero(id, payload) : this.svc.createGranjero(payload);
    req.subscribe({
      next: (res) => {
        if (res.success) { this.showForm.set(false); this.load(); }
        else this.error.set(res.message);
      },
      error: (e) => this.error.set(e.error?.message ?? 'Error al guardar')
    });
  }

  async delete(g: GranjeroDto): Promise<void> {
    const ok = await this.confirmSvc.ask({
      title: 'Eliminar granjero',
      message: `Vas a eliminar al granjero #${g.numero} "${g.nombreCompleto}". Esta acción no se puede deshacer. En el futuro esta acción quedará restringida por roles.`,
      confirmText: 'Eliminar',
      danger: true,
      icon: 'trash'
    });
    if (!ok) return;
    this.svc.deleteGranjero(g.id).subscribe({
      next: (res) => {
        if (res.success) { this.toast.success('Granjero eliminado'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (e) => this.toast.error(e.error?.message ?? 'Error al eliminar')
    });
  }

  private nextNumero(): number {
    const max = this.granjeros().reduce((m, g) => Math.max(m, g.numero), 0);
    return max + 1;
  }
}
