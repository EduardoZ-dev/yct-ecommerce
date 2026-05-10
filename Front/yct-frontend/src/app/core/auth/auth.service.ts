import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { GoogleLoginRequest, LoginRequest, LoginResponse, RegisterRequest, ResponseBase, UserDto } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'yct_token';
  private readonly USER_KEY = 'yct_user';

  private _isAuthenticated = signal(this.hasToken());
  private _currentUser = signal<LoginResponse | null>(this.getStoredUser());

  isAuthenticated = this._isAuthenticated.asReadonly();
  currentUser = this._currentUser.asReadonly();
  fullName = computed(() => this._currentUser()?.fullName ?? '');
  role = computed(() => this._currentUser()?.role ?? '');

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<ResponseBase<LoginResponse>> {
    return this.http.post<ResponseBase<LoginResponse>>(
      `${environment.apiUrl}/api/Auth/login`, request
    ).pipe(
      tap(res => {
        if (res.success) {
          localStorage.setItem(this.TOKEN_KEY, res.data.token);
          localStorage.setItem(this.USER_KEY, JSON.stringify(res.data));
          this._isAuthenticated.set(true);
          this._currentUser.set(res.data);
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<ResponseBase<UserDto>> {
    return this.http.post<ResponseBase<UserDto>>(
      `${environment.apiUrl}/api/Auth/register`, request
    );
  }

  googleLogin(idToken: string): Observable<ResponseBase<LoginResponse>> {
    return this.http.post<ResponseBase<LoginResponse>>(
      `${environment.apiUrl}/api/Auth/google`, { idToken } as GoogleLoginRequest
    ).pipe(
      tap(res => {
        if (res.success) {
          localStorage.setItem(this.TOKEN_KEY, res.data.token);
          localStorage.setItem(this.USER_KEY, JSON.stringify(res.data));
          this._isAuthenticated.set(true);
          this._currentUser.set(res.data);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._isAuthenticated.set(false);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  private getStoredUser(): LoginResponse | null {
    const stored = localStorage.getItem(this.USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
