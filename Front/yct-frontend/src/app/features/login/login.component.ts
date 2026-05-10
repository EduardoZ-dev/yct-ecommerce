import { AfterViewInit, Component, ElementRef, NgZone, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { IconComponent } from '../../components/icon/icon.component';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, IconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements AfterViewInit {
  isRegister = signal(false);
  loading = signal(false);
  error = signal('');
  showPassword = signal(false);

  username = '';
  password = '';
  fullName = '';
  email = '';
  phone = '';

  @ViewChild('googleBtnIn') googleBtnIn?: ElementRef<HTMLDivElement>;
  @ViewChild('googleBtnUp') googleBtnUp?: ElementRef<HTMLDivElement>;

  constructor(private authService: AuthService, private router: Router, private zone: NgZone) {}

  ngAfterViewInit(): void {
    this.initGoogle();
  }

  private initGoogle(retries = 20): void {
    if (typeof google === 'undefined' || !google?.accounts?.id) {
      if (retries > 0) setTimeout(() => this.initGoogle(retries - 1), 150);
      return;
    }

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (resp: any) => this.zone.run(() => this.handleGoogleCredential(resp.credential))
    });

    const opts = { theme: 'outline', size: 'large', width: 320, text: 'continue_with', shape: 'pill' };
    if (this.googleBtnIn) google.accounts.id.renderButton(this.googleBtnIn.nativeElement, opts);
    if (this.googleBtnUp) google.accounts.id.renderButton(this.googleBtnUp.nativeElement, opts);
  }

  private handleGoogleCredential(idToken: string): void {
    this.error.set('');
    this.loading.set(true);
    this.authService.googleLogin(idToken).subscribe({
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
        this.error.set(err.error?.message || 'Error al iniciar con Google');
      }
    });
  }

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
