import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { GoogleLoginRequest, LoginRequest, LoginResponse, ResponseBase } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'yct_acopio_token';
  private readonly USER_KEY = 'yct_acopio_user';
  private readonly USERNAME_KEY = 'yct_acopio_username';
  private readonly REMEMBER_KEY = 'yct_acopio_remember';

  private http = inject(HttpClient);
  private router = inject(Router);

  private _isAuthenticated = signal(this.hasToken());
  private _currentUser = signal<LoginResponse | null>(this.getStoredUser());

  isAuthenticated = this._isAuthenticated.asReadonly();
  currentUser = this._currentUser.asReadonly();
  fullName = computed(() => this._currentUser()?.fullName ?? '');
  role = computed(() => this._currentUser()?.role ?? '');

  /** Username persistido para prefill (no es token). */
  rememberedUsername(): string {
    return localStorage.getItem(this.USERNAME_KEY) ?? '';
  }

  /** Si recuérdame estaba marcado en último login. */
  wasRemembered(): boolean {
    return localStorage.getItem(this.REMEMBER_KEY) === '1';
  }

  login(request: LoginRequest, remember = false): Observable<ResponseBase<LoginResponse>> {
    return this.http.post<ResponseBase<LoginResponse>>(
      `${environment.apiUrl}/api/Auth/login`, request
    ).pipe(tap(res => this.persistOnSuccess(res, remember, request.username)));
  }

  googleLogin(idToken: string, remember = false): Observable<ResponseBase<LoginResponse>> {
    return this.http.post<ResponseBase<LoginResponse>>(
      `${environment.apiUrl}/api/Auth/google`, { idToken } as GoogleLoginRequest
    ).pipe(tap(res => this.persistOnSuccess(res, remember, res?.data?.username)));
  }

  private persistOnSuccess(res: ResponseBase<LoginResponse>, remember: boolean, username?: string): void {
    if (!res.success) return;

    const primary = remember ? localStorage : sessionStorage;
    const other = remember ? sessionStorage : localStorage;

    primary.setItem(this.TOKEN_KEY, res.data.token);
    primary.setItem(this.USER_KEY, JSON.stringify(res.data));

    // Evitar tokens stale en el storage opuesto
    other.removeItem(this.TOKEN_KEY);
    other.removeItem(this.USER_KEY);

    // Username prefill + flag remember (siempre localStorage)
    if (username) localStorage.setItem(this.USERNAME_KEY, username);
    localStorage.setItem(this.REMEMBER_KEY, remember ? '1' : '0');

    this._isAuthenticated.set(true);
    this._currentUser.set(res.data);
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.USER_KEY);
    // Username y remember flag NO se borran (prefill próxima vez)
    this._isAuthenticated.set(false);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  /** Limpia incluso el prefill (útil si user quiere "olvidar" totalmente). */
  forgetMe(): void {
    localStorage.removeItem(this.USERNAME_KEY);
    localStorage.removeItem(this.REMEMBER_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY) ?? sessionStorage.getItem(this.TOKEN_KEY);
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }

  private getStoredUser(): LoginResponse | null {
    const stored = localStorage.getItem(this.USER_KEY) ?? sessionStorage.getItem(this.USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
