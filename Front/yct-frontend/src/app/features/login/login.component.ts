import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { IconComponent } from '../../components/icon/icon.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, IconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  isRegister = signal(false);
  loading = signal(false);
  error = signal('');
  showPassword = signal(false);

  username = '';
  password = '';
  fullName = '';
  email = '';
  phone = '';

  constructor(private authService: AuthService, private router: Router) {}

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  onSubmit(): void {
    this.error.set('');
    this.loading.set(true);

    if (this.isRegister()) {
      this.authService.register({
        username: this.username,
        password: this.password,
        fullName: this.fullName,
        email: this.email || undefined,
        phone: this.phone || undefined
      }).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success) {
            this.isRegister.set(false);
            this.error.set('');
          } else {
            this.error.set(res.message);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.error?.message || 'Error en el registro');
        }
      });
    } else {
      this.authService.login({ username: this.username, password: this.password }).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success) {
            const adminRoles = ['SuperAdmin', 'Admin', 'Employee'];
            const dest = adminRoles.includes(res.data.role) ? '/admin' : '/shop';
            this.router.navigate([dest]);
          } else {
            this.error.set(res.message);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.error?.message || 'Error al iniciar sesión');
        }
      });
    }
  }

  toggleMode(): void {
    this.isRegister.update(v => !v);
    this.error.set('');
    this.password = '';
    this.showPassword.set(false);
  }
}
