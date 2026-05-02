import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DistributorDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';
import { IconComponent } from '../../components/icon/icon.component';

const VEHICLE_TYPES = ['Moto', 'Camioneta', 'Camion', 'Bicicleta', 'Externo'] as const;

@Component({
  selector: 'app-distributors',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './distributors.component.html',
  styleUrl: './distributors.component.scss'
})
export class DistributorsComponent implements OnInit {
  private http = inject(HttpClient);

  distributors = signal<DistributorDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  error = signal('');

  vehicleTypes = VEHICLE_TYPES;

  formName = '';
  formPhone = '';
  formVehicleType = 'Moto';
  formVehiclePlate = '';
  formNotes = '';
  formIsActive = true;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<DistributorDto[]>>(`${environment.apiUrl}/api/Distributors`).subscribe({
      next: (res) => { this.distributors.set(res.data ?? []); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formName = '';
    this.formPhone = '';
    this.formVehicleType = 'Moto';
    this.formVehiclePlate = '';
    this.formNotes = '';
    this.formIsActive = true;
    this.error.set('');
    this.showForm.set(true);
  }

  openEdit(d: DistributorDto): void {
    this.editingId.set(d.id);
    this.formName = d.name;
    this.formPhone = d.phone ?? '';
    this.formVehicleType = d.vehicleType;
    this.formVehiclePlate = d.vehiclePlate ?? '';
    this.formNotes = d.notes ?? '';
    this.formIsActive = d.isActive;
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void { this.showForm.set(false); this.error.set(''); }

  save(): void {
    this.error.set('');
    const payload = {
      id: this.editingId(),
      name: this.formName.trim(),
      phone: this.formPhone.trim() || null,
      vehicleType: this.formVehicleType,
      vehiclePlate: this.formVehiclePlate.trim() || null,
      notes: this.formNotes.trim() || null,
      isActive: this.formIsActive
    };

    const id = this.editingId();
    const req = id
      ? this.http.put<ResponseBase<DistributorDto>>(`${environment.apiUrl}/api/Distributors/${id}`, payload)
      : this.http.post<ResponseBase<DistributorDto>>(`${environment.apiUrl}/api/Distributors`, payload);

    req.subscribe({
      next: (res) => {
        if (res.success) { this.showForm.set(false); this.load(); }
        else this.error.set(res.message);
      },
      error: (e) => this.error.set(e.error?.message ?? 'Error al guardar')
    });
  }

  delete(d: DistributorDto): void {
    if (!confirm(`¿Desactivar a ${d.name}?\nLos pedidos asignados quedan tal como están.`)) return;
    this.http.delete<ResponseBase<boolean>>(`${environment.apiUrl}/api/Distributors/${d.id}`).subscribe({
      next: () => this.load()
    });
  }

  vehicleClass(type: string): string {
    return ({
      'Moto': 'moto', 'Camioneta': 'pickup', 'Camion': 'truck', 'Bicicleta': 'bike', 'Externo': 'external'
    } as Record<string, string>)[type] ?? 'moto';
  }
}
