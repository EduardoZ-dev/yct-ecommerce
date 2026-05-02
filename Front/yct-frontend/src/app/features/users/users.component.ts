import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ResponseBase, UserDto } from '../../core/models';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment';
import { IconComponent } from '../../components/icon/icon.component';

const ALL_ROLES = ['SuperAdmin', 'Admin', 'Employee', 'Customer'] as const;

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private http = inject(HttpClient);
  auth = inject(AuthService);

  users = signal<UserDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  error = signal('');

  formUsername = '';
  formFullName = '';
  formEmail = '';
  formPhone = '';
  formRole = 'Employee';
  formPassword = '';
  formIsActive = true;

  // Solo SuperAdmin puede crear/asignar otros SuperAdmin
  availableRoles = computed(() =>
    this.auth.role() === 'SuperAdmin'
      ? [...ALL_ROLES]
      : ALL_ROLES.filter(r => r !== 'SuperAdmin')
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<UserDto[]>>(`${environment.apiUrl}/api/Users`).subscribe({
      next: (res) => {
        this.users.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formUsername = '';
    this.formFullName = '';
    this.formEmail = '';
    this.formPhone = '';
    this.formRole = 'Employee';
    this.formPassword = '';
    this.formIsActive = true;
    this.error.set('');
    this.showForm.set(true);
  }

  openEdit(u: UserDto): void {
    this.editingId.set(u.id);
    this.formUsername = u.username;
    this.formFullName = u.fullName;
    this.formEmail = u.email ?? '';
    this.formPhone = u.phone ?? '';
    this.formRole = u.role;
    this.formPassword = '';
    this.formIsActive = u.isActive;
    this.error.set('');
    this.showForm.set(true);
  }

  cancel(): void {
    this.showForm.set(false);
    this.error.set('');
  }

  save(): void {
    this.error.set('');
    const id = this.editingId();
    if (id) {
      this.http.put<ResponseBase<UserDto>>(`${environment.apiUrl}/api/Users/${id}`, {
        id,
        fullName: this.formFullName,
        email: this.formEmail || null,
        phone: this.formPhone || null,
        role: this.formRole,
        isActive: this.formIsActive,
        newPassword: this.formPassword || null
      }).subscribe({
        next: (res) => {
          if (res.success) { this.showForm.set(false); this.load(); }
          else this.error.set(res.message);
        },
        error: (e) => this.error.set(e.error?.message ?? 'Error al actualizar')
      });
    } else {
      this.http.post<ResponseBase<UserDto>>(`${environment.apiUrl}/api/Users`, {
        username: this.formUsername,
        password: this.formPassword,
        fullName: this.formFullName,
        email: this.formEmail || null,
        phone: this.formPhone || null,
        role: this.formRole
      }).subscribe({
        next: (res) => {
          if (res.success) { this.showForm.set(false); this.load(); }
          else this.error.set(res.message);
        },
        error: (e) => this.error.set(e.error?.message ?? 'Error al crear')
      });
    }
  }

  delete(u: UserDto): void {
    if (!confirm(`¿Desactivar a ${u.username}?`)) return;
    this.http.delete<ResponseBase<boolean>>(`${environment.apiUrl}/api/Users/${u.id}`).subscribe({
      next: () => this.load()
    });
  }

  roleClass(role: string): string {
    return {
      'SuperAdmin': 'role-super',
      'Admin': 'role-admin',
      'Employee': 'role-employee',
      'Customer': 'role-customer'
    }[role] ?? '';
  }

  roleLabel(role: string): string {
    return {
      'SuperAdmin': 'Super Admin',
      'Admin': 'Administrador',
      'Employee': 'Empleado',
      'Customer': 'Cliente'
    }[role] ?? role;
  }
}
