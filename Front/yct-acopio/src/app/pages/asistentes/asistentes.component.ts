import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AcopioService } from '../../core/services/acopio.service';
import { AsistenteDto, CamionDto, SaveAsistenteRequest } from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { SelectPickerComponent, SelectOption } from '../../shared/pickers';
import { ConfirmService } from '../../shared/confirm';
import { ToastService } from '../../core/toast/toast.service';

@Component({
  selector: 'app-asistentes',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, SelectPickerComponent],
  templateUrl: './asistentes.component.html',
  styleUrl: './asistentes.component.scss'
})
export class AsistentesComponent implements OnInit {
  private svc = inject(AcopioService);
  private confirmSvc = inject(ConfirmService);
  private toast = inject(ToastService);

  asistentes = signal<AsistenteDto[]>([]);
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
      asis: this.svc.getAsistentes(),
      cam: this.svc.getCamiones()
    }).subscribe({
      next: ({ asis, cam }) => {
        this.asistentes.set(asis.data ?? []);
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

  openEdit(a: AsistenteDto): void {
    this.editingId.set(a.id);
    this.formNombre = a.nombreCompleto;
    this.formCedula = a.cedula ?? '';
    this.formTelefono = a.telefono ?? '';
    this.formCamionId = a.camionPreferidoId ?? null;
    this.formIsActive = a.isActive;
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void { this.showForm.set(false); this.error.set(''); }

  save(): void {
    this.error.set('');
    const payload: SaveAsistenteRequest = {
      id: this.editingId(),
      nombreCompleto: this.formNombre.trim(),
      cedula: this.formCedula.trim() || null,
      telefono: this.formTelefono.trim() || null,
      camionPreferidoId: this.formCamionId,
      isActive: this.formIsActive
    };
    const id = this.editingId();
    const req = id ? this.svc.updateAsistente(id, payload) : this.svc.createAsistente(payload);
    req.subscribe({
      next: (res) => {
        if (res.success) { this.showForm.set(false); this.load(); }
        else this.error.set(res.message);
      },
      error: (e) => this.error.set(e.error?.message ?? 'Error al guardar')
    });
  }

  async delete(a: AsistenteDto): Promise<void> {
    const ok = await this.confirmSvc.ask({
      title: 'Eliminar asistente',
      message: `Vas a eliminar al asistente "${a.nombreCompleto}". Esta acción no se puede deshacer. En el futuro esta acción quedará restringida por roles.`,
      confirmText: 'Eliminar',
      danger: true,
      icon: 'trash'
    });
    if (!ok) return;
    this.svc.deleteAsistente(a.id).subscribe({
      next: (res) => {
        if (res.success) { this.toast.success('Asistente eliminado'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (e) => this.toast.error(e.error?.message ?? 'Error al eliminar')
    });
  }
}
