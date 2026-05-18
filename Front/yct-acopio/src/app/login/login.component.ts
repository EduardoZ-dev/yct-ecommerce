import { Component, ElementRef, NgZone, ViewChild, AfterViewInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { IconComponent } from '../components/icon/icon.component';
import { environment } from '../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements AfterViewInit {
  private auth = inject(AuthService);
  private router = inject(Router);
  private zone = inject(NgZone);

  @ViewChild('googleBtn') googleBtn?: ElementRef<HTMLDivElement>;

  username = this.auth.rememberedUsername();
  password = '';
  remember = signal(this.auth.wasRemembered());
  showPassword = signal(false);
  loading = signal(false);
  error = signal('');

  toggleRemember(): void { this.remember.update(v => !v); }

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
    if (this.googleBtn) {
      google.accounts.id.renderButton(this.googleBtn.nativeElement, {
        theme: 'outline',
        size: 'large',
        text: 'continue_with',
        shape: 'rectangular',
        logo_alignment: 'left',
        width: 320
      });
    }
  }

  private handleGoogleCredential(idToken: string): void {
    this.error.set('');
    this.loading.set(true);
    this.auth.googleLogin(idToken, this.remember()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) this.router.navigate(['/']);
        else this.error.set(res.message || 'Error al iniciar con Google');
      },
      error: (e) => {
        this.loading.set(false);
        this.error.set(e.error?.message || 'Error al iniciar con Google');
      }
    });
  }

  submit(): void {
    this.error.set('');
    if (!this.username.trim() || !this.password) {
      this.error.set('Ingresa usuario y contraseña');
      return;
    }
    this.loading.set(true);
    this.auth.login({ username: this.username.trim(), password: this.password }, this.remember()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) this.router.navigate(['/']);
        else this.error.set(res.message || 'Credenciales inválidas');
      },
      error: (e) => {
        this.loading.set(false);
        this.error.set(e.error?.message ?? 'Error al iniciar sesión');
      }
    });
  }
}
