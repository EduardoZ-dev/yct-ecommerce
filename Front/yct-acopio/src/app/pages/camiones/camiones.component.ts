import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AcopioService } from '../../core/services/acopio.service';
import { CamionDto, CamionEstado, SaveCamionRequest } from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { SelectPickerComponent, SelectOption } from '../../shared/pickers';
import { ConfirmService } from '../../shared/confirm';
import { ToastService } from '../../core/toast/toast.service';

const ESTADOS: CamionEstado[] = ['Activo', 'Mantenimiento', 'Inactivo'];
const ESTADO_OPTIONS: SelectOption<CamionEstado>[] = ESTADOS.map(e => ({ value: e, label: e }));

@Component({
  selector: 'app-camiones',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, SelectPickerComponent],
  templateUrl: './camiones.component.html',
  styleUrl: './camiones.component.scss'
})
export class CamionesComponent implements OnInit {
  private svc = inject(AcopioService);
  private confirmSvc = inject(ConfirmService);
  private toast = inject(ToastService);

  camiones = signal<CamionDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  error = signal('');

  estados = ESTADOS;
  estadoOptions = ESTADO_OPTIONS;
  formNombre = '';
  formPlaca = '';
  formEstado: CamionEstado = 'Activo';
  formNotas = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getCamiones().subscribe({
      next: (res) => { this.camiones.set(res.data ?? []); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formNombre = '';
    this.formPlaca = '';
    this.formEstado = 'Activo';
    this.formNotas = '';
    this.error.set('');
    this.showForm.set(true);
  }

  openEdit(c: CamionDto): void {
    this.editingId.set(c.id);
    this.formNombre = c.nombre;
    this.formPlaca = c.placa ?? '';
    this.formEstado = c.estado;
    this.formNotas = c.notas ?? '';
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void { this.showForm.set(false); this.error.set(''); }

  save(): void {
    this.error.set('');
    const payload: SaveCamionRequest = {
      id: this.editingId(),
      nombre: this.formNombre.trim(),
      placa: this.formPlaca.trim() || null,
      estado: this.formEstado,
      notas: this.formNotas.trim() || null
    };
    const id = this.editingId();
    const req = id ? this.svc.updateCamion(id, payload) : this.svc.createCamion(payload);
    req.subscribe({
      next: (res) => {
        if (res.success) { this.showForm.set(false); this.load(); }
        else this.error.set(res.message);
      },
      error: (e) => this.error.set(e.error?.message ?? 'Error al guardar')
    });
  }

  async delete(c: CamionDto): Promise<void> {
    const ok = await this.confirmSvc.ask({
      title: 'Eliminar camión',
      message: `Vas a eliminar el camión "${c.nombre}". Esta acción no se puede deshacer. En el futuro esta acción quedará restringida por roles.`,
      confirmText: 'Eliminar',
      danger: true,
      icon: 'trash'
    });
    if (!ok) return;
    this.svc.deleteCamion(c.id).subscribe({
      next: (res) => {
        if (res.success) { this.toast.success('Camión eliminado'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (e) => this.toast.error(e.error?.message ?? 'Error al eliminar')
    });
  }

  estadoClass(e: string): string {
    return ({ 'Activo': 'active', 'Mantenimiento': 'maintenance', 'Inactivo': 'inactive' } as Record<string, string>)[e] ?? 'inactive';
  }
}
