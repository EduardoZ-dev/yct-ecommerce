import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AcopioService } from '../../core/services/acopio.service';
import { CamionDto, ConductorDto, SaveConductorRequest } from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { SelectPickerComponent, SelectOption } from '../../shared/pickers';
import { ConfirmService } from '../../shared/confirm';
import { ToastService } from '../../core/toast/toast.service';

@Component({
  selector: 'app-conductores',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, SelectPickerComponent],
  templateUrl: './conductores.component.html',
  styleUrl: './conductores.component.scss'
})
export class ConductoresComponent implements OnInit {
  private svc = inject(AcopioService);
  private confirmSvc = inject(ConfirmService);
  private toast = inject(ToastService);

  conductores = signal<ConductorDto[]>([]);
  camiones = signal<CamionDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  error = signal('');

  formNombre = '';
  formCedula = '';
  formTelefono = '';
  formCamionId: number | null = null;
  formIsActive = true;

  camionOptions = computed<SelectOption<number | null>[]>(() => [
    { value: null, label: '— Sin asignar —' },
    ...this.camiones().map(c => ({ value: c.id as number | null, label: c.nombre, sub: c.estado }))
  ]);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    forkJoin({
      cond: this.svc.getConductores(),
      cam: this.svc.getCamiones()
    }).subscribe({
      next: ({ cond, cam }) => {
        this.conductores.set(cond.data ?? []);
        this.camiones.set(cam.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formNombre = '';
    this.formCedula = '';
    this.formTelefono = '';
    this.formCamionId = null;
    this.formIsActive = true;
    this.error.set('');
    this.showForm.set(true);
  }

  openEdit(c: ConductorDto): void {
    this.editingId.set(c.id);
    this.formNombre = c.nombreCompleto;
    this.formCedula = c.cedula ?? '';
    this.formTelefono = c.telefono ?? '';
    this.formCamionId = c.camionPreferidoId ?? null;
    this.formIsActive = c.isActive;
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void { this.showForm.set(false); this.error.set(''); }

  save(): void {
    this.error.set('');
    const payload: SaveConductorRequest = {
      id: this.editingId(),
      nombreCompleto: this.formNombre.trim(),
      cedula: this.formCedula.trim() || null,
      telefono: this.formTelefono.trim() || null,
      camionPreferidoId: this.formCamionId,
      userId: null,
      isActive: this.formIsActive
    };
    const id = this.editingId();
    const req = id ? this.svc.updateConductor(id, payload) : this.svc.createConductor(payload);
    req.subscribe({
      next: (res) => {
        if (res.success) { this.showForm.set(false); this.load(); }
        else this.error.set(res.message);
      },
      error: (e) => this.error.set(e.error?.message ?? 'Error al guardar')
    });
  }

  async delete(c: ConductorDto): Promise<void> {
    const ok = await this.confirmSvc.ask({
      title: 'Eliminar conductor',
      message: `Vas a eliminar al conductor "${c.nombreCompleto}". Esta acción no se puede deshacer. En el futuro esta acción quedará restringida por roles.`,
      confirmText: 'Eliminar',
      danger: true,
      icon: 'trash'
    });
    if (!ok) return;
    this.svc.deleteConductor(c.id).subscribe({
      next: (res) => {
        if (res.success) { this.toast.success('Conductor eliminado'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (e) => this.toast.error(e.error?.message ?? 'Error al eliminar')
    });
  }
}
